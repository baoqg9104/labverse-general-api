using Labverse.BLL.DTOs.LabCategory;
using Labverse.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Labverse.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class LabCategoriesController : ControllerBase
{
    private readonly ILabCategoryService _labCategoryService;

    public LabCategoriesController(ILabCategoryService labCategoryService)
    {
        _labCategoryService = labCategoryService;
    }

    [HttpGet]
    public async Task<IActionResult> GetLabCategories()
    {
        var categories = await _labCategoryService.GetAllAsync();
        return Ok(categories);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetLabCategory(int id)
    {
        var category = await _labCategoryService.GetByIdAsync(id);
        if (category == null)
            return NotFound();
        return Ok(category);
    }

    [HttpPost]
    public async Task<IActionResult> CreateLabCategory([FromBody] CreateLabCategoryDto dto)
    {
        var category = await _labCategoryService.AddAsync(dto);
        return CreatedAtAction(nameof(GetLabCategory), new { id = category.Id }, category);
    }

    [HttpPatch("{id}")]
    public async Task<IActionResult> UpdateLabCategory(int id, [FromBody] UpdateLabCategoryDto dto)
    {
        await _labCategoryService.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteLabCategory(int id)
    {
        await _labCategoryService.DeleteAsync(id);
        return NoContent();
    }
}
