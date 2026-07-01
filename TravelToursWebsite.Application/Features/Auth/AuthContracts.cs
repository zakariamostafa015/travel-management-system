using FluentValidation;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Domain.Enums;

namespace TravelToursWebsite.Application.Features.Auth;

public sealed class JwtOptions
{
    public const string SectionName = "Jwt";

    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string Secret { get; set; } = string.Empty;
    public int AccessTokenMinutes { get; set; } = 30;
    public int RefreshTokenDays { get; set; } = 7;
}

public sealed record LoginRequest(string Username, string Password);
public sealed record RefreshTokenRequest(string RefreshToken);
public sealed record RevokeRefreshTokenRequest(string RefreshToken);

public sealed record AuthenticatedUserDto(
    int Id,
    string Username,
    string Email,
    string? FirstName,
    string? LastName,
    UserRole Role);

public sealed record AuthTokenResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAtUtc,
    string RefreshToken,
    DateTime RefreshTokenExpiresAtUtc,
    AuthenticatedUserDto User);

public interface IAuthService
{
    Task<OperationResult<AuthTokenResponse>> LoginAsync(LoginRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<OperationResult<AuthTokenResponse>> RefreshTokenAsync(RefreshTokenRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<OperationResult> RevokeRefreshTokenAsync(RevokeRefreshTokenRequest request, string? ipAddress = null, CancellationToken cancellationToken = default);
    Task<AuthenticatedUserDto?> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default);
}

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string passwordHash);
}

public interface IJwtTokenService
{
    (string Token, DateTime ExpiresAtUtc) CreateAccessToken(AuthenticatedUserDto user);
    (string Token, string TokenHash, DateTime ExpiresAtUtc) CreateRefreshToken(string? ipAddress = null);
    string HashRefreshToken(string refreshToken);
}

public sealed class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(request => request.Username).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Password).NotEmpty().MaximumLength(128);
    }
}

public sealed class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(request => request.RefreshToken).NotEmpty();
    }
}

public sealed class RevokeRefreshTokenRequestValidator : AbstractValidator<RevokeRefreshTokenRequest>
{
    public RevokeRefreshTokenRequestValidator()
    {
        RuleFor(request => request.RefreshToken).NotEmpty();
    }
}