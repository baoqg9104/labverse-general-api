namespace Labverse.BLL.DTOs.Labs;

public class SubmitAnswerResponse
{
    public bool IsCorrect { get; set; }
    public int AwardedXp { get; set; }
    public bool LabCompleted { get; set; }
    public int TotalUserXp { get; set; }
    public int NewLevel { get; set; }
}
