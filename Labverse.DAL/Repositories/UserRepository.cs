using Labverse.DAL.Data;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace Labverse.DAL.Repositories;

public class UserRepository : Repository<User>, IUserRepository
{
    public UserRepository(LabverseDbContext context)
        : base(context) { }

    public async Task<User?> GetByEmailAsync(string email)
    {
        return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
    }
}
