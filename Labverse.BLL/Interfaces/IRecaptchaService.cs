namespace Labverse.BLL.Interfaces;

public interface IRecaptchaService
{
    Task<bool> VerifyTokenAsync(string token);
}
