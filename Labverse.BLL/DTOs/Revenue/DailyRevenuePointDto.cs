namespace Labverse.BLL.DTOs.Revenue;

public class DailyRevenuePointDto
{
    public DateTime Date { get; set; }
    public decimal TotalRevenue { get; set; }
    public int Transactions { get; set; }
}
