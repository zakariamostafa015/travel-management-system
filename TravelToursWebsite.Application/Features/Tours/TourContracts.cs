using FluentValidation;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Domain.Entities;

namespace TravelToursWebsite.Application.Features.Tours;

public sealed record TourQuery : PagedQuery
{
    public string Language { get; init; } = "en";
    public int? CategoryId { get; init; }
    public bool? IsActive { get; init; }
    public bool? IsFeatured { get; init; }
    public bool? IsPackage { get; init; }
}

public sealed record TourCategoryQuery : PagedQuery
{
    public string Language { get; init; } = "en";
    public bool? IsActive { get; init; }
}

public sealed record TourSummaryDto(
    int Id,
    string Title,
    string? Slug,
    string ShortDescription,
    decimal? Price,
    int Duration,
    string? DurationText,
    bool IsPackage,
    string? FeaturedImagePath,
    bool IsActive,
    bool IsFeatured,
    int SortOrder,
    int CategoryId,
    string? CategoryName);

public sealed record TourDetailsDto(
    int Id,
    string Title,
    string? Slug,
    string ShortDescription,
    string Description,
    decimal? Price,
    int Duration,
    string? DurationText,
    string? DurationUnit,
    string? ActivityHighlights,
    bool IsPackage,
    string? FeaturedImagePath,
    bool IsActive,
    bool IsFeatured,
    int SortOrder,
    DateTime CreatedDate,
    DateTime? UpdatedDate,
    TourCategoryDto? Category,
    IReadOnlyList<TourImageDto> Images,
    IReadOnlyList<TourItineraryDto> Itineraries,
    IReadOnlyList<TourSpotDto> Spots);

public sealed record TourCategoryDto(
    int Id,
    string Name,
    string? Description,
    string? Slug,
    string? IconClass,
    bool IsActive,
    int SortOrder);

public sealed record TourImageDto(
    int Id,
    string ImagePath,
    string? ThumbnailPath,
    string? MediumPath,
    string? AltText,
    string? Caption,
    int SortOrder,
    bool IsMainImage);

public sealed record TourItineraryDto(
    int Id,
    int Day,
    int SortOrder,
    string Title,
    string Description,
    string? Location,
    string? Accommodation,
    string? Meals);

public sealed record TourSpotDto(
    int Id,
    decimal Latitude,
    decimal Longitude,
    int Order,
    string? Name,
    string? Description);

public sealed record TourTranslationRequest(
    string Language,
    string Title,
    string ShortDescription,
    string Description,
    string? Slug,
    string? MetaDescription,
    string? MetaKeywords,
    string? DurationUnit,
    string? ActivityHighlights);

public sealed record CreateTourRequest(
    decimal? Price,
    int Duration,
    bool IsPackage,
    string? DurationText,
    int CategoryId,
    string? FeaturedImagePath,
    bool IsActive,
    bool IsFeatured,
    int SortOrder,
    IReadOnlyList<TourTranslationRequest> Translations);

public sealed record UpdateTourRequest(
    int Id,
    decimal? Price,
    int Duration,
    bool IsPackage,
    string? DurationText,
    int CategoryId,
    string? FeaturedImagePath,
    bool IsActive,
    bool IsFeatured,
    int SortOrder,
    IReadOnlyList<TourTranslationRequest> Translations);

public sealed record TourCategoryTranslationRequest(
    string Language,
    string Name,
    string? Description,
    string? Slug);

public sealed record CreateTourCategoryRequest(
    string? IconClass,
    bool IsActive,
    int SortOrder,
    IReadOnlyList<TourCategoryTranslationRequest> Translations);

public sealed record UpdateTourCategoryRequest(
    int Id,
    string? IconClass,
    bool IsActive,
    int SortOrder,
    IReadOnlyList<TourCategoryTranslationRequest> Translations);

