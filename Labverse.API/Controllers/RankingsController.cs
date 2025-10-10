using Labverse.API.Helpers;
using Labverse.BLL.DTOs.Users;
using Labverse.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
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

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetRankings([FromQuery] string criteria = "points", [FromQuery] int take = 50)
    {
        try
        {
            var crit = criteria.ToLower() == "streak" ? RankingCriteria.Streak : RankingCriteria.Points;
            take = take <= 0 ? 50 : Math.Min(take, 100);
            var result = await _rankingService.GetTopAsync(crit, take);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("GET_RANKINGS_ERROR", ex.Message, 500);
        }
    }
}
