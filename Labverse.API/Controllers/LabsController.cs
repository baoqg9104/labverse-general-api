using Labverse.BLL.DTOs.Labs;
using Labverse.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Labverse.API.Controllers;

[Route("api/labs")]
[ApiController]
public class LabsController : ControllerBase
{
    private readonly ILabService _labService;

    public LabsController(ILabService labService)
    {
        _labService = labService;
    }

    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetLabs([FromQuery] bool includeInactive = false)
    {
        var labs = await _labService.GetAllAsync(includeInactive);
        return Ok(labs);
    }

    [HttpGet("preview")]
    public async Task<IActionResult> GetPreviewLabs()
    {
        var labs = await _labService.GetPreviewLabsAsync();
        return Ok(labs);
    }

    [HttpGet("{id}")]
    [Authorize]
    public async Task<IActionResult> GetLab(int id)
    {
        var lab = await _labService.GetByIdAsync(id);
        if (lab == null)
            return NotFound();
        return Ok(lab);
    }

    [HttpPatch("{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateLab(int id, [FromBody] UpdateLabDto dto)
    {
        await _labService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateLab([FromBody] CreateLabDto dto)
    {
        var authorId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (authorId == null)
            return Unauthorized();

        if (!int.TryParse(authorId, out var authorIdInt))
            return Unauthorized();

        var lab = await _labService.AddAsync(authorIdInt, dto);
        return CreatedAtAction(nameof(GetLab), new { id = lab.Id }, lab);
    }

    [HttpDelete("{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteLab(int id)
    {
        await _labService.DeleteAsync(id);
        return NoContent();
    }

    [HttpPost("{id}/restore")]
    [Authorize]
    public async Task<IActionResult> RestoreLab(int id)
    {
        try
        {
            await _labService.RestoreAsync(id);
            return Ok(new { message = "Lab restored" });
        }
        catch (KeyNotFoundException)
        {
            return NotFound();
        }
        catch (Exception ex)
        {
            return Problem(ex.Message, statusCode: 500);
        }
    }

    [HttpGet("slug/{slug}")]
    [Authorize]
    public async Task<IActionResult> GetLabBySlug(string slug)
    {
        var lab = await _labService.GetBySlugAsync(slug);

        if (lab == null)
            return NotFound();

        return Ok(lab);
    }
}
