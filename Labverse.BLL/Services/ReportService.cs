using System.Text;
using System.Text.Json;
using Labverse.BLL.DTOs.Reports;
using Labverse.BLL.Interfaces;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Labverse.BLL.Services;

public class ReportService : IReportService
{
    private readonly IUnitOfWork _uow;

    public ReportService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<ReportDto> CreateAsync(int reporterId, string reporterEmail, CreateReportRequest req, string ip)
    {
        ValidateCreate(req);
        var imagePaths = (req.ImagePaths ?? Array.Empty<string>()).Take(5).ToArray();
        var entity = new Report
        {
            ReporterId = reporterId,
            ReporterEmail = reporterEmail,
            Type = req.Type,
            Severity = req.Severity,
            Title = Sanitize(req.Title),
            Description = Sanitize(req.Description),
            Status = ReportStatus.Open,
            ImagePathsJson = JsonSerializer.Serialize(imagePaths)
        };
        await _uow.Reports.AddAsync(entity);
        await _uow.SaveChangesAsync();
        return ToDto(entity);
    }

    public async Task<ReportsPageDto<ReportDto>> ListAsync(ReportListQuery q)
    {
        var query = _uow.Reports.Query();

        // filters
        if (q.Type != null && q.Type.Length > 0)
        {
            var types = q.Type.Select(ParseType).ToArray();
            query = query.Where(r => types.Contains(r.Type));
        }
        if (q.Status != null && q.Status.Length > 0)
        {
            var statuses = q.Status.Select(ParseStatus).ToArray();
            query = query.Where(r => statuses.Contains(r.Status));
        }
        if (q.Severity != null && q.Severity.Length > 0)
        {
            var sevs = q.Severity.Select(ParseSeverity).ToArray();
            query = query.Where(r => sevs.Contains(r.Severity));
        }
        if (!string.IsNullOrWhiteSpace(q.Q))
        {
            var term = q.Q.Trim();
            query = query.Where(r => r.Title.Contains(term) || r.Description.Contains(term) || r.ReporterEmail.Contains(term));
        }

        // period / since-until
        DateTime? since = q.Since;
        DateTime? until = q.Until;
        if (!since.HasValue && !until.HasValue)
        {
            if (q.Period == "7d") since = DateTime.UtcNow.AddDays(-7);
            else if (q.Period == "30d") since = DateTime.UtcNow.AddDays(-30);
        }
        if (since.HasValue) query = query.Where(r => r.CreatedAt >= since.Value);
        if (until.HasValue) query = query.Where(r => r.CreatedAt <= until.Value);

        // sorting
        bool desc = q.SortDir?.ToLower() != "asc";
        query = q.SortBy?.ToLower() switch
        {
            "status" => (desc ? query.OrderByDescending(r => r.Status) : query.OrderBy(r => r.Status)),
            "severity" => (desc ? query.OrderByDescending(r => r.Severity) : query.OrderBy(r => r.Severity)),
            _ => (desc ? query.OrderByDescending(r => r.CreatedAt) : query.OrderBy(r => r.CreatedAt)),
        };

        var page = Math.Max(1, q.Page);
        var pageSize = Math.Clamp(q.PageSize, 1, 100);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new ReportsPageDto<ReportDto>
        {
            Items = items.Select(ToDto),
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    public async Task<ReportDto?> GetByIdAsync(int id)
    {
        var e = await _uow.Reports.GetByIdAsync(id);
        return e == null ? null : ToDto(e);
    }

    public async Task<ReportDto> UpdateAsync(int id, int adminId, UpdateReportRequest req)
    {
        var e = await _uow.Reports.GetByIdAsync(id) ?? throw new KeyNotFoundException("Report not found");
        if (req.Status.HasValue)
        {
            // transitions allowed: any between Open, InReview, Resolved
            e.Status = req.Status.Value;
            if (e.Status == ReportStatus.Resolved && req.ResolvedAt == null && e.ResolvedAt == null)
            {
                e.ResolvedAt = DateTime.UtcNow;
            }
        }
        if (req.AssignedAdminId.HasValue)
            e.AssignedAdminId = req.AssignedAdminId;
        if (req.ResolutionNotes != null)
            e.ResolutionNotes = Sanitize(req.ResolutionNotes);
        if (req.ResolvedAt.HasValue)
            e.ResolvedAt = req.ResolvedAt;
        e.UpdatedAt = DateTime.UtcNow;
        _uow.Reports.Update(e);
        await _uow.SaveChangesAsync();
        return ToDto(e);
    }

    public async Task<string> ExportCsvAsync(ReportListQuery query)
    {
        var page = await ListAsync(query);
        var sb = new StringBuilder();
        sb.AppendLine("id,reporterId,reporterEmail,type,severity,title,status,createdAt");
        foreach (var r in page.Items)
        {
            sb.AppendLine($"{r.Id},{r.ReporterId},\"{r.ReporterEmail}\",{r.Type},{r.Severity},\"{EscapeCsv(r.Title)}\",{r.Status},{r.CreatedAt:O}");
        }
        return sb.ToString();
    }

    public async Task<ReportsPageDto<ReportDto>> ListMineAsync(int reporterId, int page, int pageSize)
    {
        var query = _uow.Reports.Query().Where(r => r.ReporterId == reporterId).OrderByDescending(r => r.CreatedAt);
        var total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
        return new ReportsPageDto<ReportDto>
        {
            Items = items.Select(ToDto),
            Page = page,
            PageSize = pageSize,
            Total = total
        };
    }

    private static void ValidateCreate(CreateReportRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Title) || req.Title.Length > 200)
            throw new ArgumentException("Invalid title");
        if (string.IsNullOrWhiteSpace(req.Description) || req.Description.Length > 5000)
            throw new ArgumentException("Invalid description");
        if (req.ImagePaths != null)
        {
            if (req.ImagePaths.Length > 5) throw new ArgumentException("Too many images");
            foreach (var p in req.ImagePaths)
            {
                if (string.IsNullOrWhiteSpace(p)) throw new ArgumentException("Invalid image path");
            }
        }
    }

    private static string Sanitize(string input)
    {
        // very light sanitize: remove angle brackets
        return input.Replace("<", string.Empty).Replace(">", string.Empty);
    }

    private static ReportType ParseType(string s) => Enum.TryParse<ReportType>(s, true, out var v) ? v : ReportType.Other;
    private static ReportStatus ParseStatus(string s)
    {
        return s.ToLower() switch
        {
            "open" => ReportStatus.Open,
            "in review" or "inreview" => ReportStatus.InReview,
            "resolved" => ReportStatus.Resolved,
            _ => ReportStatus.Open
        };
    }
    private static ReportSeverity ParseSeverity(string s) => Enum.TryParse<ReportSeverity>(s, true, out var v) ? v : ReportSeverity.Low;

    private static string EscapeCsv(string s) => s.Replace("\"", "\"\"");

    private static ReportDto ToDto(Report e)
    {
        var paths = Array.Empty<string>();
        try
        {
            paths = JsonSerializer.Deserialize<string[]>(e.ImagePathsJson) ?? Array.Empty<string>();
        }
        catch { }
        return new ReportDto
        {
            Id = e.Id,
            ReporterId = e.ReporterId,
            ReporterEmail = e.ReporterEmail,
            Type = e.Type,
            Severity = e.Severity,
            Title = e.Title,
            Description = e.Description,
            Status = e.Status,
            ResolutionNotes = e.ResolutionNotes,
            AssignedAdminId = e.AssignedAdminId,
            ResolvedAt = e.ResolvedAt,
            ImagePaths = paths,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt
        };
    }
}
