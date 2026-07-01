namespace TravelToursWebsite.Application.Common;

public sealed record OperationResult(bool Succeeded, string? Message = null)
{
    public static OperationResult Success(string? message = null) => new(true, message);
    public static OperationResult Failure(string message) => new(false, message);
}

public sealed record OperationResult<T>(bool Succeeded, T? Value = default, string? Message = null)
{
    public static OperationResult<T> Success(T value, string? message = null) => new(true, value, message);
    public static OperationResult<T> Failure(string message) => new(false, default, message);
}