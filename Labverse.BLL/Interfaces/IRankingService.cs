using Labverse.BLL.DTOs.Ranking;
using Labverse.DAL.EntitiesModels;

namespace Labverse.BLL.Interfaces;

public enum RankingCriteria
{
    Points,
    Streak,
    Badges,
}

public interface IRankingService
{
    Task<IEnumerable<RankingResponse>> GetTopByRoleAsync(
        RankingCriteria criteria,
        UserRole role,
        int take = 50
    );
}
