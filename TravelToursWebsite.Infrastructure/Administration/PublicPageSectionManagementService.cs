using Microsoft.EntityFrameworkCore;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Application.Features.Administration;
using TravelToursWebsite.Domain.Entities;
using TravelToursWebsite.Infrastructure.Persistence;

namespace TravelToursWebsite.Infrastructure.Administration;

public sealed class PublicPageSectionManagementService(ApplicationDbContext context) : IPublicPageSectionManagementService
{
    private const int MaxPageSize = 100;

    public async Task<PagedResult<AdminPublicPageSectionDto>> GetSectionsAsync(
        PublicPageSectionQuery query,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = Math.Max(1, query.PageNumber);
        var pageSize = Math.Clamp(query.PageSize, 1, MaxPageSize);
        var sections = context.PublicPageSections
            .AsNoTracking()
            .Include(section => section.Translations)
            .Include(section => section.Items)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.PageKey))
        {
            var pageKey = NormalizeKey(query.PageKey);
            sections = sections.Where(section => section.PageKey == pageKey);
        }

        if (query.IsActive.HasValue)
        {
            sections = sections.Where(section => section.IsActive == query.IsActive.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.SearchTerm))
        {
            var term = query.SearchTerm.Trim();
            sections = sections.Where(section =>
                section.PageKey.Contains(term)
                || section.SectionKey.Contains(term)
                || section.Translations.Any(translation => translation.Title.Contains(term)));
        }

        var totalCount = await sections.CountAsync(cancellationToken);
        if (totalCount == 0)
        {
            return PagedResult<AdminPublicPageSectionDto>.Empty(pageNumber, pageSize);
        }

        var items = await sections
            .OrderBy(section => section.PageKey)
            .ThenBy(section => section.SortOrder)
            .ThenBy(section => section.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<AdminPublicPageSectionDto>(
            items.Select(section => section.ToAdminDto()).ToArray(),
            pageNumber,
            pageSize,
            totalCount);
    }

    public async Task<AdminPublicPageSectionDto?> GetSectionByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        var section = await LoadSectionAsync(id, cancellationToken);
        return section?.ToAdminDto();
    }

    public async Task<OperationResult<AdminPublicPageSectionDto>> UpsertSectionAsync(
        UpsertPublicPageSectionRequest request,
        CancellationToken cancellationToken = default)
    {
        var pageKey = NormalizeKey(request.PageKey);
        var sectionKey = NormalizeKey(request.SectionKey);

        if (await context.PublicPageSections.AnyAsync(
                section => section.PageKey == pageKey && section.SectionKey == sectionKey && section.Id != request.Id,
                cancellationToken))
        {
            return OperationResult<AdminPublicPageSectionDto>.Failure("A public page section with this page and section key already exists.");
        }

        var linkValidation = await ValidateLinkedContentAsync(request, cancellationToken);
        if (!linkValidation.Succeeded)
        {
            return OperationResult<AdminPublicPageSectionDto>.Failure(linkValidation.Message ?? "Linked content was not found.");
        }

        PublicPageSection section;
        var created = !request.Id.HasValue;
        if (request.Id.HasValue)
        {
            var existing = await context.PublicPageSections
                .Include(item => item.Translations)
                .Include(item => item.Items)
                .FirstOrDefaultAsync(item => item.Id == request.Id.Value, cancellationToken);

            if (existing is null)
            {
                return OperationResult<AdminPublicPageSectionDto>.Failure("Public page section was not found.");
            }

            section = existing;
            section.UpdatedDate = DateTime.UtcNow;
        }
        else
        {
            section = new PublicPageSection { CreatedDate = DateTime.UtcNow };
            context.PublicPageSections.Add(section);
        }

        section.PageKey = pageKey;
        section.SectionKey = sectionKey;
        section.LayoutVariant = request.LayoutVariant.Trim();
        section.Theme = NormalizeOptional(request.Theme);
        section.DesktopMediaUrl = NormalizeOptional(request.DesktopMediaUrl);
        section.MobileMediaUrl = NormalizeOptional(request.MobileMediaUrl);
        section.MediaAlt = NormalizeOptional(request.MediaAlt);
        section.CtaLabel = NormalizeOptional(request.CtaLabel);
        section.CtaUrl = NormalizeOptional(request.CtaUrl);
        section.LinkedTourId = request.LinkedTourId;
        section.LinkedTourCategoryId = request.LinkedTourCategoryId;
        section.LinkedBlogPostId = request.LinkedBlogPostId;
        section.SortOrder = request.SortOrder;
        section.IsActive = request.IsActive;

        UpsertTranslations(section.Translations, request.Translations);
        ReplaceItems(section.Items, request.Items);
        await context.SaveChangesAsync(cancellationToken);

        var loaded = await LoadSectionAsync(section.Id, cancellationToken);
        return OperationResult<AdminPublicPageSectionDto>.Success(
            loaded!.ToAdminDto(),
            created ? "Public page section created." : "Public page section updated.");
    }

    public async Task<OperationResult> DeleteSectionAsync(int id, CancellationToken cancellationToken = default)
    {
        var section = await context.PublicPageSections.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (section is null)
        {
            return OperationResult.Failure("Public page section was not found.");
        }

        context.PublicPageSections.Remove(section);
        await context.SaveChangesAsync(cancellationToken);
        return OperationResult.Success("Public page section deleted.");
    }

    private async Task<PublicPageSection?> LoadSectionAsync(int id, CancellationToken cancellationToken)
    {
        return await context.PublicPageSections
            .AsNoTracking()
            .Include(section => section.Translations)
            .Include(section => section.Items)
            .FirstOrDefaultAsync(section => section.Id == id, cancellationToken);
    }

    private async Task<OperationResult> ValidateLinkedContentAsync(
        UpsertPublicPageSectionRequest request,
        CancellationToken cancellationToken)
    {
        if (request.LinkedTourId.HasValue && !await context.Tours.AnyAsync(item => item.Id == request.LinkedTourId.Value, cancellationToken))
        {
            return OperationResult.Failure("Linked tour was not found.");
        }

        if (request.LinkedTourCategoryId.HasValue && !await context.TourCategories.AnyAsync(item => item.Id == request.LinkedTourCategoryId.Value, cancellationToken))
        {
            return OperationResult.Failure("Linked tour category was not found.");
        }

        if (request.LinkedBlogPostId.HasValue && !await context.BlogPosts.AnyAsync(item => item.Id == request.LinkedBlogPostId.Value, cancellationToken))
        {
            return OperationResult.Failure("Linked blog post was not found.");
        }

        return OperationResult.Success();
    }

    private static void UpsertTranslations(
        ICollection<PublicPageSectionTranslation> existing,
        IEnumerable<PublicPageSectionTranslationRequest> requests)
    {
        var requestedLanguages = requests.Select(request => NormalizeKey(request.Language)).ToHashSet();
        foreach (var stale in existing.Where(item => !requestedLanguages.Contains(item.Language)).ToArray())
        {
            existing.Remove(stale);
        }

        foreach (var request in requests)
        {
            var language = NormalizeKey(request.Language);
            var translation = existing.FirstOrDefault(item => item.Language == language);
            if (translation is null)
            {
                translation = new PublicPageSectionTranslation { Language = language };
                existing.Add(translation);
            }

            translation.Eyebrow = NormalizeOptional(request.Eyebrow);
            translation.Title = request.Title.Trim();
            translation.Subtitle = NormalizeOptional(request.Subtitle);
            translation.Body = NormalizeOptional(request.Body);
            translation.SupportingCopy = NormalizeOptional(request.SupportingCopy);
        }
    }

    private static void ReplaceItems(
        ICollection<PublicPageSectionItem> existing,
        IEnumerable<PublicPageSectionItemRequest> requests)
    {
        existing.Clear();
        foreach (var request in requests)
        {
            existing.Add(new PublicPageSectionItem
            {
                ItemKey = NormalizeOptional(request.ItemKey),
                Label = NormalizeOptional(request.Label),
                Value = NormalizeOptional(request.Value),
                Description = NormalizeOptional(request.Description),
                Url = NormalizeOptional(request.Url),
                IconClass = NormalizeOptional(request.IconClass),
                SortOrder = request.SortOrder,
                IsActive = request.IsActive
            });
        }
    }

    private static string NormalizeKey(string value) => value.Trim().ToLowerInvariant();
    private static string? NormalizeOptional(string? value) => string.IsNullOrWhiteSpace(value) ? null : value.Trim();
}
