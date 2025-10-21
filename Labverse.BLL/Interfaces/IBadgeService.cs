using Labverse.DAL.EntitiesModels;

namespace Labverse.BLL.Interfaces
{
    public interface IBadgeService
    {
        public Task AwardBadgesAsync(User user);
    }
}