public interface ITourCatalogService
{
    Task<PagedResult<TourSummaryDto>> GetToursAsync(TourQuery query, CancellationToken cancellationToken = default);
    Task<TourDetailsDto?> GetTourByIdAsync(int id, string language = "en", CancellationToken cancellationToken = default);
    Task<TourDetailsDto?> GetTourBySlugAsync(string slug, string language = "en", CancellationToken cancellationToken = default);
    Task<PagedResult<TourCategoryDto>> GetCategoriesAsync(TourCategoryQuery query, CancellationToken cancellationToken = default);
    Task<TourCategoryDto?> GetCategoryByIdAsync(int id, string language = "en", CancellationToken cancellationToken = default);
    Task<TourCategoryDto?> GetCategoryBySlugAsync(string slug, string language = "en", CancellationToken cancellationToken = default);
}

public interface ITourManagementService
{
    Task<OperationResult<TourDetailsDto>> CreateTourAsync(CreateTourRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult<TourDetailsDto>> UpdateTourAsync(UpdateTourRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteTourAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult<TourCategoryDto>> CreateCategoryAsync(CreateTourCategoryRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult<TourCategoryDto>> UpdateCategoryAsync(UpdateTourCategoryRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteCategoryAsync(int id, CancellationToken cancellationToken = default);
}

public static class TourMappingExtensions
{
    public static TourSummaryDto ToSummaryDto(this Tour tour, string language = "en")
    {
        var translation = tour.Translations.FindBestTranslation(language);
        var categoryTranslation = tour.Category?.Translations.FindBestTranslation(language);

        return new TourSummaryDto(
            tour.Id,
            translation?.Title ?? string.Empty,
            translation?.Slug,
            translation?.ShortDescription ?? string.Empty,
            tour.Price,
            tour.Duration,
            tour.DurationText,
            tour.IsPackage,
            tour.FeaturedImagePath,
            tour.IsActive,
            tour.IsFeatured,
            tour.SortOrder,
            tour.CategoryId,
            categoryTranslation?.Name);
    }

    public static TourDetailsDto ToDetailsDto(this Tour tour, string language = "en")
    {
        var translation = tour.Translations.FindBestTranslation(language);

        return new TourDetailsDto(
            tour.Id,
            translation?.Title ?? string.Empty,
            translation?.Slug,
            translation?.ShortDescription ?? string.Empty,
            translation?.Description ?? string.Empty,
            tour.Price,
            tour.Duration,
            tour.DurationText,
            translation?.DurationUnit,
            translation?.ActivityHighlights,
            tour.IsPackage,
            tour.FeaturedImagePath,
            tour.IsActive,
            tour.IsFeatured,
            tour.SortOrder,
            tour.CreatedDate,
            tour.UpdatedDate,
            tour.Category?.ToDto(language),
            tour.Images.OrderBy(image => image.SortOrder).Select(image => image.ToDto()).ToArray(),
            tour.Itineraries.OrderBy(itinerary => itinerary.Day).ThenBy(itinerary => itinerary.SortOrder).Select(itinerary => itinerary.ToDto(language)).ToArray(),
            tour.Spots.OrderBy(spot => spot.Order).Select(spot => spot.ToDto()).ToArray());
    }

    public static TourCategoryDto ToDto(this TourCategory category, string language = "en")
    {
        var translation = category.Translations.FindBestTranslation(language);

        return new TourCategoryDto(
            category.Id,
            translation?.Name ?? string.Empty,
            translation?.Description,
            translation?.Slug,
            category.IconClass,
            category.IsActive,
            category.SortOrder);
    }

    public static TourImageDto ToDto(this TourImage image)
    {
        return new TourImageDto(
            image.Id,
            image.ImagePath,
            image.ThumbnailPath,
            image.MediumPath,
            image.AltText,
            image.Caption,
            image.SortOrder,
            image.IsMainImage);
    }

    public static TourItineraryDto ToDto(this TourItinerary itinerary, string language = "en")
    {
        var translation = itinerary.Translations.FindBestTranslation(language);

        return new TourItineraryDto(
            itinerary.Id,
            itinerary.Day,
            itinerary.SortOrder,
            translation?.Title ?? string.Empty,
            translation?.Description ?? string.Empty,
            translation?.Location,
            translation?.Accommodation,
            translation?.Meals);
    }

    public static TourSpotDto ToDto(this TourSpot spot)
    {
        return new TourSpotDto(
            spot.Id,
            spot.Latitude,
            spot.Longitude,
            spot.Order,
            spot.Name,
            spot.Description);
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
            TourTranslation tourTranslation => tourTranslation.Language,
            TourCategoryTranslation categoryTranslation => categoryTranslation.Language,
            TourItineraryTranslation itineraryTranslation => itineraryTranslation.Language,
            _ => string.Empty
        };
    }
}

public sealed class TourQueryValidator : PagedQueryValidator<TourQuery>
{
    public TourQueryValidator()
    {
        RuleFor(query => query.Language).NotEmpty().MaximumLength(10);
    }
}

public sealed class TourCategoryQueryValidator : PagedQueryValidator<TourCategoryQuery>
{
    public TourCategoryQueryValidator()
    {
        RuleFor(query => query.Language).NotEmpty().MaximumLength(10);
    }
}

public sealed class TourTranslationRequestValidator : AbstractValidator<TourTranslationRequest>
{
    public TourTranslationRequestValidator()
    {
        RuleFor(request => request.Language).NotEmpty().MaximumLength(10);
        RuleFor(request => request.Title).NotEmpty().MaximumLength(200);
        RuleFor(request => request.ShortDescription).NotEmpty().MaximumLength(500);
        RuleFor(request => request.Description).NotEmpty();
        RuleFor(request => request.Slug).MaximumLength(200);
        RuleFor(request => request.MetaDescription).MaximumLength(200);
        RuleFor(request => request.MetaKeywords).MaximumLength(200);
        RuleFor(request => request.DurationUnit).MaximumLength(100);
    }
}

public sealed class CreateTourRequestValidator : AbstractValidator<CreateTourRequest>
{
    public CreateTourRequestValidator()
    {
        RuleFor(request => request.Price).GreaterThanOrEqualTo(0).When(request => request.Price.HasValue);
        RuleFor(request => request.Duration).GreaterThanOrEqualTo(1);
        RuleFor(request => request.DurationText).MaximumLength(100);
        RuleFor(request => request.CategoryId).GreaterThan(0);
        RuleFor(request => request.FeaturedImagePath).MaximumLength(500);
        RuleFor(request => request.Translations).NotEmpty();
        RuleForEach(request => request.Translations).SetValidator(new TourTranslationRequestValidator());
    }
}

public sealed class UpdateTourRequestValidator : AbstractValidator<UpdateTourRequest>
{
    public UpdateTourRequestValidator()
    {
        RuleFor(request => request.Id).GreaterThan(0);
        RuleFor(request => request.Price).GreaterThanOrEqualTo(0).When(request => request.Price.HasValue);
        RuleFor(request => request.Duration).GreaterThanOrEqualTo(1);
        RuleFor(request => request.DurationText).MaximumLength(100);
        RuleFor(request => request.CategoryId).GreaterThan(0);
        RuleFor(request => request.FeaturedImagePath).MaximumLength(500);
        RuleFor(request => request.Translations).NotEmpty();
        RuleForEach(request => request.Translations).SetValidator(new TourTranslationRequestValidator());
    }
}

public sealed class TourCategoryTranslationRequestValidator : AbstractValidator<TourCategoryTranslationRequest>
{
    public TourCategoryTranslationRequestValidator()
    {
        RuleFor(request => request.Language).NotEmpty().MaximumLength(10);
        RuleFor(request => request.Name).NotEmpty().MaximumLength(100);
        RuleFor(request => request.Description).MaximumLength(500);
        RuleFor(request => request.Slug).MaximumLength(100);
    }
}

public sealed class CreateTourCategoryRequestValidator : AbstractValidator<CreateTourCategoryRequest>
{
    public CreateTourCategoryRequestValidator()
    {
        RuleFor(request => request.IconClass).MaximumLength(200);
        RuleFor(request => request.Translations).NotEmpty();
        RuleForEach(request => request.Translations).SetValidator(new TourCategoryTranslationRequestValidator());
    }
}

public sealed class UpdateTourCategoryRequestValidator : AbstractValidator<UpdateTourCategoryRequest>
{
    public UpdateTourCategoryRequestValidator()
    {
        RuleFor(request => request.Id).GreaterThan(0);
        RuleFor(request => request.IconClass).MaximumLength(200);
        RuleFor(request => request.Translations).NotEmpty();
        RuleForEach(request => request.Translations).SetValidator(new TourCategoryTranslationRequestValidator());
    }
}