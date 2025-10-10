using Labverse.DAL.EntitiesModels;

namespace Labverse.BLL.DTOs.Users;

public class UserDto
{
    public int Id { get; set; }
    public string Email { get; set; }
    public string Username { get; set; }
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public UserRole Role { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
    public DateTime? EmailVerifiedAt { get; set; }
    public string Subscription { get; set; } = "Free"; // Free, Premium

    // Gamification fields
    public int Points { get; set; } // Acts as XP
    public int Level { get; set; }
    public int StreakCurrent { get; set; }
    public int StreakBest { get; set; }
    public DateTime? LastActiveAt { get; set; }
}
