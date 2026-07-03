using FluentValidation;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Application.Features.Blog;
using TravelToursWebsite.Application.Features.Tours;

namespace TravelToursWebsite.Application.Features.AdminContent;

public sealed record TourTranslationDto(
    int Id,
    int TourId,
    string Language,
    string Title,
    string ShortDescription,
    string Description,
    string? Slug,
    string? MetaDescription,
    string? MetaKeywords,
    string? DurationUnit,
    string? ActivityHighlights);

public sealed record TourCategoryTranslationDto(
    int Id,
    int TourCategoryId,
    string Language,
    string Name,
    string? Description,
    string? Slug);

public sealed record TourItineraryTranslationDto(
    int Id,
    int TourItineraryId,
    string Language,
    string Title,
    string Description,
    string? Location,
    string? Accommodation,
    string? Meals);

public sealed record BlogPostTranslationDto(
    int Id,
    int BlogPostId,
    string Language,
    string Title,
    string Excerpt,
    string Content,
    string? Slug,
    string? MetaDescription,
    string? MetaKeywords);

public sealed record BlogCategoryTranslationDto(
    int Id,
    int BlogCategoryId,
    string Language,
    string Name,
    string? Description,
    string? Slug);

public sealed record UpsertTourTranslationRequest(int TourId, TourTranslationRequest Translation);
public sealed record UpsertTourCategoryTranslationRequest(int TourCategoryId, TourCategoryTranslationRequest Translation);
public sealed record UpsertTourItineraryTranslationRequest(int TourItineraryId, TourItineraryTranslationRequest Translation);
public sealed record UpsertBlogPostTranslationRequest(int BlogPostId, BlogPostTranslationRequest Translation);
public sealed record UpsertBlogCategoryTranslationRequest(int BlogCategoryId, BlogCategoryTranslationRequest Translation);

public sealed record TourImageRequest(
    int TourId,
    string ImagePath,
    string? ImageUrl,
    string? ImageLocalPath,
    string? ThumbnailPath,
    string? MediumPath,
    string? AltText,
    string? Caption,
    int SortOrder,
    bool IsMainImage);

public sealed record UpdateTourImageRequest(
    int Id,
    string ImagePath,
    string? ImageUrl,
    string? ImageLocalPath,
    string? ThumbnailPath,
    string? MediumPath,
    string? AltText,
    string? Caption,
    int SortOrder,
    bool IsMainImage);

public sealed record BlogImageRequest(
    int BlogPostId,
    string ImagePath,
    string? ImageUrl,
    string? ImageLocalPath,
    string? ThumbnailPath,
    string? MediumPath,
    string? AltText,
    string? Caption,
    int SortOrder);

public sealed record UpdateBlogImageRequest(
    int Id,
    string ImagePath,
    string? ImageUrl,
    string? ImageLocalPath,
    string? ThumbnailPath,
    string? MediumPath,
    string? AltText,
    string? Caption,
    int SortOrder);

public sealed record TourItineraryTranslationRequest(
    string Language,
    string Title,
    string Description,
    string? Location,
    string? Accommodation,
    string? Meals);

public sealed record UpsertTourItineraryRequest(
    int? Id,
    int TourId,
    int Day,
    int SortOrder,
    IReadOnlyList<TourItineraryTranslationRequest> Translations);

public sealed record UpsertTourSpotRequest(
    int? Id,
    int TourId,
    decimal Latitude,
    decimal Longitude,
    int Order,
    string? Name,
    string? Description);

