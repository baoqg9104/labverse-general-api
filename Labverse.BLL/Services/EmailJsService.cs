using Labverse.BLL.DTOs.Email;
using Labverse.BLL.Interfaces;
using Labverse.BLL.Settings;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.UnitOfWork;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Security.Cryptography;

namespace Labverse.BLL.Services;

public class EmailJsService : IEmailJsService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly HttpClient _httpClient;
    private readonly EmailJsSettings _emailJsSettings;
    private readonly IConfiguration _configuration;

    public EmailJsService(
        IUnitOfWork unitOfWork,
        HttpClient httpClient,
        IOptions<EmailJsSettings> emailJsSettings,
        IConfiguration configuration
    )
    {
        _unitOfWork = unitOfWork;
        _httpClient = httpClient;
        _emailJsSettings = emailJsSettings.Value;
        _configuration = configuration;
    }

    private async Task<string> GenerateAndSaveTokenAsync(int userId)
    {
        var randomBytes = new byte[32];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        var token = Convert.ToBase64String(randomBytes);
        var expires = DateTime.UtcNow.AddHours(24);
        var entity = new EmailVerificationToken
        {
            Token = token,
            Expires = expires,
            UserId = userId,
            IsUsed = false,
        };
        await _unitOfWork.EmailVerificationTokens.AddAsync(entity);
        await _unitOfWork.SaveChangesAsync();
        return token;
    }

    public async Task<bool> VerifyTokenAsync(string token)
    {
        var entity = await _unitOfWork.EmailVerificationTokens.GetByTokenAsync(token);
        if (entity == null || entity.IsUsed || entity.Expires < DateTime.UtcNow)
            return false;

        // Mark token as used. Entities retrieved from repository are tracked by EF, so no explicit Update is necessary.
        entity.IsUsed = true;

        // Fetch the user by id (tracked) and update the verification timestamp. Avoid calling Update to prevent accidental inserts.
        var user = await _unitOfWork.Users.GetByIdAsync(entity.UserId);
        if (user != null)
        {
            user.EmailVerifiedAt = DateTime.UtcNow;
        }

        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    private async Task SendEmailAsync(string templateId, object template_params)
    {
        var payload = new
        {
            service_id = _emailJsSettings.ServiceId,
            template_id = templateId,
            user_id = _emailJsSettings.PublicKey,
            template_params,
        };

        var response = await _httpClient.PostAsJsonAsync(
            "https://api.emailjs.com/api/v1.0/email/send",
            payload
        );

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync();
            throw new Exception($"EmailJS error: {response.StatusCode} - {error}");
        }
    }

    public async Task SendContactUsEmailAsync(ContactUsDto dto)
    {
        var templateParams = new
        {
            name = dto.Name,
            email = dto.Email,
            message = dto.Message,
        };

        await SendEmailAsync(_emailJsSettings.ContactUsTemplateId, templateParams);
    }

    public async Task SendVerificationEmailAsync(int userId, string email)
    {
        var token = await GenerateAndSaveTokenAsync(userId);
        var encodedToken = Uri.EscapeDataString(token);

        var baseUrl = _configuration["APP_BASE_URL"] ?? "https://localhost:7106";

        var verifyUrl = $"{baseUrl}/api/users/verify-email?token={encodedToken}";

        var templateParams = new { email, verifyUrl };

        // Use the correct settings property name
        await SendEmailAsync(_emailJsSettings.VerifyEmailTemplateId, templateParams);
    }
}
