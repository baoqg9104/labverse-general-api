using Labverse.BLL.DTOs.Revenue;

namespace Labverse.BLL.Interfaces;

public interface IRevenueService
{
    Task<RevenueSummaryDto> GetRevenueAsync(DateTime? from, DateTime? to);
}
