using Labverse.BLL.DTOs.Labs;
using Labverse.BLL.Interfaces;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Labverse.BLL.Services;

public class LabService : ILabService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IActivityLogService _activity;

    public LabService(IUnitOfWork unitOfWork, IActivityLogService activity)
    {
        _unitOfWork = unitOfWork;
        _activity = activity;
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
        await _activity.LogAsync(
            authorId,
            "lab_created",
            lab.Id,
            null,
            new { title = dto.Title, slug = dto.Slug },
            description: $"Created cyber lab: {dto.Title} 🛡️"
        );

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
        var lab = await _unitOfWork
            .Labs.Query()
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(l => l.Id == id);
        if (lab == null)
            throw new KeyNotFoundException("Lab not found");
        if (lab.IsActive)
            return;
        lab.IsActive = true;
        lab.UpdatedAt = DateTime.UtcNow;
        _unitOfWork.Labs.Update(lab);
        await _unitOfWork.SaveChangesAsync();
    }

    // Track a view: increments total views; if role == User and not counted before for this user, increments unique user views
    public async Task TrackViewAsync(int labId, int? userId, string? ip)
    {
        var lab =
            await _unitOfWork.Labs.GetByIdAsync(labId)
            ?? throw new KeyNotFoundException("Lab not found");

        // Persist raw view
        await _unitOfWork.LabViews.AddAsync(
            new LabView
            {
                LabId = labId,
                UserId = userId,
                Ip = ip,
            }
        );

        // Update aggregates
        lab.Views += 1;

        if (userId.HasValue)
        {
            var role = (await _unitOfWork.Users.GetByIdAsync((int)userId)).Role;

            if (role == UserRole.User)
            {
                // Count unique user view only once
                var hasViewed = await _unitOfWork
                    .LabViews.Query()
                    .AnyAsync(v => v.LabId == labId && v.UserId == userId.Value);

                if (!hasViewed)
                {
                    lab.UniqueUserViews += 1;
                }
            }
        }

        _unitOfWork.Labs.Update(lab);
        await _unitOfWork.SaveChangesAsync();

        try
        {
            await _activity.LogAsync(
                userId ?? 0,
                "lab_view",
                labId,
                null,
                new { labId, ip },
                description: "Viewed cyber lab 👀"
            );
        }
        catch { }
    }

    // Rate a lab (1..5) and update aggregates; only role User should be allowed at controller level
    public async Task<RateLabResponse> RateAsync(int labId, int userId, RateLabRequest req)
    {
        if (req.Score < 1 || req.Score > 5)
            throw new ArgumentException("Score must be 1..5");
        var lab =
            await _unitOfWork.Labs.GetByIdAsync(labId)
            ?? throw new KeyNotFoundException("Lab not found");

        // Upsert user's rating
        var existing = await _unitOfWork
            .LabRatings.Query()
            .FirstOrDefaultAsync(r => r.LabId == labId && r.UserId == userId);
        if (existing == null)
        {
            existing = new LabRating
            {
                LabId = labId,
                UserId = userId,
                Score = req.Score,
                Comment = req.Comment,
            };
            await _unitOfWork.LabRatings.AddAsync(existing);
            lab.RatingCount += 1;
            lab.RatingAverage =
                ((lab.RatingAverage * (lab.RatingCount - 1)) + req.Score) / lab.RatingCount;
        }
        else
        {
            // adjust average
            var total = lab.RatingAverage * lab.RatingCount - existing.Score + req.Score;
            existing.Score = req.Score;
            existing.Comment = req.Comment;
            _unitOfWork.LabRatings.Update(existing);
            lab.RatingAverage = lab.RatingCount == 0 ? 0 : total / lab.RatingCount;
        }

        _unitOfWork.Labs.Update(lab);
        await _unitOfWork.SaveChangesAsync();

        try
        {
            await _activity.LogAsync(
                userId,
                "lab_rated",
                labId,
                null,
                new { labId, score = req.Score },
                description: "Rated cyber lab ⭐"
            );
        }
        catch { }

        return new RateLabResponse
        {
            RatingAverage = lab.RatingAverage,
            RatingCount = lab.RatingCount,
        };
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
            Views = lab.Views,
            UniqueUserViews = lab.UniqueUserViews,
            RatingAverage = lab.RatingAverage,
            RatingCount = lab.RatingCount,
        };
    }
}
