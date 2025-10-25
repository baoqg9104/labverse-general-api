using Labverse.BLL.DTOs.Badge;
using Labverse.BLL.Interfaces;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.UnitOfWork;

namespace Labverse.BLL.Services
{
    public class BadgeService : IBadgeService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly ISupabaseService _supabaseService;

        public BadgeService(IUnitOfWork unitOfWork, ISupabaseService supabaseService)
        {
            _unitOfWork = unitOfWork;
            _supabaseService = supabaseService;
        }

        public async Task AwardBadgesAsync(User user)
        {
            var allBadges = await _unitOfWork.Badges.GetAllAsync();
            var userBadgeIds = user.UserBadges.Select(ub => ub.BadgeId).ToHashSet();

            foreach (var badge in allBadges)
            {
                if (ShouldAward(badge, user) && !userBadgeIds.Contains(badge.Id))
                {
                    user.UserBadges.Add(new UserBadge
                    {
                        UserId = user.Id,
                        BadgeId = badge.Id,
                        DateAwarded = DateTime.UtcNow
                    });
                }
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<Badge> CreateAsync(BadgesRequest request)
        {
            string iconUrl = string.Empty;

            if (request.Icon != null)
            {
                using var stream = request.Icon.OpenReadStream();
                iconUrl = await _supabaseService.UploadBadgeIconAsync(stream, request.Icon.FileName);
            }

            var badge = new Badge
            {
                Name = request.Name,
                Description = request.Description,
                IconUrl = iconUrl
            };

            await _unitOfWork.Badges.AddAsync(badge);
            await _unitOfWork.SaveChangesAsync();

            return badge;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var badge = await _unitOfWork.Badges.GetByIdAsync(id);
            if (badge == null) return false;

            _unitOfWork.Badges.Remove(badge);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }

        public async Task<IEnumerable<Badge>> GetAllAsync()
        {
            return await _unitOfWork.Badges.GetAllAsync();
        }

        public async Task<Badge?> GetByIdAsync(int id)
        {
            return await _unitOfWork.Badges.GetByIdAsync(id);
        }

        public async Task<Badge?> UpdateAsync(int id, BadgesRequest request)
        {
            var badge = await _unitOfWork.Badges.GetByIdAsync(id);
            if (badge == null) return null;

            string iconUrl = string.Empty;

            if (request.Icon != null)
            {
                using var stream = request.Icon.OpenReadStream();
                iconUrl = await _supabaseService.UploadBadgeIconAsync(stream, request.Icon.FileName);
            }

            badge.Name = request.Name;
            badge.Description = request.Description;
            badge.IconUrl = iconUrl;

            _unitOfWork.Badges.Update(badge);
            await _unitOfWork.SaveChangesAsync();
            return badge;
        }

        private bool ShouldAward(Badge badge, User user)
        {
            return badge.Name switch
            {
                "Level 5 Achiever" => user.Level >= 5,
                "Level 10 Master" => user.Level >= 10,
                "7-Day Streak" => user.StreakBest >= 7,
                "30-Day Streak" => user.StreakBest >= 30,
                "Daily Visitor" => user.StreakCurrent >= 1,
                _ => false
            };
        }
    }
}
