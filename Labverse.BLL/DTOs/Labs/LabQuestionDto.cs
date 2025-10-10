using Labverse.DAL.EntitiesModels;

namespace Labverse.BLL.DTOs.Labs;

public class LabQuestionDto
{
    public int Id { get; set; }
    public int LabId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public string? ChoicesJson { get; set; }
}
