using Labverse.BLL.DTOs.Labs;

namespace Labverse.BLL.Interfaces;

public interface ILabService
{
    Task<LabDto?> GetByIdAsync(int id);
    Task<LabDto?> GetBySlugAsync(string slug);
    Task<IEnumerable<LabDto>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<LabDto>> GetPreviewLabsAsync(int count = 3);
    Task<LabDto> AddAsync(int authorId, CreateLabDto dto);
    Task UpdateAsync(int id, UpdateLabDto dto);
    Task DeleteAsync(int id);
    Task RestoreAsync(int id);
}