public interface IAdminTourContentService
{
    Task<OperationResult<TourImageDto>> AddTourImageAsync(TourImageRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult<TourImageDto>> UpdateTourImageAsync(UpdateTourImageRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteTourImageAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult<TourItineraryDto>> UpsertItineraryAsync(UpsertTourItineraryRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteItineraryAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult<TourSpotDto>> UpsertSpotAsync(UpsertTourSpotRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteSpotAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TourTranslationDto>> GetTourTranslationsAsync(int tourId, CancellationToken cancellationToken = default);
    Task<OperationResult<TourTranslationDto>> UpsertTourTranslationAsync(UpsertTourTranslationRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TourCategoryTranslationDto>> GetTourCategoryTranslationsAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<OperationResult<TourCategoryTranslationDto>> UpsertTourCategoryTranslationAsync(UpsertTourCategoryTranslationRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<TourItineraryTranslationDto>> GetItineraryTranslationsAsync(int itineraryId, CancellationToken cancellationToken = default);
    Task<OperationResult<TourItineraryTranslationDto>> UpsertItineraryTranslationAsync(UpsertTourItineraryTranslationRequest request, CancellationToken cancellationToken = default);
}

public interface IAdminBlogContentService
{
    Task<OperationResult<BlogImageDto>> AddBlogImageAsync(BlogImageRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult<BlogImageDto>> UpdateBlogImageAsync(UpdateBlogImageRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteBlogImageAsync(int id, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BlogPostTranslationDto>> GetBlogPostTranslationsAsync(int postId, CancellationToken cancellationToken = default);
    Task<OperationResult<BlogPostTranslationDto>> UpsertBlogPostTranslationAsync(UpsertBlogPostTranslationRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<BlogCategoryTranslationDto>> GetBlogCategoryTranslationsAsync(int categoryId, CancellationToken cancellationToken = default);
    Task<OperationResult<BlogCategoryTranslationDto>> UpsertBlogCategoryTranslationAsync(UpsertBlogCategoryTranslationRequest request, CancellationToken cancellationToken = default);
}

public sealed class TourImageRequestValidator : AbstractValidator<TourImageRequest>
{
    public TourImageRequestValidator()
    {
        RuleFor(request => request.TourId).GreaterThan(0);
        RuleFor(request => request.ImagePath).NotEmpty().MaximumLength(500);
        RuleFor(request => request.ImageUrl).MaximumLength(500);
        RuleFor(request => request.ImageLocalPath).MaximumLength(500);
        RuleFor(request => request.ThumbnailPath).MaximumLength(500);
        RuleFor(request => request.MediumPath).MaximumLength(500);
        RuleFor(request => request.AltText).MaximumLength(200);
        RuleFor(request => request.Caption).MaximumLength(200);
    }
}

public sealed class UpdateTourImageRequestValidator : AbstractValidator<UpdateTourImageRequest>
{
    public UpdateTourImageRequestValidator()
    {
        RuleFor(request => request.Id).GreaterThan(0);
        RuleFor(request => request.ImagePath).NotEmpty().MaximumLength(500);
        RuleFor(request => request.ImageUrl).MaximumLength(500);
        RuleFor(request => request.ImageLocalPath).MaximumLength(500);
        RuleFor(request => request.ThumbnailPath).MaximumLength(500);
        RuleFor(request => request.MediumPath).MaximumLength(500);
        RuleFor(request => request.AltText).MaximumLength(200);
        RuleFor(request => request.Caption).MaximumLength(200);
    }
}

public sealed class BlogImageRequestValidator : AbstractValidator<BlogImageRequest>
{
    public BlogImageRequestValidator()
    {
        RuleFor(request => request.BlogPostId).GreaterThan(0);
        RuleFor(request => request.ImagePath).NotEmpty().MaximumLength(500);
        RuleFor(request => request.ImageUrl).MaximumLength(500);
        RuleFor(request => request.ImageLocalPath).MaximumLength(500);
        RuleFor(request => request.ThumbnailPath).MaximumLength(500);
        RuleFor(request => request.MediumPath).MaximumLength(500);
        RuleFor(request => request.AltText).MaximumLength(200);
        RuleFor(request => request.Caption).MaximumLength(200);
    }
}

public sealed class UpdateBlogImageRequestValidator : AbstractValidator<UpdateBlogImageRequest>
{
    public UpdateBlogImageRequestValidator()
    {
        RuleFor(request => request.Id).GreaterThan(0);
        RuleFor(request => request.ImagePath).NotEmpty().MaximumLength(500);
        RuleFor(request => request.ImageUrl).MaximumLength(500);
        RuleFor(request => request.ImageLocalPath).MaximumLength(500);
        RuleFor(request => request.ThumbnailPath).MaximumLength(500);
        RuleFor(request => request.MediumPath).MaximumLength(500);
        RuleFor(request => request.AltText).MaximumLength(200);
        RuleFor(request => request.Caption).MaximumLength(200);
    }
}

public sealed class TourItineraryTranslationRequestValidator : AbstractValidator<TourItineraryTranslationRequest>
{
    public TourItineraryTranslationRequestValidator()
    {
        RuleFor(request => request.Language).NotEmpty().MaximumLength(10);
        RuleFor(request => request.Title).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Description).NotEmpty();
        RuleFor(request => request.Location).MaximumLength(200);
        RuleFor(request => request.Accommodation).MaximumLength(200);
        RuleFor(request => request.Meals).MaximumLength(200);
    }
}

public sealed class UpsertTourItineraryRequestValidator : AbstractValidator<UpsertTourItineraryRequest>
{
    public UpsertTourItineraryRequestValidator()
    {
        RuleFor(request => request.Id).GreaterThan(0).When(request => request.Id.HasValue);
        RuleFor(request => request.TourId).GreaterThan(0);
        RuleFor(request => request.Day).GreaterThan(0);
        RuleFor(request => request.Translations).NotEmpty();
        RuleForEach(request => request.Translations).SetValidator(new TourItineraryTranslationRequestValidator());
    }
}

public sealed class UpsertTourSpotRequestValidator : AbstractValidator<UpsertTourSpotRequest>
{
    public UpsertTourSpotRequestValidator()
    {
        RuleFor(request => request.Id).GreaterThan(0).When(request => request.Id.HasValue);
        RuleFor(request => request.TourId).GreaterThan(0);
        RuleFor(request => request.Latitude).InclusiveBetween(-90, 90);
        RuleFor(request => request.Longitude).InclusiveBetween(-180, 180);
        RuleFor(request => request.Order).GreaterThanOrEqualTo(0);
        RuleFor(request => request.Name).MaximumLength(200);
        RuleFor(request => request.Description).MaximumLength(1000);
    }
}
