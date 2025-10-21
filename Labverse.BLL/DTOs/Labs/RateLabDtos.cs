namespace Labverse.BLL.DTOs.Labs;

public class RateLabRequest
{
    public int Score { get; set; } // 1..5
    public string? Comment { get; set; }
}

public class RateLabResponse
{
    public double RatingAverage { get; set; }
    public int RatingCount { get; set; }
}
