namespace Labverse.DAL.EntitiesModels;

public class LabRating : BaseEntity
{
    public int LabId { get; set; }
    public int UserId { get; set; }
    public int Score { get; set; } // 1..5
    public string? Comment { get; set; }

    public Lab Lab { get; set; } = null!;
    public User User { get; set; } = null!;
}
