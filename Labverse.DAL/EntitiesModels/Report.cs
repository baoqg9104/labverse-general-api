namespace Labverse.DAL.EntitiesModels;

public enum ReportType
{
    Bug,
    Abuse,
    Payment,
    Other
}

public enum ReportSeverity
{
    Low,
    Medium,
    High
}

public enum ReportStatus
{
    Open,
    InReview,
    Resolved
}

public class Report : BaseEntity
{
    public int ReporterId { get; set; }
    public string ReporterEmail { get; set; } = string.Empty;

    public ReportType Type { get; set; }
    public ReportSeverity Severity { get; set; }

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public ReportStatus Status { get; set; } = ReportStatus.Open;

    public string? ResolutionNotes { get; set; }
    public int? AssignedAdminId { get; set; }
    public DateTime? ResolvedAt { get; set; }

    // Store array as JSON string
    public string ImagePathsJson { get; set; } = "[]";

    public User Reporter { get; set; } = null!;
    public User? AssignedAdmin { get; set; }
}
