using AiSpeaker.Api.Modules.Provider.Abstractions;
using AiSpeaker.Api.Modules.Tts.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace AiSpeaker.Api.Controllers;

[ApiController]
[Route("api/tts")]
public sealed class TtsController : ControllerBase
{
    private readonly ITtsProvider _ttsProvider;
    private readonly ILogger<TtsController> _logger;

    public TtsController(ITtsProvider ttsProvider, ILogger<TtsController> logger)
    {
        _ttsProvider = ttsProvider;
        _logger = logger;
    }

    [HttpPost("speak")]
    public async Task<IActionResult> SpeakAsync([FromBody] TtsSpeakRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Text))
        {
            return BadRequest(new { error = "text is required" });
        }

        try
        {
            var result = await _ttsProvider.GenerateAudioAsync(request.Text, cancellationToken);
            var contentType = string.IsNullOrWhiteSpace(result.ContentType) ? "audio/wav" : result.ContentType;
            return File(result.AudioBytes, contentType, enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to generate TTS audio.");
            return StatusCode(StatusCodes.Status503ServiceUnavailable, new { error = "[tts-error]" });
        }
    }
}
