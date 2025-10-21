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
    private readonly ILabCommentService _commentService;

    public LabsController(ILabService labService, ILabCommentService commentService)
    {
        _labService = labService;
        _commentService = commentService;
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

    // Track a view hit (should be called when reading a cyber lab)
    [HttpPost("{id}/view")]
    [Authorize]
    public async Task<IActionResult> TrackView([FromRoute] int id)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var role = User.FindFirst("role")?.Value;
        int? userId = int.TryParse(userIdStr, out var uid) ? uid : null;

        var ip = HttpContext.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (string.IsNullOrEmpty(ip))
            ip = HttpContext.Connection.RemoteIpAddress?.ToString();
        else if (ip.Contains(','))
            ip = ip.Split(',')[0].Trim();

        await _labService.TrackViewAsync(id, userId, ip);
        return Ok();
    }

    // Rate a cyber lab (only role User should call this at policy)
    [HttpPost("{id}/rate")]
    [Authorize(Roles = "user")]
    public async Task<IActionResult> Rate([FromRoute] int id, [FromBody] RateLabRequest req)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
            return Unauthorized();
        var result = await _labService.RateAsync(id, userId, req);
        return Ok(result);
    }

    // Comments
    [HttpPost("{id}/comments")]
    [Authorize]
    public async Task<IActionResult> AddComment(
        [FromRoute] int id,
        [FromBody] string content,
        [FromQuery] int? parentId = null
    )
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
            return Unauthorized();
        await _commentService.AddCommentAsync(id, userId, content, parentId);
        return Ok();
    }

    [HttpGet("{id}/comments")]
    public async Task<IActionResult> GetComments(
        [FromRoute] int id,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20
    )
    {
        var items = await _commentService.GetCommentsAsync(id, page, pageSize);
        return Ok(items);
    }

    //[HttpGet("{id}/comments/tree")]
    //public async Task<IActionResult> GetCommentTree(
    //    [FromRoute] int id,
    //    [FromQuery] int page = 1,
    //    [FromQuery] int pageSize = 20
    //)
    //{
    //    var items = await _commentService.GetCommentTreeAsync(id, page, pageSize);
    //    return Ok(items);
    //}

    [HttpPatch("comments/{commentId}")]
    [Authorize]
    public async Task<IActionResult> EditComment(
        [FromRoute] int commentId,
        [FromBody] EditCommentRequest req
    )
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
            return Unauthorized();
        await _commentService.EditCommentAsync(commentId, userId, req.Content);
        return NoContent();
    }

    [HttpDelete("comments/{commentId}")]
    [Authorize]
    public async Task<IActionResult> DeleteComment([FromRoute] int commentId)
    {
        var userIdStr = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(userIdStr) || !int.TryParse(userIdStr, out var userId))
            return Unauthorized();
        await _commentService.DeleteCommentAsync(commentId, userId);
        return NoContent();
    }
}
