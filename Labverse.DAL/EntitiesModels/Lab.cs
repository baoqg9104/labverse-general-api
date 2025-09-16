namespace Labverse.DAL.EntitiesModels;

public enum LabDifficulty
{
    Basic,
    Intermediate,
    Advanced
}

public class Lab : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public LabDifficulty DifficultyLevel { get; set; }
    public int AuthorId { get; set; }
    public int CategoryId { get; set; }

    public User Author { get; set; } = null!;
    public LabCategory Category { get; set; } = null!;

}
