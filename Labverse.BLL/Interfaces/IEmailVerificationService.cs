public interface IEmailVerificationService
{
    Task<string> GenerateAndSaveTokenAsync(int userId);
    Task<bool> VerifyTokenAsync(string token);
}
