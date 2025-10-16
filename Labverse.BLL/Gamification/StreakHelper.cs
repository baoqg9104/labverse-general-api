using Labverse.DAL.EntitiesModels;

namespace Labverse.BLL.Gamification;

public static class StreakHelper
{
    // Updates streak based on an activity at utcNow, updates LastActiveAt and StreakBest,
    // applies milestone bonus if applicable, and returns (increased, milestoneAwardedXp)
    public static (bool increased, int milestoneAwardedXp) UpdateForActivity(User user, DateTime utcNow)
    {
        var today = utcNow.Date;
        var increased = false;

        if (user.LastActiveAt == null)
        {
            user.StreakCurrent = 1;
            increased = true;
        }
        else
        {
            var last = user.LastActiveAt.Value.Date;
            if (last == today)
            {
                // same day, do not change
            }
            else if (last == today.AddDays(-1))
            {
                user.StreakCurrent += 1;
                increased = true;
            }
            else
            {
                user.StreakCurrent = 1;
                increased = true;
            }
        }

        user.LastActiveAt = utcNow;
        if (user.StreakCurrent > user.StreakBest)
            user.StreakBest = user.StreakCurrent;

        int milestoneAwarded = 0;
        if (
            increased
            && user.StreakCurrent >= XpRules.StreakMilestoneDays
            && user.StreakCurrent % XpRules.StreakMilestoneDays == 0
        )
        {
            if (user.LastStreakBonusAtDays < user.StreakCurrent)
            {
                user.Points += XpRules.StreakMilestoneXp;
                milestoneAwarded = XpRules.StreakMilestoneXp;
                user.LastStreakBonusAtDays = user.StreakCurrent;
            }
        }

        return (increased, milestoneAwarded);
    }
}
