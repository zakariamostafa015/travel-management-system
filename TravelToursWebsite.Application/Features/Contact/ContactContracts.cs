using FluentValidation;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Domain.Entities;
using TravelToursWebsite.Domain.Enums;

namespace TravelToursWebsite.Application.Features.Contact;

public sealed record ContactInquiryQuery : PagedQuery
{
    public InquiryStatus? Status { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
}

public sealed record BookingRequestQuery : PagedQuery
{
    public BookingStatus? Status { get; init; }
    public int? TourId { get; init; }
    public DateTime? CreatedFrom { get; init; }
    public DateTime? CreatedTo { get; init; }
}

public sealed record ContactInquiryDto(
    int Id,
    string Name,
    string Email,
    string? Phone,
    string Subject,
    string Message,
    InquiryStatus Status,
    DateTime CreatedDate,
    DateTime? RespondedDate,
    string? AdminNotes);

public sealed record BookingRequestDto(
    int Id,
    int TourId,
    string? TourTitle,
    string Name,
    string Email,
    string Phone,
    int NumberOfTravelers,
    DateTime PreferredTravelDate,
    string? SpecialRequests,
    BookingStatus Status,
    DateTime CreatedDate,
    DateTime? ProcessedDate,
    string? AdminNotes,
    decimal? EstimatedTotal);

public sealed record CreateContactInquiryRequest(
    string Name,
    string Email,
    string? Phone,
    string Subject,
    string Message);

public sealed record UpdateContactInquiryStatusRequest(
    int Id,
    InquiryStatus Status,
    string? AdminNotes);

public sealed record CreateBookingRequest(
    int TourId,
    string Name,
    string Email,
    string Phone,
    int NumberOfTravelers,
    DateTime PreferredTravelDate,
    string? SpecialRequests);

public sealed record UpdateBookingRequestStatusRequest(
    int Id,
    BookingStatus Status,
    string? AdminNotes);

public interface IContactApplicationService
{
    Task<OperationResult<ContactInquiryDto>> CreateInquiryAsync(CreateContactInquiryRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<ContactInquiryDto>> GetInquiriesAsync(ContactInquiryQuery query, CancellationToken cancellationToken = default);
    Task<ContactInquiryDto?> GetInquiryByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult<ContactInquiryDto>> UpdateInquiryStatusAsync(UpdateContactInquiryStatusRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteInquiryAsync(int id, CancellationToken cancellationToken = default);
}

public interface IBookingApplicationService
{
    Task<OperationResult<BookingRequestDto>> CreateBookingAsync(CreateBookingRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<BookingRequestDto>> GetBookingsAsync(BookingRequestQuery query, CancellationToken cancellationToken = default);
    Task<BookingRequestDto?> GetBookingByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult<BookingRequestDto>> UpdateBookingStatusAsync(UpdateBookingRequestStatusRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteBookingAsync(int id, CancellationToken cancellationToken = default);
}

public static class ContactMappingExtensions
{
    public static ContactInquiryDto ToDto(this ContactInquiry inquiry)
    {
        return new ContactInquiryDto(
            inquiry.Id,
            inquiry.Name,
            inquiry.Email,
            inquiry.Phone,
            inquiry.Subject,
            inquiry.Message,
            inquiry.Status,
            inquiry.CreatedDate,
            inquiry.RespondedDate,
            inquiry.AdminNotes);
    }

    public static BookingRequestDto ToDto(this BookingRequest booking, string language = "en")
    {
        var translation = booking.Tour?.Translations.FirstOrDefault(item => item.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
            ?? booking.Tour?.Translations.FirstOrDefault(item => item.Language.Equals("en", StringComparison.OrdinalIgnoreCase))
            ?? booking.Tour?.Translations.FirstOrDefault();

        return new BookingRequestDto(
            booking.Id,
            booking.TourId,
            translation?.Title,
            booking.Name,
            booking.Email,
            booking.Phone,
            booking.NumberOfTravelers,
            booking.PreferredTravelDate,
            booking.SpecialRequests,
            booking.Status,
            booking.CreatedDate,
            booking.ProcessedDate,
            booking.AdminNotes,
            booking.EstimatedTotal);
    }
}

public sealed class ContactInquiryQueryValidator : PagedQueryValidator<ContactInquiryQuery>
{
    public ContactInquiryQueryValidator()
    {
        RuleFor(query => query.CreatedTo)
            .GreaterThanOrEqualTo(query => query.CreatedFrom)
            .When(query => query.CreatedFrom.HasValue && query.CreatedTo.HasValue);
    }
}

public sealed class BookingRequestQueryValidator : PagedQueryValidator<BookingRequestQuery>
{
    public BookingRequestQueryValidator()
    {
        RuleFor(query => query.TourId).GreaterThan(0).When(query => query.TourId.HasValue);
        RuleFor(query => query.CreatedTo)
            .GreaterThanOrEqualTo(query => query.CreatedFrom)
            .When(query => query.CreatedFrom.HasValue && query.CreatedTo.HasValue);
    }
}

public sealed class CreateContactInquiryRequestValidator : AbstractValidator<CreateContactInquiryRequest>
{
    public CreateContactInquiryRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(request => request.Phone).MaximumLength(20);
        RuleFor(request => request.Subject).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Message).NotEmpty();
    }
}

public sealed class UpdateContactInquiryStatusRequestValidator : AbstractValidator<UpdateContactInquiryStatusRequest>
{
    public UpdateContactInquiryStatusRequestValidator()
    {
        RuleFor(request => request.Id).GreaterThan(0);
        RuleFor(request => request.AdminNotes).MaximumLength(500);
    }
}

public sealed class CreateBookingRequestValidator : AbstractValidator<CreateBookingRequest>
{
    public CreateBookingRequestValidator()
    {
        RuleFor(request => request.TourId).GreaterThan(0);
        RuleFor(request => request.Name).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Email).NotEmpty().EmailAddress().MaximumLength(200);
        RuleFor(request => request.Phone).NotEmpty().MaximumLength(20);
        RuleFor(request => request.NumberOfTravelers).GreaterThanOrEqualTo(1);
        RuleFor(request => request.PreferredTravelDate).GreaterThan(DateTime.UtcNow.Date.AddDays(-1));
        RuleFor(request => request.SpecialRequests).MaximumLength(200);
    }
}

public sealed class UpdateBookingRequestStatusRequestValidator : AbstractValidator<UpdateBookingRequestStatusRequest>
{
    public UpdateBookingRequestStatusRequestValidator()
    {
        RuleFor(request => request.Id).GreaterThan(0);
        RuleFor(request => request.AdminNotes).MaximumLength(500);
    }
}