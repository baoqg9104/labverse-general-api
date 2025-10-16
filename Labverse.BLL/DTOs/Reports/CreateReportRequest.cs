using Labverse.DAL.EntitiesModels;

namespace Labverse.BLL.DTOs.Reports;

public class CreateReportRequest
{
    public ReportType Type { get; set; }
    public ReportSeverity Severity { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string[]? ImagePaths { get; set; }
}
