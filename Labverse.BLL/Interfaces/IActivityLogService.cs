namespace Labverse.BLL.Interfaces;

public interface IActivityLogService
{
    Task LogAsync(
        int userId,
        string action,
        int? labId = null,
        int? questionId = null,
        object? metadata = null,
        string? description = null
    );
}
