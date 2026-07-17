using FluentValidation;
using TravelToursWebsite.Application.Common;

namespace TravelToursWebsite.Application.Features.Media;

public sealed class MediaOptions
{
    public const string SectionName = "Media";

    public long MaxFileSizeBytes { get; set; } = 10 * 1024 * 1024;
    public int MaxWidth { get; set; } = 1920;
    public int MaxHeight { get; set; } = 1080;
    public int WebPQuality { get; set; } = 82;
    public int ThumbnailWidth { get; set; } = 300;
    public int ThumbnailHeight { get; set; } = 200;
    public int MediumWidth { get; set; } = 800;
    public int MediumHeight { get; set; } = 600;
    public string UploadRootFolder { get; set; } = "uploads";
    public string PublicBasePath { get; set; } = "/uploads";
    public string? PublicBaseUrl { get; set; }
    public string[] AllowedExtensions { get; set; } = [".jpg", ".jpeg", ".png", ".webp"];
    public string[] AllowedContentTypes { get; set; } = ["image/jpeg", "image/png", "image/webp"];
}

public sealed record MediaUploadRequest(
    Stream Content,
    string FileName,
    string? ContentType,
    long Length,
    string FolderName,
    string? PublicBaseUrl = null,
    string? AltText = null,
    string? Caption = null);

public sealed record MediaAssetDto(
    string FileName,
    string ContentType,
    long SizeBytes,
    int Width,
    int Height,
    string ImageUrl,
    string ImageLocalPath,
    string? ThumbnailUrl,
    string? ThumbnailLocalPath,
    string? MediumUrl,
    string? MediumLocalPath,
    string? AltText,
    string? Caption);

public interface IMediaStorageService
{
    Task<OperationResult<MediaAssetDto>> SaveImageAsync(MediaUploadRequest request, CancellationToken cancellationToken = default);
    Task<OperationResult> DeleteImageAsync(string imageLocalPath, CancellationToken cancellationToken = default);
    string GetPublicUrl(string imageLocalPath);
}

public sealed class MediaUploadRequestValidator : AbstractValidator<MediaUploadRequest>
{
    public MediaUploadRequestValidator()
    {
        RuleFor(request => request.Content).NotNull();
        RuleFor(request => request.FileName).NotEmpty().MaximumLength(260);
        RuleFor(request => request.Length).GreaterThan(0);
        RuleFor(request => request.FolderName)
            .NotEmpty()
            .MaximumLength(80)
            .Matches("^[a-zA-Z0-9_-]+$");
        RuleFor(request => request.AltText).MaximumLength(200);
        RuleFor(request => request.Caption).MaximumLength(200);
    }
}


