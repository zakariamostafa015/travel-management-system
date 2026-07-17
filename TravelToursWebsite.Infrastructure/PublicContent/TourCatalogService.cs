using Microsoft.EntityFrameworkCore;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Application.Features.Tours;
using TravelToursWebsite.Domain.Entities;
using TravelToursWebsite.Infrastructure.Persistence;

namespace TravelToursWebsite.Infrastructure.PublicContent;

public sealed class TourCatalogService(ApplicationDbContext context) : ITourCatalogService
{
    private const int MaxPageSize = 100;

    public async Task<PagedResult<TourSummaryDto>> GetToursAsync(
        TourQuery query,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = NormalizePageNumber(query.PageNumber);
        var pageSize = NormalizePageSize(query.PageSize);
        var language = NormalizeLanguage(query.Language);

        var tours = context.Tours
            .AsNoTracking()
            .Where(tour => tour.IsActive);

        if (query.CategoryId.HasValue)
        {
            tours = tours.Where(tour => tour.CategoryId == query.CategoryId.Value);
        }

        if (query.IsFeatured.HasValue)
        {
            tours = tours.Where(tour => tour.IsFeatured == query.IsFeatured.Value);
        }

        if (query.IsPackage.HasValue)
        {
            tours = tours.Where(tour => tour.IsPackage == query.IsPackage.Value);
        }

        tours = ApplySearch(tours, query.SearchTerm);
        var totalCount = await tours.CountAsync(cancellationToken);
        if (totalCount == 0)
        {
            return PagedResult<TourSummaryDto>.Empty(pageNumber, pageSize);
        }

        var items = await ApplyTourSorting(tours, query.SortBy, query.SortDirection, language)
            .Include(tour => tour.Category)
                .ThenInclude(category => category!.Translations)
            .Include(tour => tour.Translations)
            .Include(tour => tour.Images)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TourSummaryDto>(
            items.Select(tour => tour.ToSummaryDto(language)).ToArray(),
            pageNumber,
            pageSize,
            totalCount);
    }

    public async Task<TourDetailsDto?> GetTourByIdAsync(
        int id,
        string language = "en",
        CancellationToken cancellationToken = default)
    {
        var tour = await CreateTourDetailsQuery()
            .FirstOrDefaultAsync(item => item.Id == id && item.IsActive, cancellationToken);

        return tour?.ToDetailsDto(NormalizeLanguage(language));
    }

    public async Task<TourDetailsDto?> GetTourBySlugAsync(
        string slug,
        string language = "en",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        var normalizedSlug = slug.Trim();
        var normalizedLanguage = NormalizeLanguage(language);
        var tour = await FindTourBySlugAsync(normalizedSlug, normalizedLanguage, cancellationToken);
        if (tour is not null)
        {
            return tour.ToDetailsDto(normalizedLanguage);
        }

        var defaultLanguage = await GetDefaultLanguageAsync(cancellationToken);
        if (!defaultLanguage.Equals(normalizedLanguage, StringComparison.OrdinalIgnoreCase))
        {
            tour = await FindTourBySlugAsync(normalizedSlug, defaultLanguage, cancellationToken);
        }

        return tour?.ToDetailsDto(normalizedLanguage);
    }

    public async Task<PagedResult<TourCategoryDto>> GetCategoriesAsync(
        TourCategoryQuery query,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = NormalizePageNumber(query.PageNumber);
        var pageSize = NormalizePageSize(query.PageSize);
        var language = NormalizeLanguage(query.Language);

        var categories = context.TourCategories
            .AsNoTracking()
            .Where(category => category.IsActive);

        categories = ApplyCategorySearch(categories, query.SearchTerm);
        var totalCount = await categories.CountAsync(cancellationToken);
        if (totalCount == 0)
        {
            return PagedResult<TourCategoryDto>.Empty(pageNumber, pageSize);
        }

        var items = await ApplyTourCategorySorting(categories, query.SortBy, query.SortDirection, language)
            .Include(category => category.Translations)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<TourCategoryDto>(
            items.Select(category => category.ToDto(language)).ToArray(),
            pageNumber,
            pageSize,
            totalCount);
    }

    public async Task<TourCategoryDto?> GetCategoryByIdAsync(
        int id,
        string language = "en",
        CancellationToken cancellationToken = default)
    {
        var category = await context.TourCategories
            .AsNoTracking()
            .Include(item => item.Translations)
            .FirstOrDefaultAsync(item => item.Id == id && item.IsActive, cancellationToken);

        return category?.ToDto(NormalizeLanguage(language));
    }

    public async Task<TourCategoryDto?> GetCategoryBySlugAsync(
        string slug,
        string language = "en",
        CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(slug))
        {
            return null;
        }

        var normalizedSlug = slug.Trim();
        var normalizedLanguage = NormalizeLanguage(language);
        var category = await FindCategoryBySlugAsync(normalizedSlug, normalizedLanguage, cancellationToken);
        if (category is not null)
        {
            return category.ToDto(normalizedLanguage);
        }

        var defaultLanguage = await GetDefaultLanguageAsync(cancellationToken);
        if (!defaultLanguage.Equals(normalizedLanguage, StringComparison.OrdinalIgnoreCase))
        {
            category = await FindCategoryBySlugAsync(normalizedSlug, defaultLanguage, cancellationToken);
        }

