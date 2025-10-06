using Labverse.BLL.Interfaces;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Labverse.BLL.Services;

public class UserProgressService : IUserProgressService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserProgressService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<int>> GetCompletedLabsByUser(int userId)
    {
        var completedLabs = await _unitOfWork
            .UserProgresses.Query()
            .Where(up => up.UserId == userId && up.Status == ProgressStatus.Completed)
            .ToListAsync();

        return completedLabs.Select(lab => lab.LabId);
    }

    public async Task<IEnumerable<int>> GetInProgressLabsByUser(int userId)
    {
        var inProgressLabs = await _unitOfWork
            .UserProgresses.Query()
            .Where(up => up.UserId == userId && up.Status == ProgressStatus.InProgress)
            .ToListAsync();

        return inProgressLabs.Select(lab => lab.LabId);
    }

    public async Task<ProgressStatus> GetLabProgressStatus(int userId, int labId)
    {
        var progress = await _unitOfWork
            .UserProgresses.Query()
            .Where(up => up.UserId == userId && up.LabId == labId)
            .Select(up => up.Status)
            .FirstOrDefaultAsync();

        return progress;
    }

    public async Task MarkLabAsCompleted(int userId, int labId)
    {
        var progress = await _unitOfWork
            .UserProgresses.Query()
            .FirstOrDefaultAsync(up => up.UserId == userId && up.LabId == labId);

        if (progress == null)
        {
            progress = new UserProgress
            {
                UserId = userId,
                LabId = labId,
                Status = ProgressStatus.Completed,
                StartedAt = DateTime.UtcNow,
                CompletedAt = DateTime.UtcNow,
            };
            await _unitOfWork.UserProgresses.AddAsync(progress);
        }
        else
        {
            progress.Status = ProgressStatus.Completed;
            progress.CompletedAt = DateTime.UtcNow;
            _unitOfWork.UserProgresses.Update(progress);
        }

        await _unitOfWork.SaveChangesAsync();
    }

    public async Task MarkLabAsStarted(int userId, int labId)
    {
        var progress = await _unitOfWork
            .UserProgresses.Query()
            .FirstOrDefaultAsync(up => up.UserId == userId && up.LabId == labId);
        if (progress == null)
        {
            progress = new UserProgress
            {
                UserId = userId,
                LabId = labId,
                Status = ProgressStatus.InProgress,
                StartedAt = DateTime.UtcNow,
            };
            await _unitOfWork.UserProgresses.AddAsync(progress);
        }
        else if (progress.Status != ProgressStatus.Completed)
        {
            progress.Status = ProgressStatus.InProgress;
            if (progress.StartedAt == null)
            {
                progress.StartedAt = DateTime.UtcNow;
            }
            _unitOfWork.UserProgresses.Update(progress);
        }
        await _unitOfWork.SaveChangesAsync();
    }
}
