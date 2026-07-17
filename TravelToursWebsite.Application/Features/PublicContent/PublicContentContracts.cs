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

public sealed record PublicPageSectionItemDto(
    int Id,
    string? ItemKey,
    string? Label,
    string? Value,
    string? Description,
    string? Url,
    string? IconClass,
    int SortOrder,
    bool IsActive);

public sealed record PublicPageSectionDto(
    int Id,
    string PageKey,
    string SectionKey,
    string LayoutVariant,
    string? Theme,
    int SortOrder,
    bool IsActive,
    string? DesktopMediaUrl,
    string? MobileMediaUrl,
    string? MediaAlt,
    string? CtaLabel,
    string? CtaUrl,
    int? LinkedTourId,
    int? LinkedTourCategoryId,
    int? LinkedBlogPostId,
    string? Eyebrow,
    string Title,
    string? Subtitle,
    string? Body,
    string? SupportingCopy,
    IReadOnlyList<PublicPageSectionItemDto> Items);
public sealed record HomeContentDto(
    IReadOnlyList<TourSummaryDto> FeaturedTours,
    IReadOnlyList<BlogPostSummaryDto> FeaturedBlogPosts,
    IReadOnlyList<BlogPostSummaryDto> FeaturedEvents,
    IReadOnlyList<TourCategoryDto> TourCategories,
    IReadOnlyList<PublicSiteSettingDto> SiteSettings,
    IReadOnlyList<PublicPageSectionDto> ExperienceSections);

public interface IPublicHomeService
{
    Task<HomeContentDto> GetHomeAsync(string language = "en", CancellationToken cancellationToken = default);
}

public interface IPublicPageService
{
    Task<IReadOnlyList<PublicPageSectionDto>> GetPageSectionsAsync(string pageKey, string language = "en", CancellationToken cancellationToken = default);
}

public interface IPublicSettingsService
{
    Task<IReadOnlyList<PublicSiteSettingDto>> GetSettingsAsync(string? category = null, CancellationToken cancellationToken = default);
    Task<PublicSiteSettingDto?> GetSettingByKeyAsync(string key, CancellationToken cancellationToken = default);
}

public static class PublicContentMappingExtensions
{
    public static PublicPageSectionDto ToPublicDto(this PublicPageSection section, string language = "en")
    {
        var translation = section.Translations.FirstOrDefault(item => item.Language.Equals(language, StringComparison.OrdinalIgnoreCase))
            ?? section.Translations.FirstOrDefault(item => item.Language.Equals("en", StringComparison.OrdinalIgnoreCase))
            ?? section.Translations.FirstOrDefault();

        return new PublicPageSectionDto(
            section.Id,
            section.PageKey,
            section.SectionKey,
            section.LayoutVariant,
            section.Theme,
            section.SortOrder,
            section.IsActive,
            section.DesktopMediaUrl,
            section.MobileMediaUrl,
            section.MediaAlt,
            section.CtaLabel,
            section.CtaUrl,
            section.LinkedTourId,
            section.LinkedTourCategoryId,
            section.LinkedBlogPostId,
            translation?.Eyebrow,
            translation?.Title ?? section.SectionKey,
            translation?.Subtitle,
            translation?.Body,
            translation?.SupportingCopy,
            section.Items
                .Where(item => item.IsActive)
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Id)
                .Select(item => item.ToPublicDto())
                .ToArray());
    }

    public static PublicPageSectionItemDto ToPublicDto(this PublicPageSectionItem item)
    {
        return new PublicPageSectionItemDto(
            item.Id,
            item.ItemKey,
            item.Label,
            item.Value,
            item.Description,
            item.Url,
            item.IconClass,
            item.SortOrder,
            item.IsActive);
    }
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
