using Labverse.BLL.DTOs.Users;

namespace Labverse.BLL.Interfaces;

public enum RankingCriteria
{
    Points,
    Streak
}

public interface IRankingService
{
    Task<IEnumerable<RankingDto>> GetTopAsync(RankingCriteria criteria, int take = 50);
}
