using Labverse.DAL.EntitiesModels;

namespace Labverse.BLL.DTOs.Labs;

public class UpdateLabQuestionDto
{
    public string? QuestionText { get; set; }
    public QuestionType? Type { get; set; }
    public string[]? Choices { get; set; }
    public string? CorrectText { get; set; }
    public string[]? CorrectOptions { get; set; }
    public bool? CorrectBool { get; set; }
}
