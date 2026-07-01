using Microsoft.EntityFrameworkCore;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Application.Features.Auth;
using TravelToursWebsite.Domain.Entities;
using TravelToursWebsite.Infrastructure.Persistence;

namespace TravelToursWebsite.Infrastructure.Auth;

public sealed class AuthService(
    ApplicationDbContext context,
    IPasswordHasher passwordHasher,
    IJwtTokenService jwtTokenService)
    : IAuthService
{
    public async Task<OperationResult<AuthTokenResponse>> LoginAsync(
        LoginRequest request,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .FirstOrDefaultAsync(item => item.Username == request.Username && item.IsActive, cancellationToken);

        if (user is null || !passwordHasher.VerifyPassword(request.Password, user.PasswordHash))
        {
            return OperationResult<AuthTokenResponse>.Failure("Invalid username or password.");
        }

        user.LastLoginDate = DateTime.UtcNow;
        var response = CreateTokenResponse(user, ipAddress);
        await context.SaveChangesAsync(cancellationToken);

        return OperationResult<AuthTokenResponse>.Success(response);
    }

    public async Task<OperationResult<AuthTokenResponse>> RefreshTokenAsync(
        RefreshTokenRequest request,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = jwtTokenService.HashRefreshToken(request.RefreshToken);
        var existingToken = await context.RefreshTokens
            .Include(token => token.User)
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (existingToken is null || !existingToken.IsActive || existingToken.User is null || !existingToken.User.IsActive)
        {
            return OperationResult<AuthTokenResponse>.Failure("Invalid refresh token.");
        }

        existingToken.RevokedAtUtc = DateTime.UtcNow;
        existingToken.RevokedByIp = ipAddress;

        var response = CreateTokenResponse(existingToken.User, ipAddress);
        existingToken.ReplacedByTokenHash = jwtTokenService.HashRefreshToken(response.RefreshToken);
        await context.SaveChangesAsync(cancellationToken);

        return OperationResult<AuthTokenResponse>.Success(response);
    }

    public async Task<OperationResult> RevokeRefreshTokenAsync(
        RevokeRefreshTokenRequest request,
        string? ipAddress = null,
        CancellationToken cancellationToken = default)
    {
        var tokenHash = jwtTokenService.HashRefreshToken(request.RefreshToken);
        var existingToken = await context.RefreshTokens
            .FirstOrDefaultAsync(token => token.TokenHash == tokenHash, cancellationToken);

        if (existingToken is null)
        {
            return OperationResult.Failure("Refresh token was not found.");
        }

        if (!existingToken.IsRevoked)
        {
            existingToken.RevokedAtUtc = DateTime.UtcNow;
            existingToken.RevokedByIp = ipAddress;
            await context.SaveChangesAsync(cancellationToken);
        }

        return OperationResult.Success("Refresh token revoked.");
    }

    public async Task<AuthenticatedUserDto?> GetCurrentUserAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await context.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(item => item.Id == userId && item.IsActive, cancellationToken);

        return user?.ToAuthenticatedUser();
    }

    private AuthTokenResponse CreateTokenResponse(User user, string? ipAddress)
    {
        var authenticatedUser = user.ToAuthenticatedUser();
        var accessToken = jwtTokenService.CreateAccessToken(authenticatedUser);
        var refreshToken = jwtTokenService.CreateRefreshToken(ipAddress);

        context.RefreshTokens.Add(new RefreshToken
        {
            UserId = user.Id,
            TokenHash = refreshToken.TokenHash,
            ExpiresAtUtc = refreshToken.ExpiresAtUtc,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedByIp = ipAddress
        });

        return new AuthTokenResponse(
            accessToken.Token,
            accessToken.ExpiresAtUtc,
            refreshToken.Token,
            refreshToken.ExpiresAtUtc,
            authenticatedUser);
    }
}

internal static class AuthUserMappingExtensions
{
    public static AuthenticatedUserDto ToAuthenticatedUser(this User user)
    {
        return new AuthenticatedUserDto(
            user.Id,
            user.Username,
            user.Email,
            user.FirstName,
            user.LastName,
            user.Role);
    }
}