using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Application.Features.Contact;
using TravelToursWebsite.Domain.Entities;
using TravelToursWebsite.Domain.Enums;
using TravelToursWebsite.Infrastructure.Persistence;

namespace TravelToursWebsite.Infrastructure.Contact;

public sealed class ContactApplicationService(
    ApplicationDbContext context,
    IContactNotificationService notificationService,
    ILogger<ContactApplicationService> logger)
    : IContactApplicationService, IBookingApplicationService
{
    private const int MaxPageSize = 100;

    public async Task<OperationResult<ContactInquiryDto>> CreateInquiryAsync(
        CreateContactInquiryRequest request,
        CancellationToken cancellationToken = default)
    {
        var inquiry = new ContactInquiry
        {
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Phone = NormalizeOptional(request.Phone),
            Subject = request.Subject.Trim(),
            Message = request.Message.Trim(),
            Status = InquiryStatus.New,
            CreatedDate = DateTime.UtcNow
        };

        context.ContactInquiries.Add(inquiry);
        await context.SaveChangesAsync(cancellationToken);
        await TrySendContactInquiryEmailAsync(inquiry, cancellationToken);

        return OperationResult<ContactInquiryDto>.Success(inquiry.ToDto(), "Inquiry received.");
    }

    public async Task<PagedResult<ContactInquiryDto>> GetInquiriesAsync(
        ContactInquiryQuery query,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = NormalizePageNumber(query.PageNumber);
        var pageSize = NormalizePageSize(query.PageSize);
        var inquiries = context.ContactInquiries.AsNoTracking();

        if (query.Status.HasValue)
        {
            inquiries = inquiries.Where(inquiry => inquiry.Status == query.Status.Value);
        }

        if (query.CreatedFrom.HasValue)
        {
            inquiries = inquiries.Where(inquiry => inquiry.CreatedDate >= query.CreatedFrom.Value);
        }

        if (query.CreatedTo.HasValue)
        {
            inquiries = inquiries.Where(inquiry => inquiry.CreatedDate <= query.CreatedTo.Value);
        }

        inquiries = ApplyInquirySearch(inquiries, query.SearchTerm);
        var totalCount = await inquiries.CountAsync(cancellationToken);
        if (totalCount == 0)
        {
            return PagedResult<ContactInquiryDto>.Empty(pageNumber, pageSize);
        }

        var items = await ApplyInquirySorting(inquiries, query.SortBy, query.SortDirection)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ContactInquiryDto>(
            items.Select(inquiry => inquiry.ToDto()).ToArray(),
            pageNumber,
            pageSize,
            totalCount);
    }

    public async Task<ContactInquiryDto?> GetInquiryByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var inquiry = await context.ContactInquiries
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return inquiry?.ToDto();
    }

    public async Task<OperationResult<ContactInquiryDto>> UpdateInquiryStatusAsync(
        UpdateContactInquiryStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var inquiry = await context.ContactInquiries
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);

        if (inquiry is null)
        {
            return OperationResult<ContactInquiryDto>.Failure("Contact inquiry was not found.");
        }

        inquiry.Status = request.Status;
        inquiry.AdminNotes = NormalizeOptional(request.AdminNotes);
        inquiry.RespondedDate = request.Status == InquiryStatus.Responded
            ? DateTime.UtcNow
            : request.Status == InquiryStatus.New
                ? null
                : inquiry.RespondedDate;

        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<ContactInquiryDto>.Success(inquiry.ToDto(), "Inquiry status updated.");
    }

    public async Task<OperationResult> DeleteInquiryAsync(int id, CancellationToken cancellationToken = default)
    {
        var inquiry = await context.ContactInquiries
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (inquiry is null)
        {
            return OperationResult.Failure("Contact inquiry was not found.");
        }

        context.ContactInquiries.Remove(inquiry);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Contact inquiry deleted.");
    }

    public async Task<OperationResult<BookingRequestDto>> CreateBookingAsync(
        CreateBookingRequest request,
        CancellationToken cancellationToken = default)
    {
        var tour = await context.Tours
            .Include(item => item.Translations)
            .FirstOrDefaultAsync(item => item.Id == request.TourId && item.IsActive, cancellationToken);

        if (tour is null)
        {
            return OperationResult<BookingRequestDto>.Failure("Selected tour was not found.");
        }

        var booking = new BookingRequest
        {
            TourId = tour.Id,
            Tour = tour,
            Name = request.Name.Trim(),
            Email = request.Email.Trim(),
            Phone = request.Phone.Trim(),
            NumberOfTravelers = request.NumberOfTravelers,
            PreferredTravelDate = request.PreferredTravelDate.Date,
            SpecialRequests = NormalizeOptional(request.SpecialRequests),
            Status = BookingStatus.Pending,
            CreatedDate = DateTime.UtcNow,
            EstimatedTotal = tour.Price * request.NumberOfTravelers
        };

        context.BookingRequests.Add(booking);
        await context.SaveChangesAsync(cancellationToken);
        await TrySendBookingEmailAsync(booking, cancellationToken);

        return OperationResult<BookingRequestDto>.Success(booking.ToDto(), "Booking request received.");
    }

    public async Task<PagedResult<BookingRequestDto>> GetBookingsAsync(
        BookingRequestQuery query,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = NormalizePageNumber(query.PageNumber);
        var pageSize = NormalizePageSize(query.PageSize);
        var bookings = context.BookingRequests
            .AsNoTracking()
            .Include(booking => booking.Tour)
                .ThenInclude(tour => tour!.Translations)
            .AsQueryable();

        if (query.Status.HasValue)
        {
            bookings = bookings.Where(booking => booking.Status == query.Status.Value);
        }

        if (query.TourId.HasValue)
        {
            bookings = bookings.Where(booking => booking.TourId == query.TourId.Value);
        }

        if (query.CreatedFrom.HasValue)
        {
            bookings = bookings.Where(booking => booking.CreatedDate >= query.CreatedFrom.Value);
        }

        if (query.CreatedTo.HasValue)
        {
            bookings = bookings.Where(booking => booking.CreatedDate <= query.CreatedTo.Value);
        }

        bookings = ApplyBookingSearch(bookings, query.SearchTerm);
        var totalCount = await bookings.CountAsync(cancellationToken);
        if (totalCount == 0)
        {
            return PagedResult<BookingRequestDto>.Empty(pageNumber, pageSize);
        }

        var items = await ApplyBookingSorting(bookings, query.SortBy, query.SortDirection)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<BookingRequestDto>(
            items.Select(booking => booking.ToDto()).ToArray(),
            pageNumber,
            pageSize,
            totalCount);
    }

    public async Task<BookingRequestDto?> GetBookingByIdAsync(
        int id,
        CancellationToken cancellationToken = default)
    {
        var booking = await context.BookingRequests
            .AsNoTracking()
            .Include(item => item.Tour)
                .ThenInclude(tour => tour!.Translations)
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        return booking?.ToDto();
    }

    public async Task<OperationResult<BookingRequestDto>> UpdateBookingStatusAsync(
        UpdateBookingRequestStatusRequest request,
        CancellationToken cancellationToken = default)
    {
        var booking = await context.BookingRequests
            .Include(item => item.Tour)
                .ThenInclude(tour => tour!.Translations)
            .FirstOrDefaultAsync(item => item.Id == request.Id, cancellationToken);

        if (booking is null)
        {
            return OperationResult<BookingRequestDto>.Failure("Booking request was not found.");
        }

        booking.Status = request.Status;
        booking.AdminNotes = NormalizeOptional(request.AdminNotes);
        booking.ProcessedDate = request.Status is BookingStatus.Confirmed or BookingStatus.InProgress or BookingStatus.Completed
            ? DateTime.UtcNow
            : request.Status == BookingStatus.Pending
                ? null
                : booking.ProcessedDate;

        await context.SaveChangesAsync(cancellationToken);
        return OperationResult<BookingRequestDto>.Success(booking.ToDto(), "Booking status updated.");
    }

    public async Task<OperationResult> DeleteBookingAsync(int id, CancellationToken cancellationToken = default)
    {
        var booking = await context.BookingRequests
            .FirstOrDefaultAsync(item => item.Id == id, cancellationToken);

        if (booking is null)
        {
            return OperationResult.Failure("Booking request was not found.");
        }

        context.BookingRequests.Remove(booking);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Booking request deleted.");
    }

    private async Task TrySendContactInquiryEmailAsync(ContactInquiry inquiry, CancellationToken cancellationToken)
    {
        try
        {
            await notificationService.SendContactInquiryConfirmationAsync(inquiry, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Contact inquiry {InquiryId} was saved but confirmation email failed.", inquiry.Id);
        }
    }

    private async Task TrySendBookingEmailAsync(BookingRequest booking, CancellationToken cancellationToken)
    {
        try
        {
            await notificationService.SendBookingRequestConfirmationAsync(booking, cancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogWarning(exception, "Booking request {BookingId} was saved but confirmation email failed.", booking.Id);
        }
    }

    private static IQueryable<ContactInquiry> ApplyInquirySearch(IQueryable<ContactInquiry> inquiries, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return inquiries;
        }

        var term = searchTerm.Trim();
        return inquiries.Where(inquiry =>
            inquiry.Name.Contains(term)
            || inquiry.Email.Contains(term)
            || inquiry.Subject.Contains(term)
            || inquiry.Message.Contains(term)
            || inquiry.Phone != null && inquiry.Phone.Contains(term));
    }

    private static IQueryable<BookingRequest> ApplyBookingSearch(IQueryable<BookingRequest> bookings, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return bookings;
        }

        var term = searchTerm.Trim();
        return bookings.Where(booking =>
            booking.Name.Contains(term)
            || booking.Email.Contains(term)
            || booking.Phone.Contains(term)
            || booking.SpecialRequests != null && booking.SpecialRequests.Contains(term)
            || booking.Tour != null && booking.Tour.Translations.Any(translation => translation.Title.Contains(term)));
    }

    private static IQueryable<ContactInquiry> ApplyInquirySorting(
        IQueryable<ContactInquiry> inquiries,
        string? sortBy,
        SortDirection sortDirection)
    {
        var descending = sortDirection == SortDirection.Descending;
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "name" => descending
                ? inquiries.OrderByDescending(inquiry => inquiry.Name).ThenByDescending(inquiry => inquiry.Id)
                : inquiries.OrderBy(inquiry => inquiry.Name).ThenBy(inquiry => inquiry.Id),
            "email" => descending
                ? inquiries.OrderByDescending(inquiry => inquiry.Email).ThenByDescending(inquiry => inquiry.Id)
                : inquiries.OrderBy(inquiry => inquiry.Email).ThenBy(inquiry => inquiry.Id),
            "status" => descending
                ? inquiries.OrderByDescending(inquiry => inquiry.Status).ThenByDescending(inquiry => inquiry.Id)
                : inquiries.OrderBy(inquiry => inquiry.Status).ThenBy(inquiry => inquiry.Id),
            null or "" => inquiries.OrderByDescending(inquiry => inquiry.CreatedDate).ThenByDescending(inquiry => inquiry.Id),
            _ => descending
                ? inquiries.OrderByDescending(inquiry => inquiry.CreatedDate).ThenByDescending(inquiry => inquiry.Id)
                : inquiries.OrderBy(inquiry => inquiry.CreatedDate).ThenBy(inquiry => inquiry.Id)
        };
    }

    private static IQueryable<BookingRequest> ApplyBookingSorting(
        IQueryable<BookingRequest> bookings,
        string? sortBy,
        SortDirection sortDirection)
    {
        var descending = sortDirection == SortDirection.Descending;
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "name" => descending
                ? bookings.OrderByDescending(booking => booking.Name).ThenByDescending(booking => booking.Id)
                : bookings.OrderBy(booking => booking.Name).ThenBy(booking => booking.Id),
            "traveldate" => descending
                ? bookings.OrderByDescending(booking => booking.PreferredTravelDate).ThenByDescending(booking => booking.Id)
                : bookings.OrderBy(booking => booking.PreferredTravelDate).ThenBy(booking => booking.Id),
            "status" => descending
                ? bookings.OrderByDescending(booking => booking.Status).ThenByDescending(booking => booking.Id)
                : bookings.OrderBy(booking => booking.Status).ThenBy(booking => booking.Id),
            "estimatedtotal" => descending
                ? bookings.OrderByDescending(booking => booking.EstimatedTotal).ThenByDescending(booking => booking.Id)
                : bookings.OrderBy(booking => booking.EstimatedTotal).ThenBy(booking => booking.Id),
            null or "" => bookings.OrderByDescending(booking => booking.CreatedDate).ThenByDescending(booking => booking.Id),
            _ => descending
                ? bookings.OrderByDescending(booking => booking.CreatedDate).ThenByDescending(booking => booking.Id)
                : bookings.OrderBy(booking => booking.CreatedDate).ThenBy(booking => booking.Id)
        };
    }

    private static int NormalizePageNumber(int pageNumber)
    {
        return Math.Max(1, pageNumber);
    }

    private static int NormalizePageSize(int pageSize)
    {
        return Math.Clamp(pageSize, 1, MaxPageSize);
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
