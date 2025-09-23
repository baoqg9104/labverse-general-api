using Labverse.DAL.EntitiesModels;
using Labverse.DAL.UnitOfWork;
using System.Security.Cryptography;

namespace Labverse.BLL.Services
{
    public class EmailVerificationService : IEmailVerificationService
    {
        private readonly IUnitOfWork _unitOfWork;

        public EmailVerificationService(IUnitOfWork unitOfWork)
        {
            _unitOfWork = unitOfWork;
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
    }
}
