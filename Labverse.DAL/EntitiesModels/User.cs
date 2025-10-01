namespace Labverse.DAL.EntitiesModels;

public enum UserRole
{
    User,
    Admin,
    Author
}

public class User : BaseEntity
{
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public string? Bio { get; set; }
    public UserRole Role { get; set; } = UserRole.User;
    public DateTime? EmailVerifiedAt { get; set; }

    public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
    public ICollection<Lab> CreatedLabs { get; set; } = new List<Lab>();
    public ICollection<UserProgress> Progresses { get; set; } = new List<UserProgress>();
    public ICollection<UserBadge> UserBadges { get; set; } = new List<UserBadge>();
    public ICollection<Message> Messages { get; set; } = new List<Message>();
    public ICollection<ChatRoomUser> ChatRooms { get; set; } = new List<ChatRoomUser>();
    public ICollection<Resource> Resources { get; set; } = new List<Resource>();
}
