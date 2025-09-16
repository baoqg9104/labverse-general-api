namespace Labverse.DAL.EntitiesModels;

public class UserSubscription : BaseEntity
{
    public int UserId { get; set; }
    public int SubscriptionId { get; set; }
    public DateTime StartDate { get; set; } = DateTime.UtcNow;
    public DateTime EndDate { get; set; }

    public User User { get; set; } = null!;
    public Subscription Subscription { get; set; } = null!;
}
