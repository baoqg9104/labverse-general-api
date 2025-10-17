using Labverse.BLL.DTOs.Ranking;
using Labverse.BLL.Interfaces;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Labverse.BLL.Services;

public class RankingService : IRankingService
{
    private readonly IUnitOfWork _unitOfWork;

    public RankingService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<RankingResponse>> GetTopByRoleAsync(
        RankingCriteria criteria,
        UserRole role,
        int take = 50
    )
    {
        var q = _unitOfWork
            .Users.Query()
            .Where(u => u.Role == role)
            .Select(u => new
            {
                User = u,
                BadgesCount = _unitOfWork.UserBadges.Query().Count(ub => ub.UserId == u.Id),
            });

        switch (criteria)
        {
            case RankingCriteria.Streak:
                q = q.OrderByDescending(x => x.User.StreakCurrent)
                    .ThenByDescending(x => x.User.StreakBest)
                    .ThenByDescending(x => x.User.Level)
                    .ThenByDescending(x => x.User.Points)
                    .ThenBy(x => x.User.Id);
                break;
            case RankingCriteria.Badges:
                q = q.OrderByDescending(x => x.BadgesCount)
                    .ThenByDescending(x => x.User.Level)
                    .ThenByDescending(x => x.User.Points)
                    .ThenBy(x => x.User.Id);
                break;
            case RankingCriteria.Points:
            default:
                q = q.OrderByDescending(x => x.User.Level)
                    .ThenByDescending(x => x.User.Points)
                    .ThenByDescending(x => x.User.StreakBest)
                    .ThenBy(x => x.User.Id);
                break;
        }

        var users = await q.Take(take).ToListAsync();
        return users.Select(x => new RankingResponse
        {
            UserId = x.User.Id,
            Username = x.User.Username,
            AvatarUrl = x.User.AvatarUrl,
            Points = x.User.Points,
            Level = x.User.Level,
            StreakCurrent = x.User.StreakCurrent,
            StreakBest = x.User.StreakBest,
            BadgesCount = x.BadgesCount,
        });
    }
}
