using Labverse.DAL.EntitiesModels;

namespace Labverse.BLL.DTOs.Labs;

public class CreateLabDto
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public LabDifficulty DifficultyLevel { get; set; }
    public int CategoryId { get; set; }
}
