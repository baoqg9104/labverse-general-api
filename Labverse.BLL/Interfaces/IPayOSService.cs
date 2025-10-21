using Labverse.BLL.DTOs.Payments;
using Net.payOS.Types;

namespace Labverse.BLL.Interfaces;

public interface IPayOSService
{
    Task<CreatePaymentResult> CreatePaymentLink(int userId, SubscriptionRequest dto);
    Task<PaymentLinkInformation> GetOrder(long orderId);
    Task<PaymentLinkInformation> CancelOrder(long orderId);
    Task ConfirmWebhook(ConfirmWebhook dto);
    Task<bool> ActivatePremiumIfPaidAsync(int userId, long orderId, int subscriptionId);
}
