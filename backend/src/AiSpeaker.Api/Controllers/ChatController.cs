using AiSpeaker.Api.Modules.Chat.Dtos;
using AiSpeaker.Api.Modules.Chat.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiSpeaker.Api.Controllers;

[ApiController]
[Route("api/chat")]
public sealed class ChatController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatController(IChatService chatService)
    {
        _chatService = chatService;
    }

    [HttpPost("text")]
    public async Task<ActionResult<ChatResponse>> TextAsync([FromBody] ChatTextRequest request, CancellationToken cancellationToken)
    {
        var response = await _chatService.ProcessTextAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("audio")]
    [RequestSizeLimit(10 * 1024 * 1024)]
    public async Task<ActionResult<ChatResponse>> AudioAsync([FromForm] string deviceCode, [FromForm] string? sessionId, [FromForm] IFormFile file, CancellationToken cancellationToken)
    {
        if (file.Length == 0)
        {
            return BadRequest("Audio file is required.");
        }

        await using var stream = file.OpenReadStream();
        var response = await _chatService.ProcessAudioAsync(deviceCode, sessionId, stream, file.FileName, cancellationToken);
        return Ok(response);
    }
}
