using Asp.Versioning.ApiExplorer;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace TravelToursWebsite.Api.OpenApi;

internal sealed class ConfigureSwaggerOptions(
    IApiVersionDescriptionProvider versionDescriptionProvider)
    : IConfigureOptions<SwaggerGenOptions>
{
    public void Configure(SwaggerGenOptions options)
    {
        foreach (var description in versionDescriptionProvider.ApiVersionDescriptions)
        {
            options.SwaggerDoc(description.GroupName, new OpenApiInfo
            {
                Title = "TravelToursWebsite API",
                Version = description.ApiVersion.ToString(),
                Description = "Public and admin API for TravelToursWebsite."
            });
        }
    }
}
