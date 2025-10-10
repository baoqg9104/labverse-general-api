namespace Labverse.BLL.DTOs.Revenue;

public class RevenueSummaryDto
{
    public DateTime From { get; set; }
    public DateTime To { get; set; }
    public decimal TotalRevenue { get; set; }
    public int Transactions { get; set; }
    public string Currency { get; set; } = "VND";
}
