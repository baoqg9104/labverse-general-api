using Labverse.BLL.DTOs.Activities;
using Labverse.BLL.DTOs.Paging;
using Labverse.BLL.Interfaces;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Labverse.BLL.Services;

public class ActivityQueryService : IActivityQueryService
{
    private readonly IUnitOfWork _uow;

    public ActivityQueryService(IUnitOfWork uow)
    {
        _uow = uow;
    }

    public async Task<PagedResult<ActivityDto>> ListAsync(ActivityListQuery query)
    {
        var q = _uow.ActivityHistories.Query();

        if (query.UserId.HasValue)
            q = q.Where(a => a.UserId == query.UserId.Value);
        if (query.LabId.HasValue)
            q = q.Where(a => a.LabId == query.LabId.Value);
        if (query.QuestionId.HasValue)
            q = q.Where(a => a.QuestionId == query.QuestionId.Value);
        if (query.Actions != null && query.Actions.Length > 0)
            q = q.Where(a => query.Actions.Contains(a.Action));
        if (query.Since.HasValue)
            q = q.Where(a => a.CreatedAt >= query.Since.Value);
        if (query.Until.HasValue)
            q = q.Where(a => a.CreatedAt <= query.Until.Value);

        bool desc = query.SortDir?.ToLower() != "asc";
        q = desc ? q.OrderByDescending(a => a.CreatedAt) : q.OrderBy(a => a.CreatedAt);

        var page = Math.Max(1, query.Page);
        var pageSize = Math.Clamp(query.PageSize, 1, 200);
        var total = await q.CountAsync();
        var items = await q.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<ActivityDto>
        {
            Items = items.Select(ToDto),
            Page = page,
            PageSize = pageSize,
            Total = total,
        };
    }

    private static ActivityDto ToDto(ActivityHistory e)
    {
        return new ActivityDto
        {
            Id = e.Id,
            UserId = e.UserId,
            LabId = e.LabId,
            QuestionId = e.QuestionId,
            Action = e.Action,
            Description = e.Description,
            MetadataJson = e.MetadataJson,
            CreatedAt = e.CreatedAt,
        };
    }
}
