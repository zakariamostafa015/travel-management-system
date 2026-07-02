using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelToursWebsite.Api.Common;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Application.Features.Blog;

namespace TravelToursWebsite.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[AllowAnonymous]
[Route("api/v{version:apiVersion}/blog")]
public sealed class BlogController(IBlogCatalogService blogCatalogService) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BlogPostSummaryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPosts(
        [FromQuery] BlogPostQuery query,
        CancellationToken cancellationToken)
    {
        var result = await blogCatalogService.GetPostsAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<BlogPostSummaryDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{idOrSlug}")]
    [ProducesResponseType(typeof(ApiResponse<BlogPostDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetPost(
        string idOrSlug,
        [FromQuery] string language = "en",
        CancellationToken cancellationToken = default)
    {
        var post = int.TryParse(idOrSlug, out var id)
            ? await blogCatalogService.GetPostByIdAsync(id, language, cancellationToken)
            : await blogCatalogService.GetPostBySlugAsync(idOrSlug, language, cancellationToken);

        if (post is null)
        {
            return NotFound(ApiResponse<object>.Fail("Blog post was not found.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<BlogPostDetailsDto>.Ok(post, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("categories")]
    [ProducesResponseType(typeof(ApiResponse<PagedResult<BlogCategoryDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCategories(
        [FromQuery] BlogCategoryQuery query,
        CancellationToken cancellationToken)
    {
        var result = await blogCatalogService.GetCategoriesAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<BlogCategoryDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("categories/{idOrSlug}")]
    [ProducesResponseType(typeof(ApiResponse<BlogCategoryDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCategory(
        string idOrSlug,
        [FromQuery] string language = "en",
        CancellationToken cancellationToken = default)
    {
        var category = int.TryParse(idOrSlug, out var id)
            ? await blogCatalogService.GetCategoryByIdAsync(id, language, cancellationToken)
            : await blogCatalogService.GetCategoryBySlugAsync(idOrSlug, language, cancellationToken);

        if (category is null)
        {
            return NotFound(ApiResponse<object>.Fail("Blog category was not found.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<BlogCategoryDto>.Ok(category, traceId: HttpContext.TraceIdentifier));
    }
}
