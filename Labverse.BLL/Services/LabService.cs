using Labverse.BLL.DTOs.Labs;
using Labverse.BLL.Interfaces;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

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
        // Check for duplicate slug
        var existingLab = await _unitOfWork
            .Labs.Query()
            .FirstOrDefaultAsync(l => l.Slug == dto.Slug);

        if (existingLab != null)
            throw new InvalidOperationException("Slug already exists");

        var lab = await _unitOfWork.Labs.AddAsync(
            new Lab
            {
                Title = dto.Title,
                Slug = dto.Slug,
                MdPath = dto.MdPath,
                MdPublicUrl = dto.MdPublicUrl,
                Description = dto.Description,
                DifficultyLevel = dto.DifficultyLevel,
                AuthorId = authorId,
            }
        );

        await _unitOfWork.SaveChangesAsync();

        return MapToDto(lab);
    }

    public async Task DeleteAsync(int id)
    {
        var lab = await _unitOfWork.Labs.GetByIdAsync(id);
        if (lab == null)
            throw new KeyNotFoundException("Lab not found");
        _unitOfWork.Labs.Remove(lab);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<IEnumerable<LabDto>> GetAllAsync(bool includeInactive = false)
    {
        var query = _unitOfWork.Labs.Query();
        if (includeInactive)
        {
            query = _unitOfWork.Labs.Query().IgnoreQueryFilters();
        }
        var labs = await query.ToListAsync();
        return labs.Select(MapToDto);
    }

    public async Task<IEnumerable<LabDto>> GetPreviewLabsAsync(int count = 3)
    {
        var labs = await _unitOfWork
            .Labs.Query()
            .Where(l => l.DifficultyLevel == LabDifficulty.Basic)
            .OrderBy(l => l.CreatedAt)
            .Take(count)
            .ToListAsync();

        return labs.Select(MapToDto);
    }

    public async Task<LabDto?> GetByIdAsync(int id)
    {
        var lab = await _unitOfWork.Labs.GetByIdAsync(id);
        return lab == null ? null : MapToDto(lab);
    }

    public async Task<LabDto?> GetBySlugAsync(string slug)
    {
        var lab = await _unitOfWork.Labs.Query().FirstOrDefaultAsync(l => l.Slug == slug);

        return lab == null ? null : MapToDto(lab);
    }

    public async Task UpdateAsync(int id, UpdateLabDto dto)
    {
        var lab = await _unitOfWork.Labs.GetByIdAsync(id);
        if (lab == null)
            throw new KeyNotFoundException("Lab not found");

        // Check for duplicate slug (except for current lab)
        var duplicateLab = await _unitOfWork
            .Labs.Query()
            .FirstOrDefaultAsync(l => l.Slug == dto.Slug && l.Id != id);

        if (duplicateLab != null)
            throw new InvalidOperationException("Slug already exists");

        lab.Title = dto.Title;
        lab.Slug = dto.Slug;
        lab.MdPath = dto.MdPath;
        lab.MdPublicUrl = dto.MdPublicUrl;
        lab.Description = dto.Description;
        lab.DifficultyLevel = dto.DifficultyLevel;
        lab.UpdatedAt = DateTime.UtcNow;

        _unitOfWork.Labs.Update(lab);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task RestoreAsync(int id)
    {
        var lab = await _unitOfWork.Labs.Query().IgnoreQueryFilters().FirstOrDefaultAsync(l => l.Id == id);
        if (lab == null)
            throw new KeyNotFoundException("Lab not found");
        if (lab.IsActive)
            return;
        lab.IsActive = true;
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
            Slug = lab.Slug,
            MdPath = lab.MdPath,
            MdPublicUrl = lab.MdPublicUrl,
            Description = lab.Description,
            DifficultyLevel = lab.DifficultyLevel,
            AuthorId = lab.AuthorId,
            CreatedAt = lab.CreatedAt,
            UpdatedAt = lab.UpdatedAt,
            IsActive = lab.IsActive,
        };
    }
}
