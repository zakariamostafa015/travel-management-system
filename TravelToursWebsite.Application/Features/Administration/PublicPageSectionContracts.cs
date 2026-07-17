using FluentValidation;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Application.Features.PublicContent;
using TravelToursWebsite.Domain.Entities;

namespace TravelToursWebsite.Application.Features.Administration;

public sealed record PublicPageSectionQuery : PagedQuery
{
    public string? PageKey { get; init; }
    public bool? IsActive { get; init; }
}

public sealed record PublicPageSectionTranslationRequest(
    string Language,
    string? Eyebrow,
    string Title,
    string? Subtitle,
    string? Body,
    string? SupportingCopy);

public sealed record PublicPageSectionItemRequest(
    int? Id,
    string? ItemKey,
    string? Label,
    string? Value,
    string? Description,
    string? Url,
    string? IconClass,
    int SortOrder,
    bool IsActive);

public sealed record UpsertPublicPageSectionRequest(
    int? Id,
    string PageKey,
    string SectionKey,
    string LayoutVariant,
    string? Theme,
    string? DesktopMediaUrl,
    string? MobileMediaUrl,
    string? MediaAlt,
    string? CtaLabel,
    string? CtaUrl,
    int? LinkedTourId,
    int? LinkedTourCategoryId,
    int? LinkedBlogPostId,
    int SortOrder,
    bool IsActive,
    IReadOnlyList<PublicPageSectionTranslationRequest> Translations,
    IReadOnlyList<PublicPageSectionItemRequest> Items);

public sealed record AdminPublicPageSectionDto(
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
    DateTime CreatedDate,
    DateTime? UpdatedDate,
    IReadOnlyList<PublicPageSectionTranslationRequest> Translations,
    IReadOnlyList<PublicPageSectionItemDto> Items);

public interface IPublicPageSectionManagementService
{
    Task<PagedResult<AdminPublicPageSectionDto>> GetSectionsAsync(PublicPageSectionQuery query, CancellationToken cancellationToken = default);
    Task<AdminPublicPageSectionDto?> GetSectionByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<OperationResult<AdminPublicPageSectionDto>> UpsertSectionAsync(UpsertPublicPageSectionRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteSectionAsync(int id, CancellationToken cancellationToken = default);
}

public static class PublicPageSectionAdministrationMappingExtensions
{
    public static AdminPublicPageSectionDto ToAdminDto(this PublicPageSection section)
    {
        return new AdminPublicPageSectionDto(
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
            section.CreatedDate,
            section.UpdatedDate,
            section.Translations
                .OrderBy(item => item.Language)
                .Select(item => new PublicPageSectionTranslationRequest(
                    item.Language,
                    item.Eyebrow,
                    item.Title,
                    item.Subtitle,
                    item.Body,
                    item.SupportingCopy))
                .ToArray(),
            section.Items
                .OrderBy(item => item.SortOrder)
                .ThenBy(item => item.Id)
                .Select(item => item.ToPublicDto())
                .ToArray());
    }
}

public sealed class PublicPageSectionQueryValidator : PagedQueryValidator<PublicPageSectionQuery>
{
    public PublicPageSectionQueryValidator()
    {
        RuleFor(query => query.PageKey).MaximumLength(100);
    }
}

public sealed class PublicPageSectionTranslationRequestValidator : AbstractValidator<PublicPageSectionTranslationRequest>
{
    public PublicPageSectionTranslationRequestValidator()
    {
        RuleFor(request => request.Language).NotEmpty().MaximumLength(10);
        RuleFor(request => request.Title).NotEmpty().MaximumLength(240);
        RuleFor(request => request.Eyebrow).MaximumLength(160);
        RuleFor(request => request.Subtitle).MaximumLength(300);
    }
}

public sealed class PublicPageSectionItemRequestValidator : AbstractValidator<PublicPageSectionItemRequest>
{
    public PublicPageSectionItemRequestValidator()
    {
        RuleFor(request => request.Id).GreaterThan(0).When(request => request.Id.HasValue);
        RuleFor(request => request.ItemKey).MaximumLength(100);
        RuleFor(request => request.Label).MaximumLength(160);
        RuleFor(request => request.Value).MaximumLength(160);
        RuleFor(request => request.Description).MaximumLength(500);
        RuleFor(request => request.Url).MaximumLength(500);
        RuleFor(request => request.IconClass).MaximumLength(80);
    }
}

public sealed class UpsertPublicPageSectionRequestValidator : AbstractValidator<UpsertPublicPageSectionRequest>
{
    public UpsertPublicPageSectionRequestValidator()
    {
        RuleFor(request => request.Id).GreaterThan(0).When(request => request.Id.HasValue);
        RuleFor(request => request.PageKey).NotEmpty().MaximumLength(100);
        RuleFor(request => request.SectionKey).NotEmpty().MaximumLength(100);
        RuleFor(request => request.LayoutVariant).NotEmpty().MaximumLength(80);
        RuleFor(request => request.Theme).MaximumLength(80);
        RuleFor(request => request.DesktopMediaUrl).MaximumLength(500);
        RuleFor(request => request.MobileMediaUrl).MaximumLength(500);
        RuleFor(request => request.MediaAlt).MaximumLength(300);
        RuleFor(request => request.CtaLabel).MaximumLength(120);
        RuleFor(request => request.CtaUrl).MaximumLength(500);
        RuleFor(request => request.LinkedTourId).GreaterThan(0).When(request => request.LinkedTourId.HasValue);
        RuleFor(request => request.LinkedTourCategoryId).GreaterThan(0).When(request => request.LinkedTourCategoryId.HasValue);
        RuleFor(request => request.LinkedBlogPostId).GreaterThan(0).When(request => request.LinkedBlogPostId.HasValue);
        RuleFor(request => request.Translations).NotEmpty();
        RuleForEach(request => request.Translations).SetValidator(new PublicPageSectionTranslationRequestValidator());
        RuleForEach(request => request.Items).SetValidator(new PublicPageSectionItemRequestValidator());
    }
}
