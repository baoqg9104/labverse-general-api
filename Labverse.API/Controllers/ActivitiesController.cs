using Labverse.API.Helpers;
using Labverse.BLL.DTOs.Activities;
using Labverse.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Labverse.API.Controllers;

[Route("api/activities")]
[ApiController]
[Authorize]
public class ActivitiesController : ControllerBase
{
    private readonly IActivityQueryService _activityQuery;

    public ActivitiesController(IActivityQueryService activityQuery)
    {
        _activityQuery = activityQuery;
    }

    // Admin: list activities with filters
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> List([FromQuery] ActivityListQuery query)
    {
        try
        {
            var page = await _activityQuery.ListAsync(query);
            return Ok(page);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("LIST_ACTIVITIES_ERROR", ex.Message, 500);
        }
    }

    // User: list my recent activities with pagination
    [HttpGet("me/recent")]
    public async Task<IActionResult> MyRecent(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return ApiErrorHelper.Error("UNAUTHORIZED", "User not authenticated", 401);
            var result = await _activityQuery.ListAsync(
                new ActivityListQuery
                {
                    UserId = userId,
                    Page = Math.Max(1, page),
                    PageSize = Math.Clamp(pageSize, 1, 200),
                    SortDir = "desc",
                }
            );
            return Ok(result);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("LIST_MY_ACTIVITIES_ERROR", ex.Message, 500);
        }
    }
}
