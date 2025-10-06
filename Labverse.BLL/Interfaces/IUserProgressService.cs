using Labverse.DAL.EntitiesModels;

namespace Labverse.BLL.Interfaces;

public interface IUserProgressService
{
    Task MarkLabAsStarted(int userId, int labId);
    Task MarkLabAsCompleted(int userId, int labId);
    Task<ProgressStatus> GetLabProgressStatus(int userId, int labId);
    Task<IEnumerable<int>> GetCompletedLabsByUser(int userId);
    Task<IEnumerable<int>> GetInProgressLabsByUser(int userId);
}
