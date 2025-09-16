using Labverse.BLL.DTOs.Labs;

namespace Labverse.BLL.Interfaces;

public interface ILabService
{
    Task<LabDto?> GetByIdAsync(int id);
    Task<IEnumerable<LabDto>> GetAllAsync();
    Task<LabDto> AddAsync(int authorId, CreateLabDto dto);
    Task UpdateAsync(int id, UpdateLabDto dto);
    Task DeleteAsync(int id);
}
