using Labverse.DAL.EntitiesModels;

namespace Labverse.BLL.DTOs.Labs;

public class LabDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string MdPath { get; set; } = string.Empty;
    public string MdPublicUrl { get; set; } = string.Empty;
    public string Description { get; set; }
    public LabDifficulty DifficultyLevel { get; set; }
    public int AuthorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }

    // Aggregates
    public int Views { get; set; }
    public int UniqueUserViews { get; set; }
    public double RatingAverage { get; set; }
    public int RatingCount { get; set; }
}
