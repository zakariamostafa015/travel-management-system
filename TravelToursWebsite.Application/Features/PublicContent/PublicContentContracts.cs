using TravelToursWebsite.Application.Features.Blog;
using TravelToursWebsite.Application.Features.Tours;
using TravelToursWebsite.Domain.Entities;
using TravelToursWebsite.Domain.Enums;

namespace TravelToursWebsite.Application.Features.PublicContent;

public sealed record PublicSiteSettingDto(
    string Key,
    string? Value,
    string? Description,
    string? Category,
    string? IconClass,
    int SortOrder,
    SettingType Type);

public sealed record HomeContentDto(
    IReadOnlyList<TourSummaryDto> FeaturedTours,
    IReadOnlyList<BlogPostSummaryDto> FeaturedBlogPosts,
    IReadOnlyList<BlogPostSummaryDto> FeaturedEvents,
    IReadOnlyList<TourCategoryDto> TourCategories,
    IReadOnlyList<PublicSiteSettingDto> SiteSettings);

public interface IPublicHomeService
{
    Task<HomeContentDto> GetHomeAsync(string language = "en", CancellationToken cancellationToken = default);
}

public interface IPublicSettingsService
{
    Task<IReadOnlyList<PublicSiteSettingDto>> GetSettingsAsync(string? category = null, CancellationToken cancellationToken = default);
    Task<PublicSiteSettingDto?> GetSettingByKeyAsync(string key, CancellationToken cancellationToken = default);
}

public static class PublicContentMappingExtensions
{
    public static PublicSiteSettingDto ToPublicDto(this SiteSettings settings)
    {
        return new PublicSiteSettingDto(
            settings.Key,
            settings.Value,
            settings.Description,
            settings.Category,
            settings.IconClass,
            settings.SortOrder,
            settings.Type);
    }
}
