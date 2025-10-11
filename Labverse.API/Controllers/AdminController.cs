using Labverse.API.Helpers;
using Labverse.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Labverse.API.Controllers;

[Route("api/admin")]
[ApiController]
//[Authorize(Roles = "admin")]
public class AdminController : ControllerBase
{
    private readonly IRevenueService _revenueService;

    public AdminController(IRevenueService revenueService)
    {
        _revenueService = revenueService;
    }

    [HttpGet("revenue")]
    public async Task<IActionResult> GetRevenue([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        try
        {
            var summary = await _revenueService.GetRevenueAsync(from, to);
            return Ok(summary);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("GET_REVENUE_ERROR", ex.Message, 500);
        }
    }

    [HttpGet("revenue/daily")]
    public async Task<IActionResult> GetRevenueDaily([FromQuery] DateTime? from, [FromQuery] DateTime? to)
    {
        try
        {
            var summary = await _revenueService.GetRevenueDailyAsync(from, to);
            return Ok(summary); // List<DailyRevenuePointDto>
        }
        catch (FormatException ex)
        {
            return ApiErrorHelper.Error("BAD_REQUEST", ex.Message, 400);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("GET_REVENUE_DAILY_ERROR", ex.Message, 500);
        }
    }
}
