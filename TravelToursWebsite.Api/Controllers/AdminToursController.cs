using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelToursWebsite.Api.Common;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Application.Features.AdminContent;
using TravelToursWebsite.Application.Features.Tours;

namespace TravelToursWebsite.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize(Policy = "ContentManager")]
[Route("api/v{version:apiVersion}/admin/tours")]
public sealed class AdminToursController(
    ITourManagementService tourManagementService,
    IAdminTourContentService adminTourContentService)
    : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreateTour([FromBody] CreateTourRequest request, CancellationToken cancellationToken)
    {
        return ToCreatedOrBadRequest(await tourManagementService.CreateTourAsync(request, cancellationToken), nameof(CreateTour));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateTour(int id, [FromBody] UpdateTourRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            return BadRequest(ApiResponse<object>.Fail("Route id must match request id.", traceId: HttpContext.TraceIdentifier));
        }

        return ToOkOrBadRequest(await tourManagementService.UpdateTourAsync(request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteTour(int id, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await tourManagementService.DeleteTourAsync(id, cancellationToken));
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateTourCategoryRequest request, CancellationToken cancellationToken)
    {
        return ToCreatedOrBadRequest(await tourManagementService.CreateCategoryAsync(request, cancellationToken), nameof(CreateCategory));
    }

    [HttpPut("categories/{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateTourCategoryRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            return BadRequest(ApiResponse<object>.Fail("Route id must match request id.", traceId: HttpContext.TraceIdentifier));
        }

        return ToOkOrBadRequest(await tourManagementService.UpdateCategoryAsync(request, cancellationToken));
    }

    [HttpDelete("categories/{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await tourManagementService.DeleteCategoryAsync(id, cancellationToken));
    }

    [HttpPost("images")]
    public async Task<IActionResult> AddImage([FromBody] TourImageRequest request, CancellationToken cancellationToken)
    {
        return ToCreatedOrBadRequest(await adminTourContentService.AddTourImageAsync(request, cancellationToken), nameof(AddImage));
    }

    [HttpPut("images/{id:int}")]
    public async Task<IActionResult> UpdateImage(int id, [FromBody] UpdateTourImageRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            return BadRequest(ApiResponse<object>.Fail("Route id must match request id.", traceId: HttpContext.TraceIdentifier));
        }

        return ToOkOrBadRequest(await adminTourContentService.UpdateTourImageAsync(request, cancellationToken));
    }

    [HttpDelete("images/{id:int}")]
    public async Task<IActionResult> DeleteImage(int id, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await adminTourContentService.DeleteTourImageAsync(id, cancellationToken));
    }

    [HttpPost("itineraries")]
    public async Task<IActionResult> UpsertItinerary([FromBody] UpsertTourItineraryRequest request, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await adminTourContentService.UpsertItineraryAsync(request, cancellationToken));
    }

    [HttpDelete("itineraries/{id:int}")]
    public async Task<IActionResult> DeleteItinerary(int id, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await adminTourContentService.DeleteItineraryAsync(id, cancellationToken));
    }

    [HttpPost("spots")]
    public async Task<IActionResult> UpsertSpot([FromBody] UpsertTourSpotRequest request, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await adminTourContentService.UpsertSpotAsync(request, cancellationToken));
    }

    [HttpDelete("spots/{id:int}")]
    public async Task<IActionResult> DeleteSpot(int id, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await adminTourContentService.DeleteSpotAsync(id, cancellationToken));
    }

    [HttpGet("{id:int}/translations")]
    public async Task<IActionResult> GetTourTranslations(int id, CancellationToken cancellationToken)
    {
        var translations = await adminTourContentService.GetTourTranslationsAsync(id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TourTranslationDto>>.Ok(translations, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPut("{id:int}/translations")]
    public async Task<IActionResult> UpsertTourTranslation(int id, [FromBody] UpsertTourTranslationRequest request, CancellationToken cancellationToken)
    {
        if (id != request.TourId)
        {
            return BadRequest(ApiResponse<object>.Fail("Route id must match request id.", traceId: HttpContext.TraceIdentifier));
        }

        return ToOkOrBadRequest(await adminTourContentService.UpsertTourTranslationAsync(request, cancellationToken));
    }

    [HttpGet("categories/{id:int}/translations")]
    public async Task<IActionResult> GetCategoryTranslations(int id, CancellationToken cancellationToken)
    {
        var translations = await adminTourContentService.GetTourCategoryTranslationsAsync(id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TourCategoryTranslationDto>>.Ok(translations, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPut("categories/{id:int}/translations")]
    public async Task<IActionResult> UpsertCategoryTranslation(int id, [FromBody] UpsertTourCategoryTranslationRequest request, CancellationToken cancellationToken)
    {
        if (id != request.TourCategoryId)
        {
            return BadRequest(ApiResponse<object>.Fail("Route id must match request id.", traceId: HttpContext.TraceIdentifier));
        }

        return ToOkOrBadRequest(await adminTourContentService.UpsertTourCategoryTranslationAsync(request, cancellationToken));
    }

    [HttpGet("itineraries/{id:int}/translations")]
    public async Task<IActionResult> GetItineraryTranslations(int id, CancellationToken cancellationToken)
    {
        var translations = await adminTourContentService.GetItineraryTranslationsAsync(id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<TourItineraryTranslationDto>>.Ok(translations, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPut("itineraries/{id:int}/translations")]
    public async Task<IActionResult> UpsertItineraryTranslation(int id, [FromBody] UpsertTourItineraryTranslationRequest request, CancellationToken cancellationToken)
    {
        if (id != request.TourItineraryId)
        {
            return BadRequest(ApiResponse<object>.Fail("Route id must match request id.", traceId: HttpContext.TraceIdentifier));
        }

        return ToOkOrBadRequest(await adminTourContentService.UpsertItineraryTranslationAsync(request, cancellationToken));
    }

    private IActionResult ToCreatedOrBadRequest<T>(OperationResult<T> result, string actionName)
    {
        if (!result.Succeeded || result.Value is null)
        {
            return BadRequest(ApiResponse<object>.Fail(result.Message ?? "Request failed.", traceId: HttpContext.TraceIdentifier));
        }

        return CreatedAtAction(actionName, ApiResponse<T>.Ok(result.Value, result.Message, HttpContext.TraceIdentifier));
    }

    private IActionResult ToOkOrBadRequest<T>(OperationResult<T> result)
    {
        if (!result.Succeeded || result.Value is null)
        {
            return BadRequest(ApiResponse<object>.Fail(result.Message ?? "Request failed.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<T>.Ok(result.Value, result.Message, HttpContext.TraceIdentifier));
    }

    private IActionResult ToOkOrBadRequest(OperationResult result)
    {
        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<object>.Fail(result.Message ?? "Request failed.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse.Ok(result.Message, HttpContext.TraceIdentifier));
    }
}
