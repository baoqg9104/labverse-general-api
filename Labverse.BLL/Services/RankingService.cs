using Labverse.BLL.DTOs.Users;
using Labverse.BLL.Interfaces;
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

    public async Task<IEnumerable<RankingDto>> GetTopAsync(RankingCriteria criteria, int take = 50)
    {
        var q = _unitOfWork.Users.Query();
        switch (criteria)
        {
            case RankingCriteria.Streak:
                q = q.OrderByDescending(u => u.StreakCurrent)
                     .ThenByDescending(u => u.StreakBest)
                     .ThenByDescending(u => u.Level)
                     .ThenByDescending(u => u.Points)
                     .ThenBy(u => u.Id);
                break;
            case RankingCriteria.Points:
            default:
                q = q.OrderByDescending(u => u.Level)
                     .ThenByDescending(u => u.Points)
                     .ThenByDescending(u => u.StreakBest)
                     .ThenBy(u => u.Id);
                break;
        }

        var users = await q.Take(take).ToListAsync();
        return users.Select(u => new RankingDto
        {
            UserId = u.Id,
            Username = u.Username,
            Points = u.Points,
            Level = u.Level,
            StreakCurrent = u.StreakCurrent,
            StreakBest = u.StreakBest
        });
    }
}
