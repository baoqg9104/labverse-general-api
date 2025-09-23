namespace Labverse.DAL.EntitiesModels;

public class Subscription : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Currency { get; set; } = "VND";
    public int DurationValue { get; set; }
    public string DurationUnit { get; set; } = "Month";

    public ICollection<UserSubscription> UserSubscriptions { get; set; } = new List<UserSubscription>();
}
