using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace TravelToursWebsite.Api.Common;

internal static class ValidationErrorFactory
{
    public static IReadOnlyDictionary<string, string[]> FromModelState(ModelStateDictionary modelState)
    {
        return modelState
            .Where(entry => entry.Value?.Errors.Count > 0)
            .ToDictionary(
                entry => entry.Key,
                entry => entry.Value!.Errors
                    .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage)
                        ? "The input was not valid."
                        : error.ErrorMessage)
                    .ToArray());
    }
}
