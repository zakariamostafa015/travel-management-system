using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelToursWebsite.Api.Common;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Application.Features.Tours;

namespace TravelToursWebsite.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[AllowAnonymous]
[Route("api/v{version:apiVersion}/tours")]
public sealed class ToursController(ITourCatalogService tourCatalogService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TourSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTours(
        [FromQuery] TourQuery query,
        CancellationToken cancellationToken)
    {
        var result = await tourCatalogService.GetToursAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<TourSummaryDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{idOrSlug}")]
    [ProducesResponseType(typeof(ApiResponse<TourDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetTour(
        string idOrSlug,
        [FromQuery] string language = "en",
        CancellationToken cancellationToken = default)
    {
        var tour = int.TryParse(idOrSlug, out var id)
            ? await tourCatalogService.GetTourByIdAsync(id, language, cancellationToken)
            : await tourCatalogService.GetTourBySlugAsync(idOrSlug, language, cancellationToken);

        if (tour is null)
        {
            return NotFound(ApiResponse<object>.Fail("Tour was not found.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<TourDetailsDto>.Ok(tour, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("categories")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<TourCategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(
        [FromQuery] TourCategoryQuery query,
        CancellationToken cancellationToken)
    {
        var result = await tourCatalogService.GetCategoriesAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<TourCategoryDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("categories/{idOrSlug}")]
    [ProducesResponseType(typeof(ApiResponse<TourCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategory(
        string idOrSlug,
        [FromQuery] string language = "en",
        CancellationToken cancellationToken = default)
    {
        var category = int.TryParse(idOrSlug, out var id)
            ? await tourCatalogService.GetCategoryByIdAsync(id, language, cancellationToken)
            : await tourCatalogService.GetCategoryBySlugAsync(idOrSlug, language, cancellationToken);

        if (category is null)
        {
            return NotFound(ApiResponse<object>.Fail("Tour category was not found.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<TourCategoryDto>.Ok(category, traceId: HttpContext.TraceIdentifier));
    }
}
