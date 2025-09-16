using Labverse.DAL.Data;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Labverse.DAL.Repositories;

public class RefreshTokenRepository : Repository<RefreshToken>, IRefreshTokenRepository
{
    public RefreshTokenRepository(LabverseDbContext context)
        : base(context) { }

    public async Task<RefreshToken?> GetByTokenAsync(string token)
    {
        return await _context.RefreshTokens
            .FirstOrDefaultAsync(rt => rt.Token == token);
    }
}
