using FluentValidation;
using Microsoft.Extensions.DependencyInjection;

namespace TravelToursWebsite.Application.DependencyInjection;

public static class ApplicationServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(ApplicationAssemblyMarker).Assembly, includeInternalTypes: false);
        return services;
    }
}