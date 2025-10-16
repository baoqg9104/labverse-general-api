namespace Labverse.DAL.EntitiesModels;

public enum LabDifficulty
{
    Basic,
    Intermediate,
    Advanced,
}

public class Lab : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public LabDifficulty DifficultyLevel { get; set; }

    public string Slug { get; set; } = string.Empty;
    public string MdPath { get; set; } = string.Empty;
    public string MdPublicUrl { get; set; } = string.Empty;

    public int AuthorId { get; set; }

    public User Author { get; set; } = null!;

    public ICollection<LabQuestion> Questions { get; set; } = new List<LabQuestion>();
}
