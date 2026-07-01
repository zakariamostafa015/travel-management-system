using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Webp;
using SixLabors.ImageSharp.Processing;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Application.Features.Media;

namespace TravelToursWebsite.Infrastructure.Media;

public sealed class WebPImageStorageService(
    IHostEnvironment environment,
    IOptions<MediaOptions> options)
    : IMediaStorageService
{
    private readonly MediaOptions _options = options.Value;

    public async Task<OperationResult<MediaAssetDto>> SaveImageAsync(
        MediaUploadRequest request,
        CancellationToken cancellationToken = default)
    {
        var extension = Path.GetExtension(request.FileName).ToLowerInvariant();
        if (!_options.AllowedExtensions.Contains(extension, StringComparer.OrdinalIgnoreCase))
        {
            return OperationResult<MediaAssetDto>.Failure("Only jpg, jpeg, png, and webp images are allowed.");
        }

        if (!string.IsNullOrWhiteSpace(request.ContentType)
            && !_options.AllowedContentTypes.Contains(request.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            return OperationResult<MediaAssetDto>.Failure("The uploaded file content type is not allowed.");
        }

        if (request.Length <= 0 || request.Length > _options.MaxFileSizeBytes)
        {
            return OperationResult<MediaAssetDto>.Failure($"Image size must be between 1 byte and {_options.MaxFileSizeBytes} bytes.");
        }

        if (!IsSafeFolderName(request.FolderName))
        {
            return OperationResult<MediaAssetDto>.Failure("Folder name may only contain letters, numbers, underscores, and hyphens.");
        }

        Image image;
        try
        {
            image = await Image.LoadAsync(request.Content, cancellationToken);
        }
        catch (UnknownImageFormatException)
        {
            return OperationResult<MediaAssetDto>.Failure("The uploaded file is not a supported image.");
        }
        catch (InvalidImageContentException)
        {
            return OperationResult<MediaAssetDto>.Failure("The uploaded image content is invalid.");
        }

        using (image)
        {
            image.Metadata.ExifProfile = null;
            ResizeToFit(image, _options.MaxWidth, _options.MaxHeight, ResizeMode.Max);

            var fileName = $"{Guid.NewGuid():N}.webp";
            var relativeDirectory = NormalizeRelativeDirectory(request.FolderName);
            var uploadDirectory = EnsureUploadDirectory(relativeDirectory);
            var fullPath = Path.Combine(uploadDirectory, fileName);
            var localPath = ToRelativeLocalPath(relativeDirectory, fileName);

            var encoder = new WebpEncoder { Quality = _options.WebPQuality };
            await image.SaveAsWebpAsync(fullPath, encoder, cancellationToken);

            var thumbnail = await SaveVariantAsync(image, uploadDirectory, relativeDirectory, $"thumb_{fileName}", _options.ThumbnailWidth, _options.ThumbnailHeight, ResizeMode.Crop, encoder, cancellationToken);
            var medium = await SaveVariantAsync(image, uploadDirectory, relativeDirectory, $"medium_{fileName}", _options.MediumWidth, _options.MediumHeight, ResizeMode.Max, encoder, cancellationToken);
            var fileInfo = new FileInfo(fullPath);

            var result = new MediaAssetDto(
                fileName,
                "image/webp",
                fileInfo.Length,
                image.Width,
                image.Height,
                GetPublicUrl(localPath),
                localPath,
                GetPublicUrl(thumbnail.LocalPath),
                thumbnail.LocalPath,
                GetPublicUrl(medium.LocalPath),
                medium.LocalPath,
                request.AltText,
                request.Caption);

            return OperationResult<MediaAssetDto>.Success(result);
        }
    }

    public Task<OperationResult> DeleteImageAsync(string imageLocalPath, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(imageLocalPath))
        {
            return Task.FromResult(OperationResult.Failure("Image path is required."));
        }

        var root = GetUploadsRoot();
        var normalized = imageLocalPath.Replace('/', Path.DirectorySeparatorChar).TrimStart(Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(GetWebRootPath(), normalized));

        if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            return Task.FromResult(OperationResult.Failure("Image path is outside the uploads folder."));
        }

        DeleteIfExists(fullPath);
        var directory = Path.GetDirectoryName(fullPath);
        var fileName = Path.GetFileName(fullPath);
        if (!string.IsNullOrWhiteSpace(directory) && !string.IsNullOrWhiteSpace(fileName))
        {
            DeleteIfExists(Path.Combine(directory, $"thumb_{fileName}"));
            DeleteIfExists(Path.Combine(directory, $"medium_{fileName}"));
        }

        return Task.FromResult(OperationResult.Success("Image deleted."));
    }

    public string GetPublicUrl(string imageLocalPath)
    {
        if (string.IsNullOrWhiteSpace(imageLocalPath))
        {
            return string.Empty;
        }

        var uploadsRoot = _options.UploadRootFolder.Trim('/').Trim('\\');
        var normalized = imageLocalPath.Replace('\\', '/').TrimStart('/');
        if (normalized.StartsWith($"{uploadsRoot}/", StringComparison.OrdinalIgnoreCase))
        {
            normalized = normalized[(uploadsRoot.Length + 1)..];
        }

        return $"{_options.PublicBasePath.TrimEnd('/')}/{normalized}";
    }

    private async Task<(string LocalPath, string FullPath)> SaveVariantAsync(
        Image source,
        string uploadDirectory,
        string relativeDirectory,
        string fileName,
        int width,
        int height,
        ResizeMode mode,
        WebpEncoder encoder,
        CancellationToken cancellationToken)
    {
        using var clone = source.Clone(context => context.Resize(new ResizeOptions
        {
            Size = new Size(width, height),
            Mode = mode,
            Sampler = KnownResamplers.Lanczos3
        }));

        var fullPath = Path.Combine(uploadDirectory, fileName);
        await clone.SaveAsWebpAsync(fullPath, encoder, cancellationToken);
        return (ToRelativeLocalPath(relativeDirectory, fileName), fullPath);
    }

    private static void ResizeToFit(Image image, int maxWidth, int maxHeight, ResizeMode mode)
    {
        if (image.Width <= maxWidth && image.Height <= maxHeight)
        {
            return;
        }

        image.Mutate(context => context.Resize(new ResizeOptions
        {
            Size = new Size(maxWidth, maxHeight),
            Mode = mode,
            Sampler = KnownResamplers.Lanczos3
        }));
    }

    private string EnsureUploadDirectory(string relativeDirectory)
    {
        var root = GetUploadsRoot();
        var uploadDirectory = Path.GetFullPath(Path.Combine(root, relativeDirectory));
        if (!uploadDirectory.StartsWith(root, StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException("Upload directory is outside the configured uploads root.");
        }

        Directory.CreateDirectory(uploadDirectory);
        return uploadDirectory;
    }

    private string GetUploadsRoot()
    {
        var root = Path.GetFullPath(Path.Combine(GetWebRootPath(), _options.UploadRootFolder.Trim('/').Trim('\\')));
        Directory.CreateDirectory(root);
        return root;
    }

    private string GetWebRootPath()
    {
        return Path.Combine(environment.ContentRootPath, "wwwroot");
    }

    private static bool IsSafeFolderName(string folderName)
    {
        return !string.IsNullOrWhiteSpace(folderName)
            && folderName.All(character => char.IsLetterOrDigit(character) || character is '_' or '-');
    }

    private string NormalizeRelativeDirectory(string folderName)
    {
        return folderName.Trim().Trim('/', '\\');
    }

    private string ToRelativeLocalPath(string relativeDirectory, string fileName)
    {
        return Path.Combine(_options.UploadRootFolder.Trim('/').Trim('\\'), relativeDirectory, fileName).Replace('\\', '/');
    }

    private static void DeleteIfExists(string path)
    {
        if (File.Exists(path))
        {
            File.Delete(path);
        }
    }
}