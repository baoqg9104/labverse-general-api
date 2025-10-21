using Labverse.BLL.DTOs.Activities;
using Labverse.BLL.DTOs.Paging;

namespace Labverse.BLL.Interfaces;

public interface IActivityQueryService
{
    Task<PagedResult<ActivityDto>> ListAsync(ActivityListQuery query);
}
