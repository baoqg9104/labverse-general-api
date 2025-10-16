using Labverse.DAL.EntitiesModels;

namespace Labverse.BLL.DTOs.Reports;

public class UpdateReportRequest
{
    public ReportStatus? Status { get; set; }
    public int? AssignedAdminId { get; set; }
    public string? ResolutionNotes { get; set; }
    public DateTime? ResolvedAt { get; set; }
}
