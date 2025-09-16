using Labverse.DAL.EntitiesModels;

namespace Labverse.DAL.Repositories.Interfaces;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByEmailAsync(string email);
}
