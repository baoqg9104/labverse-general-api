using Labverse.BLL.DTOs.Payments;
using Labverse.BLL.Interfaces;
using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;


namespace Labverse.BLL.Services;

public class PayOSService : IPayOSService
{
    private readonly PayOS _payOS;
    private readonly IConfiguration _configuration;
    private readonly IUserSubscriptionService _userSubscriptionService;

    public PayOSService(PayOS payOS, IConfiguration configuration, IUserSubscriptionService userSubscriptionService)
    {
        _payOS = payOS;
        _configuration = configuration;
        _userSubscriptionService = userSubscriptionService;
    }

    public async Task<PaymentLinkInformation> CancelOrder(long orderId)
    {
        PaymentLinkInformation paymentLinkInformation = await _payOS.cancelPaymentLink(orderId);

        return paymentLinkInformation;
    }

    public async Task ConfirmWebhook(ConfirmWebhook dto)
    {
        await _payOS.confirmWebhook(dto.WebhookUrl);
    }

    public async Task<CreatePaymentResult> CreatePaymentLink(SubscriptionRequest dto)
    {
        string domain = _configuration["Frontend:BaseUrl"] ?? "https://localhost:5173";

        ItemData item = new(dto.ProductName, 1, dto.Price);

        var paymentLinkRequest = new PaymentData(
            orderCode: long.Parse($"{DateTime.UtcNow:yyMMddHHmmss}{Random.Shared.Next(10, 99)}"),
            amount: dto.Price,
            description: dto.Description,
            items: [item],
            returnUrl: dto.ReturnUrl,
            cancelUrl: dto.CancelUrl
        );

        CreatePaymentResult createPayment = await _payOS.createPaymentLink(paymentLinkRequest);

        return createPayment;
    }

    public async Task<PaymentLinkInformation> GetOrder(long orderId)
    {
        PaymentLinkInformation paymentLinkInformation = await _payOS.getPaymentLinkInformation(
            orderId
        );

        return paymentLinkInformation;
    }

    public async Task<bool> ActivatePremiumIfPaidAsync(int userId, long orderId, int subscriptionId)
    {

        var info = await _payOS.getPaymentLinkInformation(orderId);
        if (string.Equals(info.status, "PAID", StringComparison.OrdinalIgnoreCase))
        {
            await _userSubscriptionService.CreateUserSubscriptionAsync(userId, subscriptionId);
            return true;
        }
        return false;
    }
}
