namespace Labverse.BLL.DTOs.Payments;

public class ConfirmSubscriptionRequest
{
    public long OrderId { get; set; }
    //public int UserId { get; set; }
    public int SubscriptionId { get; set; }
}
