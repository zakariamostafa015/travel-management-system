using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelToursWebsite.Api.Common;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Application.Features.Administration;

namespace TravelToursWebsite.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize(Policy = "AdminOnly")]
[Route("api/v{version:apiVersion}/admin")]
public sealed class AdminOperationsController(
    ILanguageApplicationService languageService,
    IOperationsContentService operationsContentService,
    IPublicPageSectionManagementService publicPageSectionService,
    IResourceContentService resourceContentService)
    : ControllerBase
{
    [HttpGet("languages")]
    public async Task<IActionResult> GetLanguages([FromQuery] LanguageQuery query, CancellationToken cancellationToken)
    {
        var result = await languageService.GetLanguagesAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<LanguageDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("languages/active-codes")]
    public async Task<IActionResult> GetActiveLanguageCodes(CancellationToken cancellationToken)
    {
        var result = await languageService.GetActiveLanguageCodesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<string>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("languages/default")]
    public async Task<IActionResult> GetDefaultLanguage(CancellationToken cancellationToken)
    {
        var result = await languageService.GetDefaultLanguageAsync(cancellationToken);
        return result is null
            ? NotFound(ApiResponse<object>.Fail("Default language was not found.", traceId: HttpContext.TraceIdentifier))
            : Ok(ApiResponse<LanguageDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("languages/{id:int}")]
    public async Task<IActionResult> GetLanguageById(int id, CancellationToken cancellationToken)
    {
        var result = await languageService.GetLanguageByIdAsync(id, cancellationToken);
        return result is null
            ? NotFound(ApiResponse<object>.Fail("Language was not found.", traceId: HttpContext.TraceIdentifier))
            : Ok(ApiResponse<LanguageDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPut("languages")]
    public async Task<IActionResult> UpsertLanguage([FromBody] UpsertLanguageRequest request, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await languageService.UpsertLanguageAsync(request, cancellationToken));
    }

    [HttpDelete("languages/{id:int}")]
    public async Task<IActionResult> DeleteLanguage(int id, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await languageService.DeleteLanguageAsync(id, cancellationToken));
    }

    [HttpPatch("languages/{id:int}/default")]
    public async Task<IActionResult> SetDefaultLanguage(int id, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await languageService.SetDefaultLanguageAsync(id, cancellationToken));
    }

    [HttpPatch("languages/{id:int}/toggle-status")]
    public async Task<IActionResult> ToggleLanguageStatus(int id, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await languageService.ToggleLanguageStatusAsync(id, cancellationToken));
    }

    [HttpGet("departments")]
    public async Task<IActionResult> GetDepartments([FromQuery] DepartmentQuery query, CancellationToken cancellationToken)
    {
        var result = await operationsContentService.GetDepartmentsAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<DepartmentDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPut("departments")]
    public async Task<IActionResult> UpsertDepartment([FromBody] UpsertDepartmentRequest request, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await operationsContentService.UpsertDepartmentAsync(request, cancellationToken));
    }

    [HttpDelete("departments/{id:int}")]
    public async Task<IActionResult> DeleteDepartment(int id, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await operationsContentService.DeleteDepartmentAsync(id, cancellationToken));
    }

    [HttpGet("team-members")]
    public async Task<IActionResult> GetTeamMembers([FromQuery] TeamMemberQuery query, CancellationToken cancellationToken)
    {
        var result = await operationsContentService.GetTeamMembersAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<TeamMemberDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPut("team-members")]
    public async Task<IActionResult> UpsertTeamMember([FromBody] UpsertTeamMemberRequest request, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await operationsContentService.UpsertTeamMemberAsync(request, cancellationToken));
    }

    [HttpDelete("team-members/{id:int}")]
    public async Task<IActionResult> DeleteTeamMember(int id, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await operationsContentService.DeleteTeamMemberAsync(id, cancellationToken));
    }

    [HttpGet("settings")]
    public async Task<IActionResult> GetSiteSettings([FromQuery] SiteSettingsQuery query, CancellationToken cancellationToken)
    {
        var result = await operationsContentService.GetSiteSettingsAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<SiteSettingsDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPut("settings")]
    public async Task<IActionResult> UpsertSiteSettings([FromBody] UpsertSiteSettingsRequest request, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await operationsContentService.UpsertSiteSettingsAsync(request, cancellationToken));
    }

    [HttpDelete("settings/{id:int}")]
    public async Task<IActionResult> DeleteSiteSettings(int id, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await operationsContentService.DeleteSiteSettingsAsync(id, cancellationToken));
    }

    [HttpGet("public-page-sections")]
    public async Task<IActionResult> GetPublicPageSections([FromQuery] PublicPageSectionQuery query, CancellationToken cancellationToken)
    {
        var result = await publicPageSectionService.GetSectionsAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<AdminPublicPageSectionDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("public-page-sections/{id:int}")]
    public async Task<IActionResult> GetPublicPageSectionById(int id, CancellationToken cancellationToken)
    {
        var result = await publicPageSectionService.GetSectionByIdAsync(id, cancellationToken);
        return result is null
            ? NotFound(ApiResponse<object>.Fail("Public page section was not found.", traceId: HttpContext.TraceIdentifier))
            : Ok(ApiResponse<AdminPublicPageSectionDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPut("public-page-sections")]
    public async Task<IActionResult> UpsertPublicPageSection([FromBody] UpsertPublicPageSectionRequest request, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await publicPageSectionService.UpsertSectionAsync(request, cancellationToken));
    }

    [HttpDelete("public-page-sections/{id:int}")]
    public async Task<IActionResult> DeletePublicPageSection(int id, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await publicPageSectionService.DeleteSectionAsync(id, cancellationToken));
    }
    [HttpGet("resources/languages")]
    public async Task<IActionResult> GetResourceLanguages(CancellationToken cancellationToken)
    {
        var result = await resourceContentService.GetLanguagesAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<ResourceContentLanguageDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("resources/{cultureCode}")]
    public async Task<IActionResult> GetLanguageContent(string cultureCode, CancellationToken cancellationToken)
    {
        var result = await resourceContentService.GetLanguageContentAsync(cultureCode, cancellationToken);
        return Ok(ApiResponse<IReadOnlyDictionary<string, string>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("resources/items/{key}")]
    public async Task<IActionResult> GetContentItem(string key, CancellationToken cancellationToken)
    {
        var result = await resourceContentService.GetContentItemAsync(key, cancellationToken);
        return Ok(ApiResponse<ResourceContentItemDto>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPut("resources/items")]
    public async Task<IActionResult> UpsertContentItem([FromBody] UpsertResourceContentItemRequest request, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await resourceContentService.UpsertContentItemAsync(request, cancellationToken));
    }

    [HttpDelete("resources/items/{key}")]
    public async Task<IActionResult> DeleteContentItem(string key, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await resourceContentService.DeleteContentItemAsync(key, cancellationToken));
    }

    [HttpGet("resources/{cultureCode}/validate")]
    public async Task<IActionResult> ValidateLanguageFile(string cultureCode, CancellationToken cancellationToken)
    {
        var result = await resourceContentService.ValidateLanguageFileAsync(cultureCode, cancellationToken);
        return Ok(ApiResponse<bool>.Ok(result, traceId: HttpContext.TraceIdentifier));
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
