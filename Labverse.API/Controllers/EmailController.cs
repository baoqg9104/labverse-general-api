using Labverse.API.Helpers;
using Labverse.BLL.DTOs.Email;
using Labverse.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Labverse.API.Controllers;

[Route("api/email")]
[ApiController]
public class EmailController : ControllerBase
{
    private readonly IEmailService _emailService;
    private readonly IConfiguration _configuration;

    public EmailController(IEmailService emailService, IConfiguration configuration)
    {
        _emailService = emailService;
        _configuration = configuration;
    }

    [HttpPost("contact-us")]
    public async Task<IActionResult> ContactUs([FromBody] ContactUsDto dto)
    {
        try
        {
            var to = _configuration["Contact:ToEmail"] ?? "baoqg9104@gmail.com";
            var subject = $"Contact Us - {dto.Name} ({dto.Email})";
            var body =
                $@"<b>Name:</b> {dto.Name}<br/><b>Email:</b> {dto.Email}<br/><b>Message:</b><br/>{dto.Message}";
            await _emailService.SendEmailAsync(to, subject, body);
            return Ok(new { message = "Your message has been sent successfully." });
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("CONTACT_US_ERROR", ex.Message, 500);
        }
    }
}
