using System.Diagnostics;
using System.Security.Claims;
using TravelToursWebsite.Application.Features.Auditing;

namespace TravelToursWebsite.Api.Middleware;

internal sealed class AuditLoggingMiddleware(RequestDelegate next, ILogger<AuditLoggingMiddleware> logger)
{
    private static readonly HashSet<string> AuditedMethods = new(StringComparer.OrdinalIgnoreCase)
    {
        HttpMethods.Post,
        HttpMethods.Put,
        HttpMethods.Patch,
        HttpMethods.Delete
    };

    public async Task InvokeAsync(HttpContext context, IAuditLogService auditLogService)
    {
        if (!ShouldAudit(context))
        {
            await next(context);
            return;
        }

        var stopwatch = Stopwatch.StartNew();
        Exception? capturedException = null;

        try
        {
            await next(context);
        }
        catch (Exception exception)
        {
            capturedException = exception;
            throw;
        }
        finally
        {
            stopwatch.Stop();
            await TryWriteAuditLogAsync(context, auditLogService, stopwatch.ElapsedMilliseconds, capturedException);
        }
    }

    private async Task TryWriteAuditLogAsync(HttpContext context, IAuditLogService auditLogService, long elapsedMilliseconds, Exception? exception)
    {
        try
        {
            var userId = TryGetUserId(context.User);
            var username = context.User.Identity?.Name
                ?? context.User.FindFirstValue(ClaimTypes.Name)
                ?? context.User.FindFirstValue("unique_name");
            var statusCode = exception is null
                ? context.Response.StatusCode
                : StatusCodes.Status500InternalServerError;

            await auditLogService.CreateAsync(new CreateAuditLogRequest(
                userId,
                username,
                context.Request.Method.ToUpperInvariant(),
                context.Request.Path.Value ?? string.Empty,
                RedactQueryString(context.Request.QueryString.Value),
                GetArea(context.Request.Path),
                GetAction(context.Request.Method, context.Request.Path),
                statusCode,
                statusCode is >= 200 and < 400,
                elapsedMilliseconds,
                context.Connection.RemoteIpAddress?.ToString(),
                context.Request.Headers.UserAgent.ToString(),
                context.TraceIdentifier,
                DateTime.UtcNow), context.RequestAborted);
        }
        catch (Exception auditException)
        {
            logger.LogWarning(auditException, "Failed to write audit log for {Method} {Path}.", context.Request.Method, context.Request.Path);
        }
    }

    private static bool ShouldAudit(HttpContext context)
    {
        return AuditedMethods.Contains(context.Request.Method)
            && context.User.Identity?.IsAuthenticated == true
            && !context.Request.Path.StartsWithSegments("/health", StringComparison.OrdinalIgnoreCase);
    }

    private static int? TryGetUserId(ClaimsPrincipal user)
    {
        var value = user.FindFirstValue(ClaimTypes.NameIdentifier) ?? user.FindFirstValue("sub");
        return int.TryParse(value, out var userId) ? userId : null;
    }

    private static string? RedactQueryString(string? queryString)
    {
        if (string.IsNullOrWhiteSpace(queryString))
        {
            return null;
        }

        return queryString.Length <= 500 ? queryString : queryString[..500];
    }

    private static string? GetArea(PathString path)
    {
        var parts = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries);
        if (parts is null || parts.Length < 4)
        {
            return null;
        }

        return parts[2].Equals("admin", StringComparison.OrdinalIgnoreCase)
            ? parts.Length > 3 ? parts[3] : "admin"
            : parts[2];
    }

    private static string GetAction(string method, PathString path)
    {
        var parts = path.Value?.Split('/', StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();
        var tail = parts.Length == 0 ? string.Empty : parts[^1];
        return string.IsNullOrWhiteSpace(tail)
            ? method.ToUpperInvariant()
            : $"{method.ToUpperInvariant()} {tail}";
    }
}
