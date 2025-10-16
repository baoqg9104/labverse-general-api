namespace Labverse.BLL.Gamification;

// Centralized XP rules to avoid scattering magic numbers and appsettings config
public static class XpRules
{
    // Action XP awards
    public const int DailyLoginXp = 5;
    public const int NewCorrectAnswerXp = 10;
    public const int StreakMilestoneDays = 7; // 7, 14, 21, ...
    public const int StreakMilestoneXp = 100;
    public const int LabCompletionXp = 50;

    // Level progression
    public const int BaseLevelXp = 100; // base scaling; adjust in code if needed

    public static int RequiredTotalXp(int level) => BaseLevelXp * (level * (level + 1)) / 2; // triangular progression
}
