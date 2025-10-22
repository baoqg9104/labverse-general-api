using Labverse.BLL.Services;
using Labverse.DAL.EntitiesModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Labverse.API.Controllers
{
    [Route("api/badge")]
    [ApiController]
    public class BadgeController : ControllerBase
    {
        private readonly BadgeService _badgeService;

        public BadgeController(BadgeService badgeService)
        {
            _badgeService = badgeService;
        }

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var badges = await _badgeService.GetAllAsync();
            return Ok(badges);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var badge = await _badgeService.GetByIdAsync(id);
            if (badge == null) return NotFound();
            return Ok(badge);
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([FromBody] Badge badge)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var newBadge = await _badgeService.CreateAsync(badge);
            return CreatedAtAction(nameof(GetById), new { id = newBadge.Id }, newBadge);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Update(int id, [FromBody] Badge updated)
        {
            var result = await _badgeService.UpdateAsync(id, updated);
            if (result == null) return NotFound();
            return Ok(result);
        }

        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            var deleted = await _badgeService.DeleteAsync(id);
            if (!deleted) return NotFound();
            return NoContent();
        }
    }
}
