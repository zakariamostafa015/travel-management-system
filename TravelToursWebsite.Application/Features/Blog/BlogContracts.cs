using FluentValidation;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Domain.Entities;

namespace TravelToursWebsite.Application.Features.Blog;

public sealed record BlogPostQuery : PagedQuery
{
    public string Language { get; init; } = "en";
    public int? CategoryId { get; init; }
    public bool? IsPublished { get; init; }
    public bool? IsFeatured { get; init; }
    public bool? IsEvent { get; init; }
}

public sealed record BlogCategoryQuery : PagedQuery
{
    public string Language { get; init; } = "en";
    public bool? IsActive { get; init; }
}

public sealed record BlogPostSummaryDto(
    int Id,
    string Title,
    string? Slug,
    string Excerpt,
    string? FeaturedImagePath,
    bool IsPublished,
    bool IsFeatured,
    bool IsEvent,
    DateTime? PublishedDate,
    int ViewCount,
    int CategoryId,
    string? CategoryName,
    int AuthorId,
    string? AuthorName,
    IReadOnlyList<BlogImageDto> Images);

public sealed record BlogPostDetailsDto(
    int Id,
    string Title,
    string? Slug,
    string Excerpt,
    string Content,
    string? MetaDescription,
    string? MetaKeywords,
    string? FeaturedImagePath,
    bool IsPublished,
    bool IsFeatured,
    bool IsEvent,
    DateTime? PublishedDate,
    DateTime CreatedDate,
    DateTime? UpdatedDate,
    int ViewCount,
    BlogCategoryDto? Category,
    int AuthorId,
    string? AuthorName,
    IReadOnlyList<BlogImageDto> Images);

public sealed record BlogCategoryDto(
    int Id,
    string Name,
    string? Description,
    string? Slug,
    string? IconClass,
    bool IsActive,
    int SortOrder);

public sealed record BlogImageDto(
    int Id,
    string ImagePath,
    string? ImageUrl,
    string? ImageLocalPath,
    string? ThumbnailPath,
    string? MediumPath,
    string? AltText,
    string? Caption,
    int SortOrder);

public sealed record BlogPostTranslationRequest(
    string Language,
    string Title,
    string Excerpt,
    string Content,
    string? Slug,
    string? MetaDescription,
    string? MetaKeywords);

public sealed record CreateBlogPostRequest(
    string? FeaturedImagePath,
    int CategoryId,
    int AuthorId,
    bool IsPublished,
    bool IsFeatured,
    bool IsEvent,
    DateTime? PublishedDate,
    IReadOnlyList<BlogPostTranslationRequest> Translations);

public sealed record UpdateBlogPostRequest(
    int Id,
    string? FeaturedImagePath,
    int CategoryId,
    int AuthorId,
    bool IsPublished,
    bool IsFeatured,
    bool IsEvent,
    DateTime? PublishedDate,
    IReadOnlyList<BlogPostTranslationRequest> Translations);

public sealed record BlogCategoryTranslationRequest(
    string Language,
    string Name,
    string? Description,
    string? Slug);

public sealed record CreateBlogCategoryRequest(
    string? IconClass,
    bool IsActive,
    int SortOrder,
    IReadOnlyList<BlogCategoryTranslationRequest> Translations);

public sealed record UpdateBlogCategoryRequest(
    int Id,
    string? IconClass,
    bool IsActive,
    int SortOrder,
    IReadOnlyList<BlogCategoryTranslationRequest> Translations);

public interface IBlogCatalogService
{
    Task<PagedResult<BlogPostSummaryDto>> GetPostsAsync(BlogPostQuery query, CancellationToken cancellationToken = default);
    Task<BlogPostDetailsDto?> GetPostByIdAsync(int id, string language = "en", CancellationToken cancellationToken = default);
    Task<BlogPostDetailsDto?> GetPostBySlugAsync(string slug, string language = "en", CancellationToken cancellationToken = default);
    Task<PagedResult<BlogCategoryDto>> GetCategoriesAsync(BlogCategoryQuery query, CancellationToken cancellationToken = default);
    Task<BlogCategoryDto?> GetCategoryByIdAsync(int id, string language = "en", CancellationToken cancellationToken = default);
    Task<BlogCategoryDto?> GetCategoryBySlugAsync(string slug, string language = "en", CancellationToken cancellationToken = default);
}

