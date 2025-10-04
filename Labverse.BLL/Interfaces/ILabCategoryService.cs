using Labverse.BLL.DTOs.LabCategory;

namespace Labverse.BLL.Interfaces;

public interface ILabCategoryService
{
    Task<LabCategoryResponseDto> AddAsync(CreateLabCategoryDto dto);
    Task<IEnumerable<LabCategoryResponseDto>> GetAllAsync();
    Task<LabCategoryResponseDto?> GetByIdAsync(int id);
    Task UpdateAsync(int id, UpdateLabCategoryDto dto);
    Task DeleteAsync(int id);
}
