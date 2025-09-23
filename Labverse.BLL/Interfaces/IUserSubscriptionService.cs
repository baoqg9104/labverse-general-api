using Labverse.BLL.DTOs.UserSubscriptions;

namespace Labverse.BLL.Interfaces;

public interface IUserSubscriptionService
{
    Task<UserSubscriptionResponse?> GetUserSubscriptionActiveAsync(int userId);
    Task ExtendUserSubscriptionAsync(int userId, int subscriptionId);
    Task CreateUserSubscriptionAsync(int userId, int subscriptionId);
}
