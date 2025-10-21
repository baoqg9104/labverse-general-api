namespace Labverse.BLL.DTOs.Activities;

public class ActivityListQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public int? UserId { get; set; }
    public string[]? Actions { get; set; }
    public int? LabId { get; set; }
    public int? QuestionId { get; set; }
    public DateTime? Since { get; set; }
    public DateTime? Until { get; set; }
    public string SortDir { get; set; } = "desc"; // desc|asc by CreatedAt
}
