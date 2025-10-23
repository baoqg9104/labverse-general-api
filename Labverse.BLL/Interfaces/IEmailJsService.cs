using Labverse.BLL.DTOs.Email;

namespace Labverse.BLL.Interfaces;

public interface IEmailJsService
{
    Task<bool> VerifyTokenAsync(string token);
    Task SendContactUsEmailAsync(ContactUsDto dto);
    Task SendVerificationEmailAsync(int userId, string email);
}
