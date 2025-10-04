using Labverse.BLL.DTOs.LabCategory;
using Labverse.BLL.Interfaces;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Labverse.BLL.Services;

public class LabCategoryService : ILabCategoryService
{
    private readonly IUnitOfWork _unitOfWork;

    public LabCategoryService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<LabCategoryResponseDto> AddAsync(CreateLabCategoryDto dto)
    {
        var existingCategory = await _unitOfWork
            .LabCategories.Query()
            .FirstOrDefaultAsync(c => c.Name == dto.Name);

        if (existingCategory != null)
            throw new InvalidOperationException("Category with the same name already exists");

        var labCategory = new LabCategory { Name = dto.Name };

        await _unitOfWork.LabCategories.AddAsync(labCategory);
        await _unitOfWork.SaveChangesAsync();

        return MapToDto(labCategory);
    }

    public async Task DeleteAsync(int id)
    {
        var labCategory = await _unitOfWork.LabCategories.GetByIdAsync(id);
        if (labCategory == null)
            throw new KeyNotFoundException("Lab category not found");
        _unitOfWork.LabCategories.Remove(labCategory);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<LabCategoryResponseDto>> GetAllAsync()
    {
        var categories = await _unitOfWork.LabCategories.GetAllAsync();
        return categories.Select(MapToDto);
    }

    public async Task<LabCategoryResponseDto?> GetByIdAsync(int id)
    {
        var labCategory = await _unitOfWork.LabCategories.GetByIdAsync(id);
        return labCategory == null ? null : MapToDto(labCategory);
    }

    public async Task UpdateAsync(int id, UpdateLabCategoryDto dto)
    {
        var labCategory = await _unitOfWork.LabCategories.GetByIdAsync(id);
        if (labCategory == null)
            throw new KeyNotFoundException("Lab category not found");

        // Check for duplicate name
        var existingCategory = await _unitOfWork
            .LabCategories.Query()
            .FirstOrDefaultAsync(c => c.Name == dto.Name && c.Id != id);

        if (existingCategory != null)
            throw new InvalidOperationException("Category with the same name already exists");

        labCategory.Name = dto.Name;

        _unitOfWork.LabCategories.Update(labCategory);
        await _unitOfWork.SaveChangesAsync();
    }

    private static LabCategoryResponseDto MapToDto(LabCategory labCategory)
    {
        return new LabCategoryResponseDto
        {
            Id = labCategory.Id,
            Name = labCategory.Name,
            IsActive = labCategory.IsActive,
        };
    }
}
