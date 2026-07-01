using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using TravelToursWebsite.Application.Features.Auth;

namespace TravelToursWebsite.Infrastructure.Auth;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    private readonly JwtOptions _options = options.Value;

    public (string Token, DateTime ExpiresAtUtc) CreateAccessToken(AuthenticatedUserDto user)
    {
        var expiresAtUtc = DateTime.UtcNow.AddMinutes(Math.Max(1, _options.AccessTokenMinutes));
        var signingCredentials = new SigningCredentials(CreateSecurityKey(), SecurityAlgorithms.HmacSha256);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role.ToString()),
            new Claim("role", user.Role.ToString()),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString("N"))
        };

        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAtUtc,
            signingCredentials: signingCredentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAtUtc);
    }

    public (string Token, string TokenHash, DateTime ExpiresAtUtc) CreateRefreshToken(string? ipAddress = null)
    {
        var randomBytes = RandomNumberGenerator.GetBytes(64);
        var token = Convert.ToBase64String(randomBytes);
        var expiresAtUtc = DateTime.UtcNow.AddDays(Math.Max(1, _options.RefreshTokenDays));
        return (token, HashRefreshToken(token), expiresAtUtc);
    }

    public string HashRefreshToken(string refreshToken)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(refreshToken));
        return Convert.ToBase64String(bytes);
    }

    private SymmetricSecurityKey CreateSecurityKey()
    {
        if (string.IsNullOrWhiteSpace(_options.Secret) || Encoding.UTF8.GetByteCount(_options.Secret) < 32)
        {
            throw new InvalidOperationException("JWT secret must be configured with at least 32 bytes.");
        }

        return new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_options.Secret));
    }
}