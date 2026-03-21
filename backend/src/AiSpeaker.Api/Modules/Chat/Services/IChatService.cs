using AiSpeaker.Api.Modules.Chat.Dtos;

namespace AiSpeaker.Api.Modules.Chat.Services;

public interface IChatService
{
    Task<ChatResponse> ProcessTextAsync(ChatTextRequest request, CancellationToken cancellationToken);
    Task<ChatResponse> ProcessAudioAsync(string deviceCode, string? sessionId, Stream audioStream, string fileName, CancellationToken cancellationToken);
}
