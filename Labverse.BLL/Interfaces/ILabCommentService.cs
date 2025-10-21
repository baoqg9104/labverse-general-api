using Labverse.BLL.DTOs.Labs;
using Labverse.BLL.DTOs.Paging;

namespace Labverse.BLL.Interfaces;

public interface ILabCommentService
{
    Task AddCommentAsync(int labId, int userId, string content, int? parentId = null);
    Task<PagedResult<LabCommentDto>> GetCommentsAsync(int labId, int page = 1, int pageSize = 20);

    //Task<PagedResult<LabCommentTreeDto>> GetCommentTreeAsync(
    //    int labId,
    //    int page = 1,
    //    int pageSize = 20
    //);
    Task EditCommentAsync(int commentId, int userId, string content);
    Task DeleteCommentAsync(int commentId, int userId);
}
