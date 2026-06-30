namespace TravelToursWebsite.Api.Common;

public sealed record ApiResponse<T>(
    bool Success,
    string? Message,
    T? Data,
    IReadOnlyDictionary<string, string[]>? Errors,
    string? TraceId)
{
    public static ApiResponse<T> Ok(T data, string? message = null, string? traceId = null)
    {
        return new ApiResponse<T>(true, message, data, null, traceId);
    }

    public static ApiResponse<T> Fail(
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null,
        string? traceId = null)
    {
        return new ApiResponse<T>(false, message, default, errors, traceId);
    }
}

public sealed record ApiResponse(
    bool Success,
    string? Message,
    IReadOnlyDictionary<string, string[]>? Errors,
    string? TraceId)
{
    public static ApiResponse Ok(string? message = null, string? traceId = null)
    {
        return new ApiResponse(true, message, null, traceId);
    }

    public static ApiResponse Fail(
        string message,
        IReadOnlyDictionary<string, string[]>? errors = null,
        string? traceId = null)
    {
        return new ApiResponse(false, message, errors, traceId);
    }
}
