namespace AiSpeaker.Api.Modules.Chat.Dtos;

public sealed class ChatResponse
{
    public required string SessionId { get; init; }
    public required string UserText { get; init; }
    public required string AssistantText { get; init; }
    public required string AudioUrl { get; init; }
}
