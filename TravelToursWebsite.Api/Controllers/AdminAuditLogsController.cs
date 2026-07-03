using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelToursWebsite.Api.Common;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Application.Features.Auditing;

namespace TravelToursWebsite.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize(Policy = "AdminOnly")]
[Route("api/v{version:apiVersion}/admin/audit-logs")]
public sealed class AdminAuditLogsController(IAuditLogService auditLogService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAuditLogs([FromQuery] AuditLogQuery query, CancellationToken cancellationToken)
    {
        var result = await auditLogService.GetAuditLogsAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<AuditLogDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{id:long}")]
    public async Task<IActionResult> GetAuditLogById(long id, CancellationToken cancellationToken)
    {
        var auditLog = await auditLogService.GetAuditLogByIdAsync(id, cancellationToken);
        return auditLog is null
            ? NotFound(ApiResponse<object>.Fail("Audit log was not found.", traceId: HttpContext.TraceIdentifier))
            : Ok(ApiResponse<AuditLogDto>.Ok(auditLog, traceId: HttpContext.TraceIdentifier));
    }
}
