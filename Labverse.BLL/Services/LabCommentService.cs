using Labverse.BLL.DTOs.Labs;
using Labverse.BLL.DTOs.Paging;
using Labverse.BLL.Interfaces;
using Labverse.DAL.EntitiesModels;
using Labverse.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Labverse.BLL.Services;

public class LabCommentService : ILabCommentService
{
    private readonly IUnitOfWork _uow;
    private readonly IActivityLogService _activity;

    public LabCommentService(IUnitOfWork uow, IActivityLogService activity)
    {
        _uow = uow;
        _activity = activity;
    }

    public async Task AddCommentAsync(int labId, int userId, string content, int? parentId = null)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required");
        var lab =
            await _uow.Labs.GetByIdAsync(labId) ?? throw new KeyNotFoundException("Lab not found");

        var comment = new LabComment
        {
            LabId = labId,
            UserId = userId,
            Content = content.Trim(),
            ParentId = parentId,
        };
        await _uow.LabComments.AddAsync(comment);
        await _uow.SaveChangesAsync();
        try
        {
            await _activity.LogAsync(
                userId,
                "lab_commented",
                labId,
                null,
                new { labId, commentId = comment.Id },
                description: "Commented on cyber lab 💬"
            );
        }
        catch { }
    }

    public async Task<PagedResult<LabCommentDto>> GetCommentsAsync(
        int labId,
        int page = 1,
        int pageSize = 20
    )
    {
        var query = _uow
            .LabComments.Query()
            .Include(c => c.User)
            .Where(c => c.LabId == labId)
            .OrderByDescending(c => c.CreatedAt);
        var items = await query
            .Skip((Math.Max(1, page) - 1) * Math.Clamp(pageSize, 1, 200))
            .Take(Math.Clamp(pageSize, 1, 200))
            .ToListAsync();

        return new PagedResult<LabCommentDto>
        {
            Items = items.Select(ToDto),
            Page = page,
            PageSize = pageSize,
            Total = await query.CountAsync(),
        };
    }

    //public async Task<PagedResult<LabCommentTreeDto>> GetCommentTreeAsync(
    //    int labId,
    //    int page = 1,
    //    int pageSize = 20
    //)
    //{
    //    var all = await _uow
    //        .LabComments.Query()
    //        .Where(c => c.LabId == labId)
    //        .OrderBy(c => c.CreatedAt)
    //        .ToListAsync();

    //    // Build lookup by parentId
    //    var map = all.ToDictionary(c => c.Id);
    //    var nodes = all.Select(c => new LabCommentTreeDto
    //    {
    //        Id = c.Id,
    //        LabId = c.LabId,
    //        UserId = c.UserId,
    //        Content = c.IsActive ? c.Content : "(deleted)",
    //        ParentId = c.ParentId,
    //        CreatedAt = c.CreatedAt,
    //        IsDeleted = !c.IsActive,
    //    })
    //        .ToDictionary(n => n.Id);

    //    foreach (var node in nodes.Values)
    //    {
    //        if (node.ParentId.HasValue && nodes.TryGetValue(node.ParentId.Value, out var parent))
    //        {
    //            parent.Replies.Add(node);
    //        }
    //    }

    //    var roots = nodes.Values.Where(n => n.ParentId == null).OrderByDescending(n => n.CreatedAt);
    //    return roots
    //        .Skip((Math.Max(1, page) - 1) * Math.Clamp(pageSize, 1, 200))
    //        .Take(Math.Clamp(pageSize, 1, 200));
    //}

    public async Task EditCommentAsync(int commentId, int userId, string content)
    {
        if (string.IsNullOrWhiteSpace(content))
            throw new ArgumentException("Content is required");
        var c =
            await _uow.LabComments.GetByIdAsync(commentId)
            ?? throw new KeyNotFoundException("Comment not found");
        if (c.UserId != userId)
            throw new UnauthorizedAccessException("Cannot edit others' comments");
        c.Content = content.Trim();
        c.UpdatedAt = DateTime.UtcNow;
        _uow.LabComments.Update(c);
        await _uow.SaveChangesAsync();
    }

    public async Task DeleteCommentAsync(int commentId, int userId)
    {
        var c =
            await _uow.LabComments.GetByIdAsync(commentId)
            ?? throw new KeyNotFoundException("Comment not found");
        if (c.UserId != userId)
            throw new UnauthorizedAccessException("Cannot delete others' comments");
        // Soft delete: mark inactive, keep content for tree, mask content
        c.IsActive = false;
        c.UpdatedAt = DateTime.UtcNow;

        // delete all replies recursively
        async Task DeleteRepliesAsync(int parentId)
        {
            var replies = await _uow
                .LabComments.Query()
                .Where(rc => rc.ParentId == parentId && rc.IsActive)
                .ToListAsync();

            foreach (var reply in replies)
            {
                reply.IsActive = false;
                reply.UpdatedAt = DateTime.UtcNow;
                _uow.LabComments.Update(reply);
                await DeleteRepliesAsync(reply.Id);
            }
        }

        await DeleteRepliesAsync(c.Id);

        _uow.LabComments.Update(c);
        await _uow.SaveChangesAsync();
    }

    private static LabCommentDto ToDto(LabComment e)
    {
        return new LabCommentDto
        {
            Id = e.Id,
            LabId = e.LabId,
            UserId = e.UserId,
            Username = e.User.Username,
            Role = e.User.Role,
            AvatarUrl = e.User.AvatarUrl,
            Content = e.Content,
            ParentId = e.ParentId,
            CreatedAt = e.CreatedAt,
            UpdatedAt = e.UpdatedAt,
            IsActive = e.IsActive,
        };
    }
}
