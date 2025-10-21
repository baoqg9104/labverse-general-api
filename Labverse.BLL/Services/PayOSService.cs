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
    private readonly IActivityLogService _activity;

    public PayOSService(
        PayOS payOS,
        IConfiguration configuration,
        IUserSubscriptionService userSubscriptionService,
        IActivityLogService activity
    )
    {
        _payOS = payOS;
        _configuration = configuration;
        _userSubscriptionService = userSubscriptionService;
        _activity = activity;
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

    public async Task<CreatePaymentResult> CreatePaymentLink(int userId, SubscriptionRequest dto)
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

        try
        {
            await _activity.LogAsync(
                userId,
                "payment_link_created",
                metadata: new
                {
                    amount = dto.Price,
                    product = dto.ProductName,
                    orderCode = createPayment.orderCode,
                },
                description: $"Payment link created for {dto.ProductName} ({dto.Price})"
            );
        }
        catch { }

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
            try
            {
                await _activity.LogAsync(
                    userId,
                    "payment_completed",
                    metadata: new { orderId, subscriptionId },
                    description: "Payment completed; Premium activated ⭐"
                );
            }
            catch { }
            return true;
        }
        return false;
    }
}
