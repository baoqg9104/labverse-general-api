namespace Labverse.BLL.DTOs.Users;

public class RankingDto
{
    public int UserId { get; set; }
    public string Username { get; set; } = string.Empty;
    public int Points { get; set; } // XP
    public int Level { get; set; }
    public int StreakCurrent { get; set; }
    public int StreakBest { get; set; }
}
