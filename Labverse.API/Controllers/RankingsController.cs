using Labverse.API.Helpers;
using Labverse.BLL.Interfaces;
using Labverse.DAL.EntitiesModels;
using Microsoft.AspNetCore.Mvc;

namespace Labverse.API.Controllers;

[Route("api/rankings")]
[ApiController]
public class RankingsController : ControllerBase
{
    private readonly IRankingService _rankingService;

    public RankingsController(IRankingService rankingService)
    {
        _rankingService = rankingService;
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUserRankings(
        [FromQuery] string criteria = "points",
        [FromQuery] int take = 50
    )
    {
        try
        {
            RankingCriteria crit = criteria.ToLower() switch
            {
                "streak" => RankingCriteria.Streak,
                "badges" => RankingCriteria.Badges,
                _ => RankingCriteria.Points,
            };
            take = take <= 0 ? 50 : Math.Min(take, 100);
            var result = await _rankingService.GetTopByRoleAsync(crit, UserRole.User, take);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("GET_USER_RANKINGS_ERROR", ex.Message, 500);
        }
    }

    [HttpGet("authors")]
    public async Task<IActionResult> GetAuthorRankings(
        [FromQuery] string criteria = "points",
        [FromQuery] int take = 50
    )
    {
        try
        {
            RankingCriteria crit = criteria.ToLower() switch
            {
                "streak" => RankingCriteria.Streak,
                "badges" => RankingCriteria.Badges,
                _ => RankingCriteria.Points,
            };
            take = take <= 0 ? 50 : Math.Min(take, 100);
            var result = await _rankingService.GetTopByRoleAsync(crit, UserRole.Author, take);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("GET_AUTHOR_RANKINGS_ERROR", ex.Message, 500);
        }
    }
}
