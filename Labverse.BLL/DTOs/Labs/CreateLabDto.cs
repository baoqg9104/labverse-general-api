using Labverse.DAL.EntitiesModels;

namespace Labverse.BLL.DTOs.Labs;

public class CreateLabDto
{
    public string Title { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string MdPath { get; set; } = string.Empty;
    public string MdPublicUrl { get; set; } = string.Empty;
    public LabDifficulty DifficultyLevel { get; set; }
}
