using Microsoft.EntityFrameworkCore;
using TravelToursWebsite.Application.Features.Blog;
using TravelToursWebsite.Application.Features.PublicContent;
using TravelToursWebsite.Application.Features.Tours;
using TravelToursWebsite.Infrastructure.Persistence;

namespace TravelToursWebsite.Infrastructure.PublicContent;

public sealed class PublicContentService(ApplicationDbContext context) : IPublicHomeService, IPublicSettingsService
{
    public async Task<HomeContentDto> GetHomeAsync(
        string language = "en",
        CancellationToken cancellationToken = default)
    {
        var normalizedLanguage = NormalizeLanguage(language);

        var featuredTours = await context.Tours
            .AsNoTracking()
            .Where(tour => tour.IsActive && tour.IsFeatured)
            .OrderBy(tour => tour.SortOrder)
            .ThenByDescending(tour => tour.CreatedDate)
            .Include(tour => tour.Category)
                .ThenInclude(category => category!.Translations)
            .Include(tour => tour.Translations)
            .Take(6)
            .ToListAsync(cancellationToken);

        var featuredPosts = await context.BlogPosts
            .AsNoTracking()
            .Where(post => post.IsPublished && post.IsFeatured && !post.IsEvent)
            .OrderByDescending(post => post.PublishedDate)
            .ThenByDescending(post => post.Id)
            .Include(post => post.Category)
                .ThenInclude(category => category!.Translations)
            .Include(post => post.Author)
            .Include(post => post.Translations)
            .Take(3)
            .ToListAsync(cancellationToken);

        var featuredEvents = await context.BlogPosts
            .AsNoTracking()
            .Where(post => post.IsPublished && post.IsFeatured && post.IsEvent)
            .OrderByDescending(post => post.PublishedDate)
            .ThenByDescending(post => post.Id)
            .Include(post => post.Category)
                .ThenInclude(category => category!.Translations)
            .Include(post => post.Author)
            .Include(post => post.Translations)
            .Take(3)
            .ToListAsync(cancellationToken);

        var tourCategories = await context.TourCategories
            .AsNoTracking()
            .Where(category => category.IsActive)
            .OrderBy(category => category.SortOrder)
            .ThenBy(category => category.Id)
            .Include(category => category.Translations)
            .Take(12)
            .ToListAsync(cancellationToken);

        var settings = await context.SiteSettings
            .AsNoTracking()
            .Where(setting => setting.IsActive)
            .OrderBy(setting => setting.Category)
            .ThenBy(setting => setting.SortOrder)
            .ThenBy(setting => setting.Key)
            .ToListAsync(cancellationToken);

        return new HomeContentDto(
            featuredTours.Select(tour => tour.ToSummaryDto(normalizedLanguage)).ToArray(),
            featuredPosts.Select(post => post.ToSummaryDto(normalizedLanguage)).ToArray(),
            featuredEvents.Select(post => post.ToSummaryDto(normalizedLanguage)).ToArray(),
            tourCategories.Select(category => category.ToDto(normalizedLanguage)).ToArray(),
            settings.Select(setting => setting.ToPublicDto()).ToArray());
    }

    public async Task<IReadOnlyList<PublicSiteSettingDto>> GetSettingsAsync(
        string? category = null,
        CancellationToken cancellationToken = default)
    {
        var settings = context.SiteSettings
            .AsNoTracking()
            .Where(setting => setting.IsActive);

        if (!string.IsNullOrWhiteSpace(category))
        {
            var normalizedCategory = category.Trim();
            settings = settings.Where(setting => setting.Category == normalizedCategory);
        }

        var items = await settings
            .OrderBy(setting => setting.Category)
            .ThenBy(setting => setting.SortOrder)
            .ThenBy(setting => setting.Key)
            .ToListAsync(cancellationToken);

        return items.Select(setting => setting.ToPublicDto()).ToArray();
    }

    public async Task<PublicSiteSettingDto?> GetSettingByKeyAsync(
        string key,
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(key))
        {
            return null;
        }

        var normalizedKey = key.Trim();
        var setting = await context.SiteSettings
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.IsActive && item.Key == normalizedKey, cancellationToken);

        return setting?.ToPublicDto();
    }

    private static string NormalizeLanguage(string? language)
    {
        return string.IsNullOrWhiteSpace(language) ? "en" : language.Trim().ToLowerInvariant();
    }
}
