using Labverse.API.Helpers;
using Labverse.BLL.DTOs.Payments;
using Labverse.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Net.payOS.Types;
using System.Security.Claims;

namespace Labverse.API.Controllers;

[Route("api/payments")]
[ApiController]
[Authorize]
public class PaymentController : ControllerBase
{
    private readonly IPayOSService _payOSService;

    public PaymentController(IPayOSService payOSService)
    {
        _payOSService = payOSService;
    }

    [HttpPost("create-embedded-payment-link")]
    public async Task<IActionResult> Create(SubscriptionRequest dto)
    {
        try
        {
            CreatePaymentResult createPayment = await _payOSService.CreatePaymentLink(dto);
            return Ok(createPayment);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("CREATE_PAYMENT_LINK_ERROR", ex.Message, 500);
        }
    }

    [HttpGet("{orderId}")]
    public async Task<IActionResult> GetOrder([FromRoute] long orderId)
    {
        try
        {
            PaymentLinkInformation paymentLinkInformation = await _payOSService.GetOrder(orderId);
            return Ok(paymentLinkInformation);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("GET_ORDER_ERROR", ex.Message, 500);
        }
    }

    [HttpPost("cancel/{orderId}")]
    public async Task<IActionResult> CancelOrder([FromRoute] long orderId)
    {
        try
        {
            PaymentLinkInformation paymentLinkInformation = await _payOSService.CancelOrder(orderId);
            return Ok(paymentLinkInformation);
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("CANCEL_ORDER_ERROR", ex.Message, 500);
        }
    }

    [HttpPost("confirm-webhook")]
    public async Task<IActionResult> ConfirmWebhook([FromBody] ConfirmWebhook dto)
    {
        try
        {
            await _payOSService.ConfirmWebhook(dto);
            return Ok(new { message = "Webhook confirmed successfully." });
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("CONFIRM_WEBHOOK_ERROR", ex.Message, 500);
        }
    }

    [HttpPost("activate-premium")]
    public async Task<IActionResult> ActivatePremiumIfPaid([FromBody] ConfirmSubscriptionRequest dto)
    {
        try
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (userId == null)
                return ApiErrorHelper.Error("UNAUTHORIZED", "User not authenticated", 401);

            if (!int.TryParse(userId, out var userIdInt))
                return ApiErrorHelper.Error("UNAUTHORIZED", "Invalid user id", 401);

            bool result = await _payOSService.ActivatePremiumIfPaidAsync(userIdInt, dto.OrderId, dto.SubscriptionId);
            if (result)
            {
                return Ok(new { message = "Premium activated successfully." });
            }
            else
            {
                return ApiErrorHelper.Error("ACTIVATE_PREMIUM_FAILED", "Payment not completed or other issue", 400);
            }
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("ACTIVATE_PREMIUM_ERROR", ex.Message, 500);
        }
    }
}
