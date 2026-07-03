using FluentValidation;
using TravelToursWebsite.Application.Common;

namespace TravelToursWebsite.Application.Features.Auditing;

public sealed record AuditLogQuery : PagedQuery
{
    public int? UserId { get; init; }
    public string? HttpMethod { get; init; }
    public string? Area { get; init; }
    public int? StatusCode { get; init; }
    public bool? Succeeded { get; init; }
    public DateTime? CreatedFromUtc { get; init; }
    public DateTime? CreatedToUtc { get; init; }
}

public sealed record AuditLogDto(
    long Id,
    int? UserId,
    string? Username,
    string HttpMethod,
    string Path,
    string? QueryString,
    string? Area,
    string? Action,
    int StatusCode,
    bool Succeeded,
    long ElapsedMilliseconds,
    string? IpAddress,
    string? UserAgent,
    string? TraceId,
    DateTime CreatedAtUtc);

public sealed record CreateAuditLogRequest(
    int? UserId,
    string? Username,
    string HttpMethod,
    string Path,
    string? QueryString,
    string? Area,
    string? Action,
    int StatusCode,
    bool Succeeded,
    long ElapsedMilliseconds,
    string? IpAddress,
    string? UserAgent,
    string? TraceId,
    DateTime CreatedAtUtc);

public interface IAuditLogService
{
    Task CreateAsync(CreateAuditLogRequest request, CancellationToken cancellationToken = default);
    Task<PagedResult<AuditLogDto>> GetAuditLogsAsync(AuditLogQuery query, CancellationToken cancellationToken = default);
    Task<AuditLogDto?> GetAuditLogByIdAsync(long id, CancellationToken cancellationToken = default);
}

public sealed class AuditLogQueryValidator : PagedQueryValidator<AuditLogQuery>
{
    public AuditLogQueryValidator()
    {
        RuleFor(query => query.UserId).GreaterThan(0).When(query => query.UserId.HasValue);
        RuleFor(query => query.HttpMethod).MaximumLength(20);
        RuleFor(query => query.Area).MaximumLength(100);
        RuleFor(query => query.StatusCode).InclusiveBetween(100, 599).When(query => query.StatusCode.HasValue);
    }
}
