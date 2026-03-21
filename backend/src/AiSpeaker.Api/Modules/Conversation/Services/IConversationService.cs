using AiSpeaker.Api.Modules.Conversation.Models;

namespace AiSpeaker.Api.Modules.Conversation.Services;

public interface IConversationService
{
    Task<string> GetOrCreateSessionAsync(string deviceCode, string? sessionId, CancellationToken cancellationToken);
    Task AddMessageAsync(string sessionId, string role, string content, string? audioUrl, CancellationToken cancellationToken);
    Task<IReadOnlyList<ConversationMessage>> GetRecentMessagesAsync(string sessionId, int take, CancellationToken cancellationToken);
}
