using Asp.Versioning.ApiExplorer;
using Microsoft.AspNetCore.Mvc;
using TravelToursWebsite.Api.Middleware;

namespace TravelToursWebsite.Api.Extensions;

internal static class ApiFoundationWebApplicationExtensions
{
    public static WebApplication UseApiFoundation(this WebApplication app)
    {
        app.UseExceptionHandler();
        app.UseStatusCodePages(async statusCodeContext =>
        {
            var httpContext = statusCodeContext.HttpContext;

            if (httpContext.Response.HasStarted)
            {
                return;
            }

            var problem = new ProblemDetails
            {
                Status = httpContext.Response.StatusCode,
                Title = GetStatusCodeTitle(httpContext.Response.StatusCode),
                Instance = httpContext.Request.Path
            };

            problem.Extensions["traceId"] = httpContext.TraceIdentifier;
            await httpContext.Response.WriteAsJsonAsync(problem);
        });

        if (app.Environment.IsDevelopment())
        {
            var versionProvider = app.Services.GetRequiredService<IApiVersionDescriptionProvider>();

            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                foreach (var description in versionProvider.ApiVersionDescriptions)
                {
                    options.SwaggerEndpoint(
                        $"/swagger/{description.GroupName}/swagger.json",
                        $"TravelToursWebsite API {description.GroupName.ToUpperInvariant()}");
                }
            });
        }
        else
        {
            app.UseHsts();
        }

        app.UseHttpsRedirection();
        app.UseStaticFiles();
        app.UseCors(ApiFoundationServiceExtensions.CorsPolicyName);
        app.UseRateLimiter();
        app.UseAuthentication();
        app.UseAuthorization();
        app.UseMiddleware<AuditLoggingMiddleware>();
        app.MapControllers();

        return app;
    }

    private static string GetStatusCodeTitle(int statusCode)
    {
        return statusCode switch
        {
            StatusCodes.Status400BadRequest => "Bad request",
            StatusCodes.Status401Unauthorized => "Unauthorized",
            StatusCodes.Status403Forbidden => "Forbidden",
            StatusCodes.Status404NotFound => "Not found",
            StatusCodes.Status405MethodNotAllowed => "Method not allowed",
            StatusCodes.Status429TooManyRequests => "Too many requests",
            _ => "Request failed"
        };
    }
}
