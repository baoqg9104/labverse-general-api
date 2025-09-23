using Labverse.DAL.Data;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Labverse.DAL.Repositories
{
    public class EmailVerificationTokenRepository : Repository<EmailVerificationToken>, IEmailVerificationTokenRepository
    {
        public EmailVerificationTokenRepository(LabverseDbContext context) : base(context) { }

        public async Task<EmailVerificationToken?> GetByTokenAsync(string token)
        {
            return await _context.EmailVerificationTokens.FirstOrDefaultAsync(t => t.Token == token);
        }
    }
}