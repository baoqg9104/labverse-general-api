using Labverse.BLL.DTOs.Labs;
using Labverse.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Labverse.API.Controllers;

[Route("api/labs")]
[ApiController]
[Authorize]
public class LabsController : ControllerBase
{
    private readonly ILabService _labService;

    public LabsController(ILabService labService)
    {
        _labService = labService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLabs()
    {
        var labs = await _labService.GetAllAsync();
        return Ok(labs);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLab(int id)
    {
        var lab = await _labService.GetByIdAsync(id);
        if (lab == null)
            return NotFound();
        return Ok(lab);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateLab(int id, [FromBody] UpdateLabDto dto)
    {
        await _labService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpPost]
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
    public async Task<IActionResult> DeleteLab(int id)
    {
        await _labService.DeleteAsync(id);
        return NoContent();
    }
}
