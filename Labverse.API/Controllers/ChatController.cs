using Labverse.API.Helpers;
using Labverse.BLL.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Labverse.API.Controllers;

public record ChatRequest(string Message);

public record ChatResponse(string Reply);

[Route("api/chat")]
[ApiController]
[Authorize]
public class ChatController : ControllerBase
{
    private readonly IChatService _chat;

    public ChatController(IChatService chat)
    {
        _chat = chat;
    }

    [HttpPost]
    public async Task<IActionResult> Post([FromBody] ChatRequest req, CancellationToken ct)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(req.Message))
                return ApiErrorHelper.Error("BAD_REQUEST", "message is required", 400);
            var reply = await _chat.AskAsync(req.Message, ct);
            return Ok(new ChatResponse(reply));
        }
        catch (Exception ex)
        {
            return ApiErrorHelper.Error("CHAT_ERROR", ex.Message, 500);
        }
    }
}
