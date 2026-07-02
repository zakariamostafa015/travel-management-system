using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelToursWebsite.Api.Common;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Application.Features.Contact;

namespace TravelToursWebsite.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/contact")]
public sealed class ContactController(
    IContactApplicationService contactService,
    IBookingApplicationService bookingService)
    : ControllerBase
{
    [HttpPost("inquiries")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<ContactInquiryDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateInquiry(
        [FromBody] CreateContactInquiryRequest request,
        CancellationToken cancellationToken)
    {
        var result = await contactService.CreateInquiryAsync(request, cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return BadRequest(ApiResponse<object>.Fail(result.Message ?? "Inquiry could not be submitted.", traceId: HttpContext.TraceIdentifier));
        }

        return CreatedAtAction(
            nameof(GetInquiryById),
            new { id = result.Value.Id, version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0" },
            ApiResponse<ContactInquiryDto>.Ok(result.Value, result.Message, HttpContext.TraceIdentifier));
    }

    [HttpGet("inquiries")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<ContactInquiryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetInquiries(
        [FromQuery] ContactInquiryQuery query,
        CancellationToken cancellationToken)
    {
        var result = await contactService.GetInquiriesAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<ContactInquiryDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("inquiries/{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<ContactInquiryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetInquiryById(int id, CancellationToken cancellationToken)
    {
        var inquiry = await contactService.GetInquiryByIdAsync(id, cancellationToken);
        if (inquiry is null)
        {
            return NotFound(ApiResponse<object>.Fail("Contact inquiry was not found.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<ContactInquiryDto>.Ok(inquiry, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPatch("inquiries/{id:int}/status")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<ContactInquiryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateInquiryStatus(
        int id,
        [FromBody] UpdateContactInquiryStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Id != id)
        {
            return BadRequest(ApiResponse<object>.Fail("Route id must match request id.", traceId: HttpContext.TraceIdentifier));
        }

        var result = await contactService.UpdateInquiryStatusAsync(request, cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return NotFound(ApiResponse<object>.Fail(result.Message ?? "Contact inquiry was not found.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<ContactInquiryDto>.Ok(result.Value, result.Message, HttpContext.TraceIdentifier));
    }

    [HttpDelete("inquiries/{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteInquiry(int id, CancellationToken cancellationToken)
    {
        var result = await contactService.DeleteInquiryAsync(id, cancellationToken);
        if (!result.Succeeded)
        {
            return NotFound(ApiResponse<object>.Fail(result.Message ?? "Contact inquiry was not found.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse.Ok(result.Message, HttpContext.TraceIdentifier));
    }

    [HttpPost("bookings")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<BookingRequestDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateBooking(
        [FromBody] CreateBookingRequest request,
        CancellationToken cancellationToken)
    {
        var result = await bookingService.CreateBookingAsync(request, cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return BadRequest(ApiResponse<object>.Fail(result.Message ?? "Booking request could not be submitted.", traceId: HttpContext.TraceIdentifier));
        }

        return CreatedAtAction(
            nameof(GetBookingById),
            new { id = result.Value.Id, version = HttpContext.GetRequestedApiVersion()?.ToString() ?? "1.0" },
            ApiResponse<BookingRequestDto>.Ok(result.Value, result.Message, HttpContext.TraceIdentifier));
    }

    [HttpGet("bookings")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BookingRequestDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBookings(
        [FromQuery] BookingRequestQuery query,
        CancellationToken cancellationToken)
    {
        var result = await bookingService.GetBookingsAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<BookingRequestDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("bookings/{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<BookingRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetBookingById(int id, CancellationToken cancellationToken)
    {
        var booking = await bookingService.GetBookingByIdAsync(id, cancellationToken);
        if (booking is null)
        {
            return NotFound(ApiResponse<object>.Fail("Booking request was not found.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<BookingRequestDto>.Ok(booking, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPatch("bookings/{id:int}/status")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse<BookingRequestDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBookingStatus(
        int id,
        [FromBody] UpdateBookingRequestStatusRequest request,
        CancellationToken cancellationToken)
    {
        if (request.Id != id)
        {
            return BadRequest(ApiResponse<object>.Fail("Route id must match request id.", traceId: HttpContext.TraceIdentifier));
        }

        var result = await bookingService.UpdateBookingStatusAsync(request, cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return NotFound(ApiResponse<object>.Fail(result.Message ?? "Booking request was not found.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<BookingRequestDto>.Ok(result.Value, result.Message, HttpContext.TraceIdentifier));
    }

    [HttpDelete("bookings/{id:int}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBooking(int id, CancellationToken cancellationToken)
    {
        var result = await bookingService.DeleteBookingAsync(id, cancellationToken);
        if (!result.Succeeded)
        {
            return NotFound(ApiResponse<object>.Fail(result.Message ?? "Booking request was not found.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse.Ok(result.Message, HttpContext.TraceIdentifier));
    }
}
