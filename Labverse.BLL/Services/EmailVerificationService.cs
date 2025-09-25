using Labverse.BLL.Interfaces;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.UnitOfWork;
using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Security.Cryptography;

namespace Labverse.BLL.Services
{
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IEmailService _emailService;
        private readonly IConfiguration _configuration;

        public EmailVerificationService(IUnitOfWork unitOfWork, IEmailService emailService, IConfiguration configuration)
        {
            _unitOfWork = unitOfWork;
            _emailService = emailService;
            _configuration = configuration;
        }

        public async Task<string> GenerateAndSaveTokenAsync(int userId)
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
            entity.IsUsed = true;
            _unitOfWork.EmailVerificationTokens.Update(entity);
            var user = await _unitOfWork.Users.GetByIdAsync(entity.UserId);
            if (user != null)
            {
                user.EmailVerifiedAt = DateTime.UtcNow;
                _unitOfWork.Users.Update(user);
            }
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task SendVerificationEmailAsync(int userId, string email)
        {
            var token = await GenerateAndSaveTokenAsync(userId);
            var encodedToken = Uri.EscapeDataString(token);
            var verifyUrl = $"https://localhost:7106/api/users/verify-email?token={encodedToken}";
            var htmlBody = $@"
        <html>
          <body style='background: #f6f6f6; font-family: Arial, sans-serif; padding: 40px;'>
            <div style='max-width: 500px; margin: auto; background: #fff; border-radius: 8px; box-shadow: 0 2px 8px #eee; padding: 32px;'>
              <h2 style='color: #2d3748;'>Labverse Email Verification</h2>
              <p style='font-size: 16px; color: #4a5568;'>
                Thank you for signing up! Please verify your email address to activate your account.
              </p>
              <a href='{verifyUrl}' style='display: inline-block; margin-top: 24px; padding: 12px 24px; background: #3182ce; color: #fff; text-decoration: none; border-radius: 4px; font-weight: bold; font-size: 16px;'>
                Verify Email
              </a>
              <p style='margin-top: 32px; font-size: 13px; color: #a0aec0;'>
                If you did not sign up for Labverse, please ignore this email.
              </p>
            </div>
          </body>
        </html>";
            await _emailService.SendEmailAsync(email, "Labverse - Verify your email", htmlBody);
        }
    }
}
