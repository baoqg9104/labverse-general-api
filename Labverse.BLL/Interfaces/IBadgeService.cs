using Labverse.DAL.EntitiesModels;

namespace Labverse.BLL.Interfaces
{
    public interface IBadgeService
    {
        public Task AwardBadgesAsync(User user);
        Task<IEnumerable<Badge>> GetAllAsync();
        Task<Badge?> GetByIdAsync(int id);
        Task<Badge> CreateAsync(Badge badge);
        Task<Badge?> UpdateAsync(int id, Badge updated);
        Task<bool> DeleteAsync(int id);
    }
}
