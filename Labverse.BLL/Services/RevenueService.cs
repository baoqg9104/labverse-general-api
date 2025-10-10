using Labverse.BLL.DTOs.Revenue;
using Labverse.BLL.Interfaces;
using Labverse.DAL.UnitOfWork;
using Microsoft.EntityFrameworkCore;

namespace Labverse.BLL.Services;

public class RevenueService : IRevenueService
{
    private readonly IUnitOfWork _unitOfWork;

    public RevenueService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<RevenueSummaryDto> GetRevenueAsync(DateTime? from, DateTime? to)
    {
        DateTime? start = from?.ToUniversalTime();
        DateTime? end = to?.ToUniversalTime();

        if (start.HasValue && end.HasValue && end.Value < start.Value)
        {
            (start, end) = (end, start);
        }

        // Build base query of user subscriptions joined with subscription prices
        var query = _unitOfWork
            .UserSubscriptions.Query()
            .Join(
                _unitOfWork.Subscriptions.Query(),
                us => us.SubscriptionId,
                s => s.Id,
                (us, s) => new { us.StartDate, s.Price }
            )
            .AsQueryable();

        if (start.HasValue)
        {
            query = query.Where(x => x.StartDate >= start.Value);
        }
        if (end.HasValue)
        {
            query = query.Where(x => x.StartDate <= end.Value);
        }

        var list = await query.ToListAsync();
        var total = list.Sum(x => x.Price);

        var actualFrom = start ?? DateTime.MinValue.ToUniversalTime();
        var actualTo = end ?? DateTime.UtcNow;

        return new RevenueSummaryDto
        {
            From = actualFrom,
            To = actualTo,
            TotalRevenue = total,
            Transactions = list.Count,
            Currency = "VND",
        };
    }
}
