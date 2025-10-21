namespace Labverse.BLL.DTOs.Activities;

public class ActivityDto
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public int? LabId { get; set; }
    public int? QuestionId { get; set; }
    public string Action { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? MetadataJson { get; set; }
    public DateTime CreatedAt { get; set; }
}