public interface IBlogManagementService
{
    Task<OperationResult<BlogPostDetailsDto>> CreatePostAsync(CreateBlogPostRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult<BlogPostDetailsDto>> UpdatePostAsync(UpdateBlogPostRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> DeletePostAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult<BlogCategoryDto>> CreateCategoryAsync(CreateBlogCategoryRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult<BlogCategoryDto>> UpdateCategoryAsync(UpdateBlogCategoryRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
    Task IncrementViewCountAsync(int postId, CancellationToken cancellationToken = default);
}

public static class BlogMappingExtensions
{
    private static string? ResolveBlogImage(BlogPost post)
    {
        if (!string.IsNullOrWhiteSpace(post.FeaturedImagePath))
        {
            return post.FeaturedImagePath;
        }

        var image = post.Images
            .OrderBy(item => item.SortOrder)
            .ThenBy(item => item.Id)
            .FirstOrDefault();

        return image?.ImageUrl ?? image?.ImagePath;
    }

    public static BlogPostSummaryDto ToSummaryDto(this BlogPost post, string language = "en")
    {
        var translation = post.Translations.FindBestTranslation(language);
        var categoryTranslation = post.Category?.Translations.FindBestTranslation(language);

        return new BlogPostSummaryDto(
            post.Id,
            translation?.Title ?? string.Empty,
            translation?.Slug,
            translation?.Excerpt ?? string.Empty,
            ResolveBlogImage(post),
            post.IsPublished,
            post.IsFeatured,
            post.IsEvent,
            post.PublishedDate,
            post.ViewCount,
            post.CategoryId,
            categoryTranslation?.Name,
            post.AuthorId,
            post.Author?.FullName(),
            post.Images.OrderBy(image => image.SortOrder).Select(image => image.ToDto()).ToArray());
    }

    public static BlogPostDetailsDto ToDetailsDto(this BlogPost post, string language = "en")
    {
        var translation = post.Translations.FindBestTranslation(language);

        return new BlogPostDetailsDto(
            post.Id,
            translation?.Title ?? string.Empty,
            translation?.Slug,
            translation?.Excerpt ?? string.Empty,
            translation?.Content ?? string.Empty,
            translation?.MetaDescription,
            translation?.MetaKeywords,
            ResolveBlogImage(post),
            post.IsPublished,
            post.IsFeatured,
            post.IsEvent,
            post.PublishedDate,
            post.CreatedDate,
            post.UpdatedDate,
            post.ViewCount,
            post.Category?.ToDto(language),
            post.AuthorId,
            post.Author?.FullName(),
            post.Images.OrderBy(image => image.SortOrder).Select(image => image.ToDto()).ToArray());
    }

    public static BlogCategoryDto ToDto(this BlogCategory category, string language = "en")
    {
        var translation = category.Translations.FindBestTranslation(language);

        return new BlogCategoryDto(
            category.Id,
            translation?.Name ?? string.Empty,
            translation?.Description,
            translation?.Slug,
            category.IconClass,
            category.IsActive,
            category.SortOrder);
    }

    public static BlogImageDto ToDto(this BlogImage image)
    {
        return new BlogImageDto(
            image.Id,
            image.ImagePath,
            image.ImageUrl,
            image.ImageLocalPath,
            image.ThumbnailPath,
            image.MediumPath,
            image.AltText,
            image.Caption,
            image.SortOrder);
    }

    private static string FullName(this User user)
    {
        return string.Join(' ', new[] { user.FirstName, user.LastName }.Where(value => !string.IsNullOrWhiteSpace(value)));
    }

    private static TTranslation? FindBestTranslation<TTranslation>(this IEnumerable<TTranslation> translations, string language)
        where TTranslation : class
    {
        return translations.FirstOrDefault(translation => GetLanguage(translation).Equals(language, StringComparison.OrdinalIgnoreCase))
            ?? translations.FirstOrDefault(translation => GetLanguage(translation).Equals("en", StringComparison.OrdinalIgnoreCase))
            ?? translations.FirstOrDefault();
    }

    private static string GetLanguage<TTranslation>(TTranslation translation)
    {
        return translation switch
        {
            BlogPostTranslation postTranslation => postTranslation.Language,
            BlogCategoryTranslation categoryTranslation => categoryTranslation.Language,
            _ => string.Empty
        };
    }
}

public sealed class BlogPostQueryValidator : PagedQueryValidator<BlogPostQuery>
{
    public BlogPostQueryValidator()
    {
        RuleFor(query => query.Language).NotEmpty().MaximumLength(10);
    }
}

public sealed class BlogCategoryQueryValidator : PagedQueryValidator<BlogCategoryQuery>
{
    public BlogCategoryQueryValidator()
    {
        RuleFor(query => query.Language).NotEmpty().MaximumLength(10);
    }
}

public sealed class BlogPostTranslationRequestValidator : AbstractValidator<BlogPostTranslationRequest>
{
    public BlogPostTranslationRequestValidator()
    {
        RuleFor(request => request.Language).NotEmpty().MaximumLength(10);
        RuleFor(request => request.Title).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Excerpt).NotEmpty().MaximumLength(500);
        RuleFor(request => request.Content).NotEmpty();
        RuleFor(request => request.Slug).MaximumLength(200);
        RuleFor(request => request.MetaDescription).MaximumLength(200);
        RuleFor(request => request.MetaKeywords).MaximumLength(200);
    }
}

public sealed class CreateBlogPostRequestValidator : AbstractValidator<CreateBlogPostRequest>
{
    public CreateBlogPostRequestValidator()
    {
        RuleFor(request => request.FeaturedImagePath).MaximumLength(500);
        RuleFor(request => request.CategoryId).GreaterThan(0);
        RuleFor(request => request.AuthorId).GreaterThan(0);
        RuleFor(request => request.Translations).NotEmpty();
        RuleForEach(request => request.Translations).SetValidator(new BlogPostTranslationRequestValidator());
    }
}

public sealed class UpdateBlogPostRequestValidator : AbstractValidator<UpdateBlogPostRequest>
{
    public UpdateBlogPostRequestValidator()
    {
        RuleFor(request => request.Id).GreaterThan(0);
        RuleFor(request => request.FeaturedImagePath).MaximumLength(500);
        RuleFor(request => request.CategoryId).GreaterThan(0);
        RuleFor(request => request.AuthorId).GreaterThan(0);
        RuleFor(request => request.Translations).NotEmpty();
        RuleForEach(request => request.Translations).SetValidator(new BlogPostTranslationRequestValidator());
    }
}

public sealed class BlogCategoryTranslationRequestValidator : AbstractValidator<BlogCategoryTranslationRequest>
{
    public BlogCategoryTranslationRequestValidator()
    {
        RuleFor(request => request.Language).NotEmpty().MaximumLength(10);
        RuleFor(request => request.Name).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Description).MaximumLength(500);
        RuleFor(request => request.Slug).MaximumLength(100);
    }
}

public sealed class CreateBlogCategoryRequestValidator : AbstractValidator<CreateBlogCategoryRequest>
{
    public CreateBlogCategoryRequestValidator()
    {
        RuleFor(request => request.IconClass).MaximumLength(200);
        RuleFor(request => request.Translations).NotEmpty();
        RuleForEach(request => request.Translations).SetValidator(new BlogCategoryTranslationRequestValidator());
    }
}

public sealed class UpdateBlogCategoryRequestValidator : AbstractValidator<UpdateBlogCategoryRequest>
{
    public UpdateBlogCategoryRequestValidator()
    {
        RuleFor(request => request.Id).GreaterThan(0);
        RuleFor(request => request.IconClass).MaximumLength(200);
        RuleFor(request => request.Translations).NotEmpty();
        RuleForEach(request => request.Translations).SetValidator(new BlogCategoryTranslationRequestValidator());
    }
}
