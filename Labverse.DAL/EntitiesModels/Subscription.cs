namespace Labverse.DAL.EntitiesModels;

public class Subscription : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "VND";
    public int DurationInDays { get; set; }

    public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}
