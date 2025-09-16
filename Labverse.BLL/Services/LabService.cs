using Labverse.BLL.DTOs.Labs;
using Labverse.BLL.Interfaces;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.UnitOfWork;

namespace Labverse.BLL.Services;

public class LabService : ILabService
{
    private readonly IUnitOfWork _unitOfWork;

    public LabService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<LabDto> AddAsync(int authorId, CreateLabDto dto)
    {
        var lab = await _unitOfWork.Labs.AddAsync(new Lab
        {
            Title = dto.Title,
            Description = dto.Description,
            DifficultyLevel = dto.DifficultyLevel,
            AuthorId = authorId,
            CategoryId = dto.CategoryId
        });

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(lab);
    }

    public async Task DeleteAsync(int id)
    {
        var lab = await _unitOfWork.Labs.GetByIdAsync(id);
        if (lab == null) throw new KeyNotFoundException("Lab not found");
        _unitOfWork.Labs.Remove(lab);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<LabDto>> GetAllAsync()
    {
        var labs = await _unitOfWork.Labs.GetAllAsync();
        return labs.Select(MapToDto);
    }

    public async Task<LabDto?> GetByIdAsync(int id)
    {
        var lab = await _unitOfWork.Labs.GetByIdAsync(id);
        return lab == null ? null : MapToDto(lab);
    }

    public async Task UpdateAsync(int id, UpdateLabDto dto)
    {
        var lab = await _unitOfWork.Labs.GetByIdAsync(id);
        if (lab == null) throw new KeyNotFoundException("Lab not found");
        lab.Title = dto.Title;
        lab.Description = dto.Description;
        lab.DifficultyLevel = dto.DifficultyLevel;
        lab.CategoryId = dto.CategoryId;
        lab.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Labs.Update(lab);
        await _unitOfWork.SaveChangesAsync();
    }

    private static LabDto MapToDto(Lab lab)
    {
        return new LabDto
        {
            Id = lab.Id,
            Title = lab.Title,
            Description = lab.Description,
            DifficultyLevel = lab.DifficultyLevel,
            AuthorId = lab.AuthorId,
            CategoryId = lab.CategoryId,
            CreatedAt = lab.CreatedAt,
            UpdatedAt = lab.UpdatedAt,
            IsActive = lab.IsActive
        };
    }
}
