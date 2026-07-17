using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelToursWebsite.Api.Common;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Application.Features.AdminContent;
using TravelToursWebsite.Application.Features.Blog;
using TravelToursWebsite.Application.Features.Media;

namespace TravelToursWebsite.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize(Policy = "ContentManager")]
[Route("api/v{version:apiVersion}/admin/blog")]
public sealed class AdminBlogController(
    IBlogManagementService blogManagementService,
    IAdminBlogContentService adminBlogContentService,
    IMediaStorageService mediaStorageService)
    : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> CreatePost([FromBody] CreateBlogPostRequest request, CancellationToken cancellationToken)
    {
        return ToCreatedOrBadRequest(await blogManagementService.CreatePostAsync(request, cancellationToken), nameof(CreatePost));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdatePost(int id, [FromBody] UpdateBlogPostRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            return BadRequest(ApiResponse<object>.Fail("Route id must match request id.", traceId: HttpContext.TraceIdentifier));
        }

        return ToOkOrBadRequest(await blogManagementService.UpdatePostAsync(request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeletePost(int id, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await blogManagementService.DeletePostAsync(id, cancellationToken));
    }

    [HttpPost("categories")]
    public async Task<IActionResult> CreateCategory([FromBody] CreateBlogCategoryRequest request, CancellationToken cancellationToken)
    {
        return ToCreatedOrBadRequest(await blogManagementService.CreateCategoryAsync(request, cancellationToken), nameof(CreateCategory));
    }

    [HttpPut("categories/{id:int}")]
    public async Task<IActionResult> UpdateCategory(int id, [FromBody] UpdateBlogCategoryRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            return BadRequest(ApiResponse<object>.Fail("Route id must match request id.", traceId: HttpContext.TraceIdentifier));
        }

        return ToOkOrBadRequest(await blogManagementService.UpdateCategoryAsync(request, cancellationToken));
    }

    [HttpDelete("categories/{id:int}")]
    public async Task<IActionResult> DeleteCategory(int id, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await blogManagementService.DeleteCategoryAsync(id, cancellationToken));
    }

    [HttpPost("images")]
    public async Task<IActionResult> AddImage([FromBody] BlogImageRequest request, CancellationToken cancellationToken)
    {
        return ToCreatedOrBadRequest(await adminBlogContentService.AddBlogImageAsync(request, cancellationToken), nameof(AddImage));
    }

    [HttpPut("images/{id:int}")]
    public async Task<IActionResult> UpdateImage(int id, [FromBody] UpdateBlogImageRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            return BadRequest(ApiResponse<object>.Fail("Route id must match request id.", traceId: HttpContext.TraceIdentifier));
        }

        return ToOkOrBadRequest(await adminBlogContentService.UpdateBlogImageAsync(request, cancellationToken));
    }

    [HttpDelete("images/{id:int}")]
    public async Task<IActionResult> DeleteImage(int id, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await adminBlogContentService.DeleteBlogImageAsync(id, cancellationToken));
    }


    [HttpPost("{blogPostId:int}/images/upload")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    public async Task<IActionResult> UploadImages(int blogPostId, [FromForm] List<IFormFile> files, CancellationToken cancellationToken)
    {
        if (files.Count == 0)
        {
            return BadRequest(ApiResponse<object>.Fail("At least one image file is required.", traceId: HttpContext.TraceIdentifier));
        }

        var images = new List<BlogImageDto>();
        for (var index = 0; index < files.Count; index++)
        {
            var file = files[index];
            if (file.Length <= 0)
            {
                return BadRequest(ApiResponse<object>.Fail("Image file is empty.", traceId: HttpContext.TraceIdentifier));
            }

            await using var stream = file.OpenReadStream();
            var upload = await mediaStorageService.SaveImageAsync(new MediaUploadRequest(
                stream,
                file.FileName,
                file.ContentType,
                file.Length,
                "blog",
                GetRequestBaseUrl()), cancellationToken);

            if (!upload.Succeeded || upload.Value is null)
            {
                return BadRequest(ApiResponse<object>.Fail(upload.Message ?? "Image upload failed.", traceId: HttpContext.TraceIdentifier));
            }

            var image = await adminBlogContentService.AddBlogImageAsync(new BlogImageRequest(
                blogPostId,
                upload.Value.ImageUrl,
                upload.Value.ImageLocalPath,
                upload.Value.ThumbnailLocalPath,
                null,
                null,
                index), cancellationToken);

            if (!image.Succeeded || image.Value is null)
            {
                return BadRequest(ApiResponse<object>.Fail(image.Message ?? "Image save failed.", traceId: HttpContext.TraceIdentifier));
            }

            images.Add(image.Value);
        }

        return Ok(ApiResponse<IReadOnlyList<BlogImageDto>>.Ok(images, "Blog images uploaded.", HttpContext.TraceIdentifier));
    }
    [HttpGet("{id:int}/translations")]
    public async Task<IActionResult> GetPostTranslations(int id, CancellationToken cancellationToken)
    {
        var translations = await adminBlogContentService.GetBlogPostTranslationsAsync(id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<BlogPostTranslationDto>>.Ok(translations, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPut("{id:int}/translations")]
    public async Task<IActionResult> UpsertPostTranslation(int id, [FromBody] UpsertBlogPostTranslationRequest request, CancellationToken cancellationToken)
    {
        if (id != request.BlogPostId)
        {
            return BadRequest(ApiResponse<object>.Fail("Route id must match request id.", traceId: HttpContext.TraceIdentifier));
        }

        return ToOkOrBadRequest(await adminBlogContentService.UpsertBlogPostTranslationAsync(request, cancellationToken));
    }

    [HttpGet("categories/{id:int}/translations")]
    public async Task<IActionResult> GetCategoryTranslations(int id, CancellationToken cancellationToken)
    {
        var translations = await adminBlogContentService.GetBlogCategoryTranslationsAsync(id, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<BlogCategoryTranslationDto>>.Ok(translations, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPut("categories/{id:int}/translations")]
    public async Task<IActionResult> UpsertCategoryTranslation(int id, [FromBody] UpsertBlogCategoryTranslationRequest request, CancellationToken cancellationToken)
    {
        if (id != request.BlogCategoryId)
        {
            return BadRequest(ApiResponse<object>.Fail("Route id must match request id.", traceId: HttpContext.TraceIdentifier));
        }

        return ToOkOrBadRequest(await adminBlogContentService.UpsertBlogCategoryTranslationAsync(request, cancellationToken));
    }

    [HttpPost("{id:int}/view-count")]
    public async Task<IActionResult> IncrementViewCount(int id, CancellationToken cancellationToken)
    {
        await blogManagementService.IncrementViewCountAsync(id, cancellationToken);
        return Ok(ApiResponse.Ok("Blog post view count incremented.", HttpContext.TraceIdentifier));
    }

    private string GetRequestBaseUrl() => $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

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

