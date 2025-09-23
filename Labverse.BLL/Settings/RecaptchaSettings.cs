namespace Labverse.BLL.Settings;

public class RecaptchaSettings
{
    public string SecretKey { get; set; } = string.Empty;
    public bool Bypass { get; set; } = false;
}
