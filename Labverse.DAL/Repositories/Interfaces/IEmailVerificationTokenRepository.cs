using Labverse.DAL.EntitiesModels;

namespace Labverse.DAL.Repositories.Interfaces
{
    public interface IEmailVerificationTokenRepository : IRepository<EmailVerificationToken>
    {
        Task<EmailVerificationToken?> GetByTokenAsync(string token);
    }
}