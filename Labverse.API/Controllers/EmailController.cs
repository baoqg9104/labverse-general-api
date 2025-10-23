using Labverse.API.Helpers;
using Labverse.BLL.DTOs.Email;
using Labverse.BLL.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Labverse.API.Controllers;

[Route("api/email")]
[ApiController]
public class EmailController : ControllerBase
{
    private readonly IEmailJsService _emailJsService;

    public EmailController(IEmailJsService emailJsService)
    {
        _emailJsService = emailJsService;
    }

    [HttpPost("contact-us")]
    public async Task<IActionResult> ContactUs([FromBody] ContactUsDto dto)
    {
        try
        {
            await _emailJsService.SendContactUsEmailAsync(dto);
            return Ok(new { message = "Your message has been sent successfully." });
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("CONTACT_US_ERROR", ex.Message, 500);
        }
    }
}
