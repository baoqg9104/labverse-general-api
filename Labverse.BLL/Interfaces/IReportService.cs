using Labverse.BLL.DTOs.Reports;

namespace Labverse.BLL.Interfaces;

public class ReportListQuery
{
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
    public string[]? Type { get; set; }
    public string[]? Status { get; set; }
    public string[]? Severity { get; set; }
    public string? Q { get; set; }
    public string Period { get; set; } = "30d";
    public DateTime? Since { get; set; }
    public DateTime? Until { get; set; }
    public string SortBy { get; set; } = "createdAt";
    public string SortDir { get; set; } = "desc";
}

public interface IReportService
{
    Task<ReportDto> CreateAsync(int reporterId, string reporterEmail, CreateReportRequest req, string ip);
    Task<ReportsPageDto<ReportDto>> ListAsync(ReportListQuery query);
    Task<ReportDto?> GetByIdAsync(int id);
    Task<ReportDto> UpdateAsync(int id, int adminId, UpdateReportRequest req);
    Task<string> ExportCsvAsync(ReportListQuery query);
    Task<ReportsPageDto<ReportDto>> ListMineAsync(int reporterId, int page, int pageSize);
}
