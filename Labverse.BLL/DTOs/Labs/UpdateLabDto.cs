using Labverse.DAL.EntitiesModels;

namespace Labverse.BLL.DTOs.Labs;

public class UpdateLabDto
{
    public string Title { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string MdPath { get; set; } = string.Empty;
    public string MdPublicUrl { get; set; } = string.Empty;
    public string Description { get; set; }
    public LabDifficulty DifficultyLevel { get; set; }
    public int CategoryId { get; set; }
}
