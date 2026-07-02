using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelToursWebsite.Api.Common;
using TravelToursWebsite.Application.Features.PublicContent;

namespace TravelToursWebsite.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[AllowAnonymous]
[Route("api/v{version:apiVersion}")]
public sealed class PublicContentController(
    IPublicHomeService publicHomeService,
    IPublicSettingsService publicSettingsService)
    : ControllerBase
{
    [HttpGet("home")]
    [ProducesResponseType(typeof(ApiResponse<HomeContentDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetHome(
        [FromQuery] string language = "en",
        CancellationToken cancellationToken = default)
    {
        var content = await publicHomeService.GetHomeAsync(language, cancellationToken);
        return Ok(ApiResponse<HomeContentDto>.Ok(content, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("content/settings")]
    [ProducesResponseType(typeof(ApiResponse<IReadOnlyList<PublicSiteSettingDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetSettings(
        [FromQuery] string? category = null,
        CancellationToken cancellationToken = default)
    {
        var settings = await publicSettingsService.GetSettingsAsync(category, cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<PublicSiteSettingDto>>.Ok(settings, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("content/settings/{key}")]
    [ProducesResponseType(typeof(ApiResponse<PublicSiteSettingDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetSetting(
        string key,
        CancellationToken cancellationToken)
    {
        var setting = await publicSettingsService.GetSettingByKeyAsync(key, cancellationToken);
        if (setting is null)
        {
            return NotFound(ApiResponse<object>.Fail("Content setting was not found.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<PublicSiteSettingDto>.Ok(setting, traceId: HttpContext.TraceIdentifier));
    }
}
