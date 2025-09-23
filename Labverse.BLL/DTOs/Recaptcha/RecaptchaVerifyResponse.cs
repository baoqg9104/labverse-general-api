namespace Labverse.BLL.DTOs.Recaptcha;

public class RecaptchaVerifyResponse
{
    public bool Success { get; set; }
    public double Score { get; set; }
    public string Action { get; set; } = string.Empty;
    public DateTime Challenge_ts { get; set; }
    public string Hostname { get; set; } = string.Empty;
    public List<string> ErrorCodes { get; set; } = new();
}
