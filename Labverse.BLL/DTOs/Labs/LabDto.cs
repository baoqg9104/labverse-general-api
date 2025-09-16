using Labverse.DAL.EntitiesModels;

namespace Labverse.BLL.DTOs.Labs;

public class LabDto
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public LabDifficulty DifficultyLevel { get; set; }
    public int AuthorId { get; set; }
    public int CategoryId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public bool IsActive { get; set; }
}
