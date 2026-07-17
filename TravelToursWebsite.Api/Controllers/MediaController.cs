using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelToursWebsite.Api.Common;
using TravelToursWebsite.Application.Features.Media;

namespace TravelToursWebsite.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize(Policy = "ContentManager")]
[Route("api/v{version:apiVersion}/media")]
public sealed class MediaController(IMediaStorageService mediaStorageService) : ControllerBase
{
    [HttpPost("images")]
    [RequestSizeLimit(50 * 1024 * 1024)]
    [ProducesResponseType(typeof(ApiResponse<MediaAssetDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> UploadImage(
        IFormFile file,
        [FromForm] string folderName = "media",
        [FromForm] string? altText = null,
        [FromForm] string? caption = null,
        CancellationToken cancellationToken = default)
    {
        if (file is null)
        {
            return BadRequest(ApiResponse<object>.Fail("Image file is required.", traceId: HttpContext.TraceIdentifier));
        }

        await using var stream = file.OpenReadStream();
        var request = new MediaUploadRequest(
            stream,
            file.FileName,
            file.ContentType,
            file.Length,
            folderName,
            GetRequestBaseUrl(),
            altText,
            caption);

        var result = await mediaStorageService.SaveImageAsync(request, cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return BadRequest(ApiResponse<object>.Fail(result.Message ?? "Image upload failed.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<MediaAssetDto>.Ok(result.Value, "Image uploaded.", HttpContext.TraceIdentifier));
    }

    private string GetRequestBaseUrl() => $"{Request.Scheme}://{Request.Host}{Request.PathBase}";

    [HttpDelete("images")]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> DeleteImage([FromQuery] string imageLocalPath, CancellationToken cancellationToken)
    {
        var result = await mediaStorageService.DeleteImageAsync(imageLocalPath, cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<object>.Fail(result.Message ?? "Image delete failed.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse.Ok(result.Message, HttpContext.TraceIdentifier));
    }
}
