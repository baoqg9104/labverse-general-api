namespace Labverse.DAL.EntitiesModels;

public enum QuestionType
{
    SingleChoice,
    MultipleChoice,
    TrueFalse,
    ShortText
}

public class LabQuestion : BaseEntity
{
    public int LabId { get; set; }
    public string QuestionText { get; set; } = string.Empty;
    public QuestionType Type { get; set; } = QuestionType.SingleChoice;

    // Serialized choices in JSON for flexibility (for Single/Multiple choice)
    public string? ChoicesJson { get; set; }

    // For SingleChoice/TrueFalse store the correct answer key/value.
    // For MultipleChoice store JSON array; For ShortText store acceptable answer(s)
    public string CorrectAnswerJson { get; set; } = string.Empty;

    public Lab Lab { get; set; } = null!;
}
