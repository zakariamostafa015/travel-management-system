using Microsoft.EntityFrameworkCore;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Application.Features.Blog;
using TravelToursWebsite.Domain.Entities;
using TravelToursWebsite.Infrastructure.Persistence;

namespace TravelToursWebsite.Infrastructure.PublicContent;

public sealed class BlogCatalogService(ApplicationDbContext context) : IBlogCatalogService
{
    private const int MaxPageSize = 100;

    public async Task<PagedResult<BlogPostSummaryDto>> GetPostsAsync(
        BlogPostQuery query,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = NormalizePageNumber(query.PageNumber);
        var pageSize = NormalizePageSize(query.PageSize);
        var language = NormalizeLanguage(query.Language);

        var posts = context.BlogPosts
            .AsNoTracking()
            .Where(post => post.IsPublished);

        if (query.CategoryId.HasValue)
        {
            posts = posts.Where(post => post.CategoryId == query.CategoryId.Value);
        }

        if (query.IsFeatured.HasValue)
        {
            posts = posts.Where(post => post.IsFeatured == query.IsFeatured.Value);
        }

        if (query.IsEvent.HasValue)
        {
            posts = posts.Where(post => post.IsEvent == query.IsEvent.Value);
        }

        posts = ApplySearch(posts, query.SearchTerm);
        var totalCount = await posts.CountAsync(cancellationToken);
        if (totalCount == 0)
        {
            return PagedResult<BlogPostSummaryDto>.Empty(pageNumber, pageSize);
        }

        var items = await ApplyPostSorting(posts, query.SortBy, query.SortDirection, language)
            .Include(post => post.Category)
                .ThenInclude(category => category!.Translations)
            .Include(post => post.Author)
            .Include(post => post.Translations)
            .Include(post => post.Images)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<BlogPostSummaryDto>(
            items.Select(post => post.ToSummaryDto(language)).ToArray(),
            pageNumber,
            pageSize,
            totalCount);
    }

    public async Task<BlogPostDetailsDto?> GetPostByIdAsync(
        int id,
        string language = "en",
        CancellationToken cancellationToken = default)
    {
        var post = await CreatePostDetailsQuery()
            .FirstOrDefaultAsync(item => item.Id == id && item.IsPublished, cancellationToken);

        return post?.ToDetailsDto(NormalizeLanguage(language));
    }

    public async Task<BlogPostDetailsDto?> GetPostBySlugAsync(
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
        var post = await FindPostBySlugAsync(normalizedSlug, normalizedLanguage, cancellationToken);
        if (post is not null)
        {
            return post.ToDetailsDto(normalizedLanguage);
        }

        var defaultLanguage = await GetDefaultLanguageAsync(cancellationToken);
        if (!defaultLanguage.Equals(normalizedLanguage, StringComparison.OrdinalIgnoreCase))
        {
            post = await FindPostBySlugAsync(normalizedSlug, defaultLanguage, cancellationToken);
        }

        return post?.ToDetailsDto(normalizedLanguage);
    }

    public async Task<PagedResult<BlogCategoryDto>> GetCategoriesAsync(
        BlogCategoryQuery query,
        CancellationToken cancellationToken = default)
    {
        var pageNumber = NormalizePageNumber(query.PageNumber);
        var pageSize = NormalizePageSize(query.PageSize);
        var language = NormalizeLanguage(query.Language);

        var categories = context.BlogCategories
            .AsNoTracking()
            .Where(category => category.IsActive);

        categories = ApplyCategorySearch(categories, query.SearchTerm);
        var totalCount = await categories.CountAsync(cancellationToken);
        if (totalCount == 0)
        {
            return PagedResult<BlogCategoryDto>.Empty(pageNumber, pageSize);
        }

        var items = await ApplyBlogCategorySorting(categories, query.SortBy, query.SortDirection, language)
            .Include(category => category.Translations)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<BlogCategoryDto>(
            items.Select(category => category.ToDto(language)).ToArray(),
            pageNumber,
            pageSize,
            totalCount);
    }

    public async Task<BlogCategoryDto?> GetCategoryByIdAsync(
        int id,
        string language = "en",
        CancellationToken cancellationToken = default)
    {
        var category = await context.BlogCategories
            .AsNoTracking()
            .Include(item => item.Translations)
            .FirstOrDefaultAsync(item => item.Id == id && item.IsActive, cancellationToken);

        return category?.ToDto(NormalizeLanguage(language));
    }

    public async Task<BlogCategoryDto?> GetCategoryBySlugAsync(
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

    private IQueryable<BlogPost> CreatePostDetailsQuery()
    {
        return context.BlogPosts
            .AsNoTracking()
            .Include(post => post.Category)
                .ThenInclude(category => category!.Translations)
            .Include(post => post.Author)
            .Include(post => post.Images)
            .Include(post => post.Translations);
    }

    private Task<BlogPost?> FindPostBySlugAsync(string slug, string language, CancellationToken cancellationToken)
    {
        return CreatePostDetailsQuery()
            .FirstOrDefaultAsync(
                post => post.IsPublished
                    && post.Translations.Any(translation => translation.Slug == slug && translation.Language == language),
                cancellationToken);
    }

    private Task<BlogCategory?> FindCategoryBySlugAsync(string slug, string language, CancellationToken cancellationToken)
    {
        return context.BlogCategories
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

    private static IQueryable<BlogPost> ApplySearch(IQueryable<BlogPost> posts, string? searchTerm)
    {
        if (string.IsNullOrWhiteSpace(searchTerm))
        {
            return posts;
        }

        var term = searchTerm.Trim();
        return posts.Where(post =>
            post.Translations.Any(translation =>
                translation.Title.Contains(term)
                || translation.Excerpt.Contains(term)
                || translation.Content.Contains(term))
            || post.Category != null
                && post.Category.Translations.Any(translation => translation.Name.Contains(term)));
    }

    private static IQueryable<BlogCategory> ApplyCategorySearch(IQueryable<BlogCategory> categories, string? searchTerm)
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

    private static IQueryable<BlogPost> ApplyPostSorting(
        IQueryable<BlogPost> posts,
        string? sortBy,
        SortDirection sortDirection,
        string language)
    {
        var descending = sortDirection == SortDirection.Descending;
        return sortBy?.Trim().ToLowerInvariant() switch
        {
            "title" => descending
                ? posts.OrderByDescending(post => post.Translations.Where(translation => translation.Language == language).Select(translation => translation.Title).FirstOrDefault()).ThenByDescending(post => post.Id)
                : posts.OrderBy(post => post.Translations.Where(translation => translation.Language == language).Select(translation => translation.Title).FirstOrDefault()).ThenBy(post => post.Id),
            "viewcount" => descending
                ? posts.OrderByDescending(post => post.ViewCount).ThenByDescending(post => post.Id)
                : posts.OrderBy(post => post.ViewCount).ThenBy(post => post.Id),
            "createddate" => descending
                ? posts.OrderByDescending(post => post.CreatedDate).ThenByDescending(post => post.Id)
                : posts.OrderBy(post => post.CreatedDate).ThenBy(post => post.Id),
            null or "" => posts.OrderByDescending(post => post.PublishedDate).ThenByDescending(post => post.Id),
            _ => descending
                ? posts.OrderByDescending(post => post.PublishedDate).ThenByDescending(post => post.Id)
                : posts.OrderBy(post => post.PublishedDate).ThenBy(post => post.Id)
        };
    }

    private static IQueryable<BlogCategory> ApplyBlogCategorySorting(
        IQueryable<BlogCategory> categories,
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
