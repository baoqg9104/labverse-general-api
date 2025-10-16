using Labverse.DAL.EntitiesModels;
using System.Text.Json;

namespace Labverse.BLL.DTOs.Reports;

public class ReportDto
{
    public int Id { get; set; }
    public int ReporterId { get; set; }
    public string ReporterEmail { get; set; } = string.Empty;
    public ReportType Type { get; set; }
    public ReportSeverity Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public ReportStatus Status { get; set; }
    public string? ResolutionNotes { get; set; }
    public int? AssignedAdminId { get; set; }
    public DateTime? ResolvedAt { get; set; }
    public string[] ImagePaths { get; set; } = Array.Empty<string>();
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
