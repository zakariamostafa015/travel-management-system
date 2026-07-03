using System.Text.Json;
using Microsoft.Extensions.Hosting;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Application.Features.Administration;

namespace TravelToursWebsite.Infrastructure.Administration;

public sealed class ResourceContentService(IHostEnvironment environment) : IResourceContentService
{
    private readonly JsonSerializerOptions jsonOptions = new() { WriteIndented = true };
    private string ResourcesPath => Path.Combine(environment.ContentRootPath, "Resources");

    public async Task<IReadOnlyList<ResourceContentLanguageDto>> GetLanguagesAsync(CancellationToken cancellationToken = default)
    {
        if (!Directory.Exists(ResourcesPath))
        {
            return Array.Empty<ResourceContentLanguageDto>();
        }

        var languages = new List<ResourceContentLanguageDto>();
        foreach (var file in Directory.GetFiles(ResourcesPath, "*.json").OrderBy(Path.GetFileNameWithoutExtension))
        {
            cancellationToken.ThrowIfCancellationRequested();
            var cultureCode = Path.GetFileNameWithoutExtension(file);
            if (cultureCode.StartsWith("_", StringComparison.Ordinal))
            {
                continue;
            }

            languages.Add(new ResourceContentLanguageDto(cultureCode, await ValidateLanguageFileAsync(cultureCode, cancellationToken)));
        }

        return languages;
    }

    public async Task<IReadOnlyDictionary<string, string>> GetLanguageContentAsync(string cultureCode, CancellationToken cancellationToken = default)
    {
        var values = await ReadLanguageFileAsync(cultureCode, cancellationToken);
        return values
            .Where(item => !item.Key.StartsWith("_comment", StringComparison.OrdinalIgnoreCase))
            .OrderBy(item => item.Key)
            .ToDictionary(item => item.Key, item => item.Value);
    }

    public async Task<ResourceContentItemDto> GetContentItemAsync(string key, CancellationToken cancellationToken = default)
    {
        var translations = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var language in await GetLanguagesAsync(cancellationToken))
        {
            var content = await GetLanguageContentAsync(language.CultureCode, cancellationToken);
            translations[language.CultureCode] = content.TryGetValue(key, out var value) ? value : string.Empty;
        }

        return new ResourceContentItemDto(key, GetCategoryForKey(key), translations);
    }

    public async Task<OperationResult<ResourceContentItemDto>> UpsertContentItemAsync(UpsertResourceContentItemRequest request, CancellationToken cancellationToken = default)
    {
        Directory.CreateDirectory(ResourcesPath);
        var languages = await GetLanguagesAsync(cancellationToken);
        if (languages.Count == 0 && request.Translations.Count == 0)
        {
            return OperationResult<ResourceContentItemDto>.Failure("No resource languages were found.");
        }

        var targetLanguages = languages.Select(language => language.CultureCode)
            .Concat(request.Translations.Keys)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        foreach (var cultureCode in targetLanguages)
        {
            var values = await ReadLanguageFileAsync(cultureCode, cancellationToken);
            values[request.Key.Trim()] = request.Translations.TryGetValue(cultureCode, out var value) ? value : string.Empty;
            await WriteLanguageFileAsync(cultureCode, values, cancellationToken);
        }

        var item = await GetContentItemAsync(request.Key.Trim(), cancellationToken);
        return OperationResult<ResourceContentItemDto>.Success(item, "Resource content saved.");
    }

    public async Task<OperationResult> DeleteContentItemAsync(string key, CancellationToken cancellationToken = default)
    {
        var languages = await GetLanguagesAsync(cancellationToken);
        if (languages.Count == 0)
        {
            return OperationResult.Failure("No resource languages were found.");
        }

        foreach (var language in languages)
        {
            var values = await ReadLanguageFileAsync(language.CultureCode, cancellationToken);
            if (values.Remove(key))
            {
                await WriteLanguageFileAsync(language.CultureCode, values, cancellationToken);
            }
        }

        return OperationResult.Success("Resource content deleted.");
    }

    public async Task<bool> ValidateLanguageFileAsync(string cultureCode, CancellationToken cancellationToken = default)
    {
        try
        {
            _ = await ReadLanguageFileAsync(cultureCode, cancellationToken);
            return true;
        }
        catch (JsonException)
        {
            return false;
        }
        catch (IOException)
        {
            return false;
        }
    }

    private async Task<Dictionary<string, string>> ReadLanguageFileAsync(string cultureCode, CancellationToken cancellationToken)
    {
        var filePath = GetLanguageFilePath(cultureCode);
        if (!File.Exists(filePath))
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        }

        var json = await File.ReadAllTextAsync(filePath, cancellationToken);
        var values = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ?? new Dictionary<string, JsonElement>();
        return values
            .Where(item => item.Value.ValueKind == JsonValueKind.String)
            .ToDictionary(item => item.Key, item => item.Value.GetString() ?? string.Empty, StringComparer.OrdinalIgnoreCase);
    }

    private async Task WriteLanguageFileAsync(string cultureCode, Dictionary<string, string> values, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(ResourcesPath);
        var ordered = values.OrderBy(item => item.Key).ToDictionary(item => item.Key, item => item.Value);
        var json = JsonSerializer.Serialize(ordered, jsonOptions);
        await File.WriteAllTextAsync(GetLanguageFilePath(cultureCode), json, cancellationToken);
    }

    private string GetLanguageFilePath(string cultureCode)
    {
        var safeCultureCode = Path.GetFileNameWithoutExtension(cultureCode.Trim());
        return Path.Combine(ResourcesPath, $"{safeCultureCode}.json");
    }

    private static string GetCategoryForKey(string key)
    {
        return key switch
        {
            _ when key.StartsWith("Nav.", StringComparison.OrdinalIgnoreCase) => "Navigation",
            _ when key.StartsWith("Home.", StringComparison.OrdinalIgnoreCase) => "Home Page",
            _ when key.StartsWith("Tours.", StringComparison.OrdinalIgnoreCase) => "Tours Page",
            _ when key.StartsWith("Contact.", StringComparison.OrdinalIgnoreCase) => "Contact Page",
            _ when key.StartsWith("Blog.", StringComparison.OrdinalIgnoreCase) => "Blog Page",
            _ when key.StartsWith("About.", StringComparison.OrdinalIgnoreCase) => "About Page",
            _ when key.StartsWith("Footer.", StringComparison.OrdinalIgnoreCase) => "Footer",
            _ when key.StartsWith("Common.", StringComparison.OrdinalIgnoreCase) => "Common",
            _ when key.StartsWith("User.", StringComparison.OrdinalIgnoreCase) => "User Management",
            _ when key.StartsWith("Category.", StringComparison.OrdinalIgnoreCase) => "Category Management",
            _ when key.StartsWith("Itinerary.", StringComparison.OrdinalIgnoreCase) => "Itinerary Management",
            _ when key.StartsWith("Admin.", StringComparison.OrdinalIgnoreCase) => "Admin Interface",
            _ => "General"
        };
    }
}
