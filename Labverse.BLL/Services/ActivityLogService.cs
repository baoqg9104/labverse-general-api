using Labverse.BLL.Interfaces;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.UnitOfWork;
using System.Text.Json;

namespace Labverse.BLL.Services;

public class ActivityLogService : IActivityLogService
{
    private readonly IUnitOfWork _uow;

    public ActivityLogService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task LogAsync(
        int userId,
        string action,
        int? labId = null,
        int? questionId = null,
        object? metadata = null,
        string? description = null
    )
    {
        try
        {
            var entry = new ActivityHistory
            {
                UserId = userId,
                LabId = labId,
                QuestionId = questionId,
                Action = action,
                Description = description,
                MetadataJson = metadata == null ? null : JsonSerializer.Serialize(metadata),
            };
            await _uow.ActivityHistories.AddAsync(entry);
            await _uow.SaveChangesAsync();
        }
        catch
        {
            // Swallow logging errors to not affect main flow
        }
    }
}
