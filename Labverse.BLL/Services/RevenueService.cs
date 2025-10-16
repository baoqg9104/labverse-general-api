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

    public async Task<List<DailyRevenuePointDto>> GetRevenueDailyAsync(DateTime? from, DateTime? to)
    {
        DateTime? start = from?.ToUniversalTime();
        DateTime? end = to?.ToUniversalTime();
        if (start.HasValue && end.HasValue && end < start)
            (start, end) = (end, start);

        // base: user subscriptions joined with subscription prices
        var baseQuery =
            from us in _unitOfWork.UserSubscriptions.Query()
            join s in _unitOfWork.Subscriptions.Query() on us.SubscriptionId equals s.Id
            select new { us.StartDate, s.Price };

        if (start.HasValue)
            baseQuery = baseQuery.Where(x => x.StartDate >= start.Value);
        if (end.HasValue)
            baseQuery = baseQuery.Where(x => x.StartDate <= end.Value);

        // group by day (UTC)
        var grouped = await baseQuery
            .GroupBy(x => x.StartDate.Date)
            .Select(g => new DailyRevenuePointDto
            {
                Date = g.Key,
                TotalRevenue = g.Sum(x => x.Price),
                Transactions = g.Count(),
            })
            .OrderBy(p => p.Date)
            .ToListAsync();

        // fill missing days with zeros if both bounds provided
        if (start.HasValue && end.HasValue)
        {
            var map = grouped.ToDictionary(p => p.Date.Date);
            var filled = new List<DailyRevenuePointDto>();
            for (var d = start.Value.Date; d <= end.Value.Date; d = d.AddDays(1))
            {
                if (map.TryGetValue(d, out var v))
                    filled.Add(v);
                else
                    filled.Add(
                        new DailyRevenuePointDto
                        {
                            Date = d,
                            TotalRevenue = 0,
                            Transactions = 0,
                        }
                    );
            }
            return filled;
        }

        return grouped;
    }
}
