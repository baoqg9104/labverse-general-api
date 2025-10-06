using Labverse.API.Helpers;
using Labverse.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Labverse.API.Controllers;

[Route("api/user-progresses")]
[ApiController]
[Authorize]
public class UserProgresesController : ControllerBase
{
    private readonly IUserProgressService _userProgressService;

    public UserProgresesController(IUserProgressService userProgressService)
    {
        _userProgressService = userProgressService;
    }

    [HttpPost("start/{labId}")]
    public async Task<IActionResult> StartLab(int labId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return ApiErrorHelper.Error("UNAUTHORIZED", "User not authenticated", 401);

        if (!int.TryParse(userId, out var userIdInt))
            return ApiErrorHelper.Error("UNAUTHORIZED", "Invalid user id", 401);

        await _userProgressService.MarkLabAsStarted(userIdInt, labId);
        return Ok();
    }

    [HttpPost("complete/{labId}")]
    public async Task<IActionResult> CompleteLab(int labId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return ApiErrorHelper.Error("UNAUTHORIZED", "User not authenticated", 401);

        if (!int.TryParse(userId, out var userIdInt))
            return ApiErrorHelper.Error("UNAUTHORIZED", "Invalid user id", 401);

        await _userProgressService.MarkLabAsCompleted(userIdInt, labId);
        return Ok();
    }

    [HttpGet("status/{labId}")]
    public async Task<IActionResult> GetLabStatus(int labId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return ApiErrorHelper.Error("UNAUTHORIZED", "User not authenticated", 401);

        if (!int.TryParse(userId, out var userIdInt))
            return ApiErrorHelper.Error("UNAUTHORIZED", "Invalid user id", 401);

        var status = await _userProgressService.GetLabProgressStatus(userIdInt, labId);
        return Ok(status);
    }

    [HttpGet("completed")]
    public async Task<IActionResult> GetCompletedLabs()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return ApiErrorHelper.Error("UNAUTHORIZED", "User not authenticated", 401);

        if (!int.TryParse(userId, out var userIdInt))
            return ApiErrorHelper.Error("UNAUTHORIZED", "Invalid user id", 401);

        var completedLabs = await _userProgressService.GetCompletedLabsByUser(userIdInt);
        return Ok(completedLabs);
    }

    [HttpGet("in-progress")]
    public async Task<IActionResult> GetInProgressLabs()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (userId == null)
            return ApiErrorHelper.Error("UNAUTHORIZED", "User not authenticated", 401);

        if (!int.TryParse(userId, out var userIdInt))
            return ApiErrorHelper.Error("UNAUTHORIZED", "Invalid user id", 401);

        var inProgressLabs = await _userProgressService.GetInProgressLabsByUser(userIdInt);
        return Ok(inProgressLabs);
    }
}
