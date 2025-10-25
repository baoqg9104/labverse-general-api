using Labverse.BLL.DTOs.Badge;
using Labverse.DAL.EntitiesModels;

namespace Labverse.BLL.Interfaces
{
    public interface IBadgeService
    {
        public Task AwardBadgesAsync(User user);
        Task<IEnumerable<Badge>> GetAllAsync();
        Task<Badge?> GetByIdAsync(int id);
        Task<Badge> CreateAsync(BadgesRequest request);
        Task<Badge?> UpdateAsync(int id, BadgesRequest request);
        Task<bool> DeleteAsync(int id);
    }
}
