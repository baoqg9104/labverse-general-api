namespace Labverse.BLL.DTOs.Ranking;

public class RankingResponse
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string? AvatarUrl { get; set; }
    public int Level { get; set; }
    public int Points { get; set; }
    public int StreakCurrent { get; set; }
    public int StreakBest { get; set; }
    public int BadgesCount { get; set; }
}
