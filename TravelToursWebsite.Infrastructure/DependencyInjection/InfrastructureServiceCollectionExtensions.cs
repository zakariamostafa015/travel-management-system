using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using TravelToursWebsite.Application.Features.AdminContent;
using TravelToursWebsite.Application.Features.Auth;
using TravelToursWebsite.Application.Features.Blog;
using TravelToursWebsite.Application.Features.Contact;
using TravelToursWebsite.Application.Features.Media;
using TravelToursWebsite.Application.Features.PublicContent;
using TravelToursWebsite.Application.Features.Tours;
using TravelToursWebsite.Infrastructure.AdminContent;
using TravelToursWebsite.Infrastructure.Auth;
using TravelToursWebsite.Infrastructure.Contact;
using TravelToursWebsite.Infrastructure.Media;
using TravelToursWebsite.Infrastructure.Persistence;
using TravelToursWebsite.Infrastructure.PublicContent;

namespace TravelToursWebsite.Infrastructure.DependencyInjection;

public static class InfrastructureServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        string? connectionString)
    {
        services.AddScoped<IPasswordHasher, LegacyPasswordHasher>();
        services.AddScoped<IJwtTokenService, JwtTokenService>();
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IMediaStorageService, WebPImageStorageService>();
        services.AddScoped<ITourCatalogService, TourCatalogService>();
        services.AddScoped<IBlogCatalogService, BlogCatalogService>();
        services.AddScoped<AdminContentManagementService>();
        services.AddScoped<ITourManagementService>(provider => provider.GetRequiredService<AdminContentManagementService>());
        services.AddScoped<IBlogManagementService>(provider => provider.GetRequiredService<AdminContentManagementService>());
        services.AddScoped<IAdminTourContentService>(provider => provider.GetRequiredService<AdminContentManagementService>());
        services.AddScoped<IAdminBlogContentService>(provider => provider.GetRequiredService<AdminContentManagementService>());
        services.AddScoped<ContactApplicationService>();
        services.AddScoped<IContactApplicationService>(provider => provider.GetRequiredService<ContactApplicationService>());
        services.AddScoped<IBookingApplicationService>(provider => provider.GetRequiredService<ContactApplicationService>());
        services.AddScoped<IContactNotificationService, EmailContactNotificationService>();
        services.AddScoped<IPublicHomeService, PublicContentService>();
        services.AddScoped<IPublicSettingsService, PublicContentService>();

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return services;
        }

        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlServer(connectionString, sqlOptions =>
            {
                sqlOptions.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                sqlOptions.EnableRetryOnFailure();
            });
        });

        return services;
    }
}