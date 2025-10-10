using Labverse.API.Helpers;
using Labverse.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Labverse.API.Controllers;

[Route("api/admin")]
[ApiController]
[Authorize(Roles = "admin")]
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
}
