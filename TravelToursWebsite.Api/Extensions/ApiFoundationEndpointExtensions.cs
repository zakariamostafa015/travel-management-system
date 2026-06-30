using Asp.Versioning;
using TravelToursWebsite.Api.Common;

namespace TravelToursWebsite.Api.Extensions;

internal static class ApiFoundationEndpointExtensions
{
    public static WebApplication MapApiFoundationEndpoints(this WebApplication app)
    {
        app.MapHealthChecks("/health")
            .WithName("HealthCheck")
            .AllowAnonymous();

        var versionSet = app.NewApiVersionSet()
            .HasApiVersion(new ApiVersion(1, 0))
            .ReportApiVersions()
            .Build();

        var api = app.MapGroup("/api/v{version:apiVersion}")
            .WithApiVersionSet(versionSet)
            .MapToApiVersion(new ApiVersion(1, 0));

        api.MapGet("/", (HttpContext httpContext) =>
            Results.Ok(ApiResponse<object>.Ok(
                new
                {
                    Service = "TravelToursWebsite API",
                    Status = "Phase 2 foundation ready",
                    Version = "1.0"
                },
                traceId: httpContext.TraceIdentifier)))
            .WithName("GetApiInfo")
            .AllowAnonymous();

        return app;
    }
}