        return category?.ToDto(normalizedLanguage);
    }

    private IQueryable<Tour> CreateTourDetailsQuery()
    {
        return context.Tours
            .AsNoTracking()
            .Include(tour => tour.Category)
                .ThenInclude(category => category!.Translations)
            .Include(tour => tour.Images)
            .Include(tour => tour.Itineraries)
                .ThenInclude(itinerary => itinerary.Translations)
            .Include(tour => tour.Spots)
            .Include(tour => tour.Translations);
    }

    private Task<Tour?> FindTourBySlugAsync(string slug, string language, CancellationToken cancellationToken)
    {
        return CreateTourDetailsQuery()
            .FirstOrDefaultAsync(
                tour => tour.IsActive
                    && tour.Translations.Any(translation => translation.Slug == slug && translation.Language == language),
                cancellationToken);
    }

    private Task<TourCategory?> FindCategoryBySlugAsync(string slug, string language, CancellationToken cancellationToken)
    {
        return context.TourCategories
            .AsNoTracking()
            .Include(category => category.Translations)
            .FirstOrDefaultAsync(
                category => category.IsActive
                    && category.Translations.Any(translation => translation.Slug == slug && translation.Language == language),
                cancellationToken);
    }

    private async Task<string> GetDefaultLanguageAsync(CancellationToken cancellationToken)
    {
        return await context.Languages
            .AsNoTracking()
            .Where(language => language.IsDefault)
            .Select(language => language.Code)
            .FirstOrDefaultAsync(cancellationToken)
            ?? "en";
    }

    private static IQueryable<Tour> ApplySearch(IQueryable<Tour> tours, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return tours;
        }

        var term = searchTerm.Trim();
        return tours.Where(tour =>
            tour.Translations.Any(translation =>
                translation.Title.Contains(term)
                || translation.ShortDescription.Contains(term)
                || translation.Description.Contains(term))
            || tour.Category != null
                && tour.Category.Translations.Any(translation => translation.Name.Contains(term)));
    }

    private static IQueryable<TourCategory> ApplyCategorySearch(IQueryable<TourCategory> categories, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return categories;
        }

        var term = searchTerm.Trim();
        return categories.Where(category =>
            category.Translations.Any(translation =>
                translation.Name.Contains(term)
                || translation.Description != null && translation.Description.Contains(term)));
    }

    private static IQueryable<Tour> ApplyTourSorting(
        IQueryable<Tour> tours,
        string? sortBy,
        SortDirection sortDirection,
        string language)
    {
        var descending = sortDirection == SortDirection.Descending;
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "title" => descending
                ? tours.OrderByDescending(tour => tour.Translations.Where(translation => translation.Language == language).Select(translation => translation.Title).FirstOrDefault()).ThenByDescending(tour => tour.Id)
                : tours.OrderBy(tour => tour.Translations.Where(translation => translation.Language == language).Select(translation => translation.Title).FirstOrDefault()).ThenBy(tour => tour.Id),
            "price" => descending
                ? tours.OrderByDescending(tour => tour.Price).ThenByDescending(tour => tour.Id)
                : tours.OrderBy(tour => tour.Price).ThenBy(tour => tour.Id),
            "duration" => descending
                ? tours.OrderByDescending(tour => tour.Duration).ThenByDescending(tour => tour.Id)
                : tours.OrderBy(tour => tour.Duration).ThenBy(tour => tour.Id),
            "createddate" => descending
                ? tours.OrderByDescending(tour => tour.CreatedDate).ThenByDescending(tour => tour.Id)
                : tours.OrderBy(tour => tour.CreatedDate).ThenBy(tour => tour.Id),
            _ => descending
                ? tours.OrderByDescending(tour => tour.SortOrder).ThenByDescending(tour => tour.Id)
                : tours.OrderBy(tour => tour.SortOrder).ThenBy(tour => tour.Id)
        };
    }

    private static IQueryable<TourCategory> ApplyTourCategorySorting(
        IQueryable<TourCategory> categories,
        string? sortBy,
        SortDirection sortDirection,
        string language)
    {
        var descending = sortDirection == SortDirection.Descending;
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "name" => descending
                ? categories.OrderByDescending(category => category.Translations.Where(translation => translation.Language == language).Select(translation => translation.Name).FirstOrDefault()).ThenByDescending(category => category.Id)
                : categories.OrderBy(category => category.Translations.Where(translation => translation.Language == language).Select(translation => translation.Name).FirstOrDefault()).ThenBy(category => category.Id),
            "createddate" => descending
                ? categories.OrderByDescending(category => category.CreatedDate).ThenByDescending(category => category.Id)
                : categories.OrderBy(category => category.CreatedDate).ThenBy(category => category.Id),
            _ => descending
                ? categories.OrderByDescending(category => category.SortOrder).ThenByDescending(category => category.Id)
                : categories.OrderBy(category => category.SortOrder).ThenBy(category => category.Id)
        };
    }

    private static int NormalizePageNumber(int pageNumber)
    {
        return Math.Max(1, pageNumber);
    }

    private static int NormalizePageSize(int pageSize)
    {
        return Math.Clamp(pageSize, 1, MaxPageSize);
    }

    private static string NormalizeLanguage(string? language)
    {
        return string.IsNullOrWhiteSpace(language) ? "en" : language.Trim().ToLowerInvariant();
    }
}
