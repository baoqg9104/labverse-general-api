using Labverse.API.Helpers;
using Labverse.BLL.DTOs.Reports;
using Labverse.BLL.Interfaces;
using Labverse.DAL.EntitiesModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Labverse.API.Controllers;

[Route("api/reports")]
[ApiController]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reportService;
    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    // USER/AUTHOR create
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateReportRequest req)
    {
        try
        {
            var role = User.FindFirst("role")?.Value ?? "user";
            if (role == "admin") return ApiErrorHelper.Error("FORBIDDEN", "Admin cannot create reports", 403);
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var email = User.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty;
            if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return ApiErrorHelper.Error("UNAUTHORIZED", "User not authenticated", 401);
            var ip = HttpContext.Connection.RemoteIpAddress?.ToString() ?? string.Empty;
            var created = await _reportService.CreateAsync(userId, email, req, ip);
            return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
        }
        catch (ArgumentException ex)
        {
            return ApiErrorHelper.Error("ValidationError", ex.Message, 400);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("CREATE_REPORT_ERROR", ex.Message, 500);
        }
    }

    // ADMIN list
    [HttpGet]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> List([FromQuery] ReportListQuery query)
    {
        try
        {
            var page = await _reportService.ListAsync(query);
            return Ok(page);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("LIST_REPORTS_ERROR", ex.Message, 500);
        }
    }

    // ADMIN get details
    [HttpGet("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> GetById([FromRoute] int id)
    {
        try
        {
            var r = await _reportService.GetByIdAsync(id);
            if (r == null) return NotFound();
            return Ok(r);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("GET_REPORT_ERROR", ex.Message, 500);
        }
    }

    // ADMIN update
    [HttpPatch("{id}")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> Update([FromRoute] int id, [FromBody] UpdateReportRequest req)
    {
        try
        {
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var adminId))
                return ApiErrorHelper.Error("UNAUTHORIZED", "User not authenticated", 401);
            var updated = await _reportService.UpdateAsync(id, adminId, req);
            return Ok(updated);
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (ArgumentException ex)
        {
            return ApiErrorHelper.Error("ValidationError", ex.Message, 400);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("UPDATE_REPORT_ERROR", ex.Message, 500);
        }
    }

    // ADMIN export CSV
    [HttpGet("export")]
    [Authorize(Roles = "admin")]
    public async Task<IActionResult> ExportCsv([FromQuery] ReportListQuery query)
    {
        try
        {
            var csv = await _reportService.ExportCsvAsync(query);
            return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", $"reports-{DateTime.UtcNow:yyyyMMddHHmmss}.csv");
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("EXPORT_REPORTS_ERROR", ex.Message, 500);
        }
    }

    // USER/AUTHOR list mine
    [HttpGet("mine")]
    public async Task<IActionResult> ListMine([FromQuery] int page = 1, [FromQuery] int pageSize = 20)
    {
        try
        {
            var role = User.FindFirst("role")?.Value ?? "user";
            if (role == "admin") return ApiErrorHelper.Error("FORBIDDEN", "Admin cannot use this endpoint", 403);
            var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
                return ApiErrorHelper.Error("UNAUTHORIZED", "User not authenticated", 401);
            var pageDto = await _reportService.ListMineAsync(userId, page, pageSize);
            return Ok(pageDto);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("LIST_MY_REPORTS_ERROR", ex.Message, 500);
        }
    }
}
