using Microsoft.EntityFrameworkCore;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Application.Features.Auditing;
using TravelToursWebsite.Domain.Entities;
using TravelToursWebsite.Infrastructure.Persistence;

namespace TravelToursWebsite.Infrastructure.Auditing;

public sealed class AuditLogService(ApplicationDbContext context) : IAuditLogService
{
    private const int MaxPageSize = 100;

    public async Task CreateAsync(CreateAuditLogRequest request, CancellationToken cancellationToken = default)
    {
        var auditLog = new AuditLog
        {
            UserId = request.UserId,
            Username = NormalizeOptional(request.Username, 100),
            HttpMethod = Truncate(request.HttpMethod, 20),
            Path = Truncate(request.Path, 500),
            QueryString = NormalizeOptional(request.QueryString, 500),
            Area = NormalizeOptional(request.Area, 100),
            Action = NormalizeOptional(request.Action, 100),
            StatusCode = request.StatusCode,
            Succeeded = request.Succeeded,
            ElapsedMilliseconds = request.ElapsedMilliseconds,
            IpAddress = NormalizeOptional(request.IpAddress, 64),
            UserAgent = NormalizeOptional(request.UserAgent, 512),
            TraceId = NormalizeOptional(request.TraceId, 100),
            CreatedAtUtc = request.CreatedAtUtc
        };

        context.AuditLogs.Add(auditLog);
        await context.SaveChangesAsync(cancellationToken);
    }

    public async Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(AuditLogQuery query, CancellationToken cancellationToken = default)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);
        var auditLogs = context.AuditLogs.AsNoTracking().AsQueryable();

        if (query.UserId.HasValue)
        {
            auditLogs = auditLogs.Where(log => log.UserId == query.UserId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.HttpMethod))
        {
            var method = query.HttpMethod.Trim().ToUpperInvariant();
            auditLogs = auditLogs.Where(log => log.HttpMethod == method);
        }

        if (!string.IsNullOrWhiteSpace(query.Area))
        {
            var area = query.Area.Trim();
            auditLogs = auditLogs.Where(log => log.Area == area);
        }

        if (query.StatusCode.HasValue)
        {
            auditLogs = auditLogs.Where(log => log.StatusCode == query.StatusCode.Value);
        }

        if (query.Succeeded.HasValue)
        {
            auditLogs = auditLogs.Where(log => log.Succeeded == query.Succeeded.Value);
        }

        if (query.CreatedFromUtc.HasValue)
        {
            auditLogs = auditLogs.Where(log => log.CreatedAtUtc >= query.CreatedFromUtc.Value);
        }

        if (query.CreatedToUtc.HasValue)
        {
            auditLogs = auditLogs.Where(log => log.CreatedAtUtc <= query.CreatedToUtc.Value);
        }

        auditLogs = ApplySearch(auditLogs, query.SearchTerm);
        var totalCount = await auditLogs.CountAsync(cancellationToken);
        if (totalCount == 0)
        {
            return PagedResult<AuditLogDto>.Empty(pageNumber, pageSize);
        }

        var items = await ApplySorting(auditLogs, query.SortBy, query.SortDirection)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .Select(log => ToDto(log))
            .ToListAsync(cancellationToken);

        return new PagedResult<AuditLogDto>(items, pageNumber, pageSize, totalCount);
    }

    public async Task<AuditLogDto?> GetAuditLogByIdAsync(long id, CancellationToken cancellationToken = default)
    {
        return await context.AuditLogs
            .AsNoTracking()
            .Where(log => log.Id == id)
            .Select(log => ToDto(log))
            .FirstOrDefaultAsync(cancellationToken);
    }

    private static IQueryable<AuditLog> ApplySearch(IQueryable<AuditLog> auditLogs, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return auditLogs;
        }

        var term = searchTerm.Trim();
        return auditLogs.Where(log =>
            log.Path.Contains(term)
            || log.Username != null && log.Username.Contains(term)
            || log.TraceId != null && log.TraceId.Contains(term)
            || log.IpAddress != null && log.IpAddress.Contains(term));
    }

    private static IQueryable<AuditLog> ApplySorting(IQueryable<AuditLog> auditLogs, string? sortBy, SortDirection sortDirection)
    {
        var descending = sortDirection == SortDirection.Descending;
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "username" => descending ? auditLogs.OrderByDescending(log => log.Username).ThenByDescending(log => log.Id) : auditLogs.OrderBy(log => log.Username).ThenBy(log => log.Id),
            "method" => descending ? auditLogs.OrderByDescending(log => log.HttpMethod).ThenByDescending(log => log.Id) : auditLogs.OrderBy(log => log.HttpMethod).ThenBy(log => log.Id),
            "path" => descending ? auditLogs.OrderByDescending(log => log.Path).ThenByDescending(log => log.Id) : auditLogs.OrderBy(log => log.Path).ThenBy(log => log.Id),
            "statuscode" => descending ? auditLogs.OrderByDescending(log => log.StatusCode).ThenByDescending(log => log.Id) : auditLogs.OrderBy(log => log.StatusCode).ThenBy(log => log.Id),
            "elapsedmilliseconds" => descending ? auditLogs.OrderByDescending(log => log.ElapsedMilliseconds).ThenByDescending(log => log.Id) : auditLogs.OrderBy(log => log.ElapsedMilliseconds).ThenBy(log => log.Id),
            _ => descending ? auditLogs.OrderByDescending(log => log.CreatedAtUtc).ThenByDescending(log => log.Id) : auditLogs.OrderBy(log => log.CreatedAtUtc).ThenBy(log => log.Id)
        };
    }

    private static AuditLogDto ToDto(AuditLog log)
    {
        return new AuditLogDto(
            log.Id,
            log.UserId,
            log.Username,
            log.HttpMethod,
            log.Path,
            log.QueryString,
            log.Area,
            log.Action,
            log.StatusCode,
            log.Succeeded,
            log.ElapsedMilliseconds,
            log.IpAddress,
            log.UserAgent,
            log.TraceId,
            log.CreatedAtUtc);
    }

    private static string Truncate(string value, int maxLength)
    {
        return value.Length <= maxLength ? value : value[..maxLength];
    }

    private static string? NormalizeOptional(string? value, int maxLength)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        var trimmed = value.Trim();
        return trimmed.Length <= maxLength ? trimmed : trimmed[..maxLength];
    }
}
