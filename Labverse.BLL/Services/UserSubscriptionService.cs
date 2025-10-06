using Labverse.BLL.DTOs.UserSubscriptions;
using Labverse.BLL.Interfaces;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Labverse.BLL.Services;

public class UserSubscriptionService : IUserSubscriptionService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserSubscriptionService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task CreateUserSubscriptionAsync(int userId, int subscriptionId)
    {
        var now = DateTime.UtcNow;
        var end = now.AddMonths(1);
        var userSub = new UserSubscription
        {
            UserId = userId,
            SubscriptionId = subscriptionId,
            StartDate = now,
            EndDate = end,
        };
        await _unitOfWork.UserSubscriptions.AddAsync(userSub);
        await _unitOfWork.SaveChangesAsync();
    }

    public Task ExtendUserSubscriptionAsync(int userId, int subscriptionId)
    {
        throw new NotImplementedException();
    }

    public async Task<UserSubscriptionResponse?> GetUserSubscriptionActiveAsync(int userId)
    {
        var now = DateTime.UtcNow;
        var userScriptionActive = await _unitOfWork
            .UserSubscriptions.Query()
            .FirstOrDefaultAsync(us => us.Id == userId && us.StartDate <= now && us.EndDate > now);

        return userScriptionActive == null ? null : MapToDto(userScriptionActive);
    }

    private static UserSubscriptionResponse MapToDto(UserSubscription userSubscription)
    {
        return new UserSubscriptionResponse
        {
            Id = userSubscription.Id,
            UserId = userSubscription.UserId,
            SubscriptionId = userSubscription.SubscriptionId,
            StartDate = userSubscription.StartDate,
            EndDate = userSubscription.EndDate,
            CreatedAt = userSubscription.CreatedAt,
            UpdatedAt = userSubscription.UpdatedAt,
            IsActive = userSubscription.IsActive,
        };
    }
}
