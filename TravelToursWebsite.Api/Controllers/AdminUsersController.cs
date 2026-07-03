using Asp.Versioning;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TravelToursWebsite.Api.Common;
using TravelToursWebsite.Application.Common;
using TravelToursWebsite.Application.Features.Administration;

namespace TravelToursWebsite.Api.Controllers;

[ApiController]
[ApiVersion(1.0)]
[Authorize(Policy = "AdminOnly")]
[Route("api/v{version:apiVersion}/admin/users")]
public sealed class AdminUsersController(IUserApplicationService userService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers([FromQuery] UserQuery query, CancellationToken cancellationToken)
    {
        var result = await userService.GetUsersAsync(query, cancellationToken);
        return Ok(ApiResponse<PagedResult<UserDto>>.Ok(result, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetUserById(int id, CancellationToken cancellationToken)
    {
        var user = await userService.GetUserByIdAsync(id, cancellationToken);
        return user is null
            ? NotFound(ApiResponse<object>.Fail("User was not found.", traceId: HttpContext.TraceIdentifier))
            : Ok(ApiResponse<UserDto>.Ok(user, traceId: HttpContext.TraceIdentifier));
    }

    [HttpGet("by-username/{username}")]
    public async Task<IActionResult> GetUserByUsername(string username, CancellationToken cancellationToken)
    {
        var user = await userService.GetUserByUsernameAsync(username, cancellationToken);
        return user is null
            ? NotFound(ApiResponse<object>.Fail("User was not found.", traceId: HttpContext.TraceIdentifier))
            : Ok(ApiResponse<UserDto>.Ok(user, traceId: HttpContext.TraceIdentifier));
    }

    [HttpPost]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request, CancellationToken cancellationToken)
    {
        var result = await userService.CreateUserAsync(request, cancellationToken);
        return ToCreatedOrBadRequest(result);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> UpdateUser(int id, [FromBody] UpdateUserRequest request, CancellationToken cancellationToken)
    {
        if (id != request.Id)
        {
            return BadRequest(ApiResponse<object>.Fail("Route id must match request id.", traceId: HttpContext.TraceIdentifier));
        }

        return ToOkOrBadRequest(await userService.UpdateUserAsync(request, cancellationToken));
    }

    [HttpPatch("{id:int}/password")]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        if (id != request.UserId)
        {
            return BadRequest(ApiResponse<object>.Fail("Route id must match request id.", traceId: HttpContext.TraceIdentifier));
        }

        return ToOkOrBadRequest(await userService.ChangePasswordAsync(request, cancellationToken));
    }

    [HttpPatch("{id:int}/password/reset")]
    public async Task<IActionResult> ResetPassword(int id, [FromBody] ResetPasswordRequest request, CancellationToken cancellationToken)
    {
        if (id != request.UserId)
        {
            return BadRequest(ApiResponse<object>.Fail("Route id must match request id.", traceId: HttpContext.TraceIdentifier));
        }

        return ToOkOrBadRequest(await userService.ResetPasswordAsync(request, cancellationToken));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteUser(int id, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await userService.DeleteUserAsync(id, cancellationToken));
    }

    [HttpPatch("{id:int}/reactivate")]
    public async Task<IActionResult> ReactivateUser(int id, CancellationToken cancellationToken)
    {
        return ToOkOrBadRequest(await userService.ReactivateUserAsync(id, cancellationToken));
    }

    private IActionResult ToCreatedOrBadRequest<T>(OperationResult<T> result)
    {
        if (!result.Succeeded || result.Value is null)
        {
            return BadRequest(ApiResponse<object>.Fail(result.Message ?? "Request failed.", traceId: HttpContext.TraceIdentifier));
        }

        return StatusCode(StatusCodes.Status201Created, ApiResponse<T>.Ok(result.Value, result.Message, HttpContext.TraceIdentifier));
    }

    private IActionResult ToOkOrBadRequest<T>(OperationResult<T> result)
    {
        if (!result.Succeeded || result.Value is null)
        {
            return BadRequest(ApiResponse<object>.Fail(result.Message ?? "Request failed.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse<T>.Ok(result.Value, result.Message, HttpContext.TraceIdentifier));
    }

    private IActionResult ToOkOrBadRequest(OperationResult result)
    {
        if (!result.Succeeded)
        {
            return BadRequest(ApiResponse<object>.Fail(result.Message ?? "Request failed.", traceId: HttpContext.TraceIdentifier));
        }

        return Ok(ApiResponse.Ok(result.Message, HttpContext.TraceIdentifier));
    }
}
