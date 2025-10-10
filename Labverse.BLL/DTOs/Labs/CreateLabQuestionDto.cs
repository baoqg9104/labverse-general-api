using Labverse.DAL.EntitiesModels;

namespace Labverse.BLL.DTOs.Labs;

public class CreateLabQuestionDto
{
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType Type { get; set; }
    public string[]? Choices { get; set; }
    public string? CorrectText { get; set; }
    public string[]? CorrectOptions { get; set; }
    public bool? CorrectBool { get; set; }
}
