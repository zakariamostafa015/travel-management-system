using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using TravelToursWebsite.Api.Common;
using TravelToursWebsite.Application.Features.Auth;

namespace TravelToursWebsite.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Route("api/v{version:apiVersion}/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.LoginAsync(request, GetIpAddress(), cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return Unauthorized(ApiResponse<object>.Fail(
                result.Message ?? "Invalid username or password.",
                traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<AuthTokenResponse>.Ok(result.Value, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<AuthTokenResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.RefreshTokenAsync(request, GetIpAddress(), cancellationToken);
        if (!result.Succeeded || result.Value is null)
        {
            return Unauthorized(ApiResponse<object>.Fail(
                result.Message ?? "Invalid refresh token.",
                traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<AuthTokenResponse>.Ok(result.Value, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPost("revoke")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Revoke(
        [FromBody] RevokeRefreshTokenRequest request,
        CancellationToken cancellationToken)
    {
        var result = await authService.RevokeRefreshTokenAsync(request, GetIpAddress(), cancellationToken);
        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<object>.Fail(
                result.Message ?? "Refresh token could not be revoked.",
                traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse.Ok(result.Message, HttpContext.TraceIdentifier));
    }

    [HttpGet("me")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<AuthenticatedUserDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Me(CancellationToken cancellationToken)
    {
        var userIdValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!int.TryParse(userIdValue, out var userId))
        {
            return Unauthorized(ApiResponse<object>.Fail(
                "Authenticated user id is missing.",
                traceId: HttpContext.TraceIdentifier));
        }

        var user = await authService.GetCurrentUserAsync(userId, cancellationToken);
        if (user is null)
        {
            return Unauthorized(ApiResponse<object>.Fail(
                "Authenticated user was not found.",
                traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<AuthenticatedUserDto>.Ok(user, traceId: HttpContext.TraceIdentifier));
    }

    private string? GetIpAddress()
    {
        return HttpContext.Connection.RemoteIpAddress?.ToString();
    }
}