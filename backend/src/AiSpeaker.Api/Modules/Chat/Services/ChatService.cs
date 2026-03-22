using AiSpeaker.Api.Modules.Chat.Dtos;
using AiSpeaker.Api.Modules.Conversation.Models;
using AiSpeaker.Api.Modules.Conversation.Services;
using AiSpeaker.Api.Modules.Provider.Abstractions;

namespace AiSpeaker.Api.Modules.Chat.Services;

public sealed class ChatService : IChatService
{
    private readonly IAsrProvider _asrProvider;
    private readonly ILlmProvider _llmProvider;
    private readonly IConversationService _conversationService;
    private readonly ILogger<ChatService> _logger;

    public ChatService(
        IAsrProvider asrProvider,
        ILlmProvider llmProvider,
        IConversationService conversationService,
        ILogger<ChatService> logger)
    {
        _asrProvider = asrProvider;
        _llmProvider = llmProvider;
        _conversationService = conversationService;
        _logger = logger;
    }

    public async Task<ChatResponse> ProcessTextAsync(ChatTextRequest request, CancellationToken cancellationToken)
    {
        var sessionId = await _conversationService.GetOrCreateSessionAsync(request.DeviceCode, request.SessionId, cancellationToken);
        await _conversationService.AddMessageAsync(sessionId, ConversationRoles.User, request.UserText, null, cancellationToken);

        var history = await _conversationService.GetRecentMessagesAsync(sessionId, 10, cancellationToken);
        var assistantText = await _llmProvider.ChatAsync(history.Select(m => (m.Role, m.Content)), cancellationToken);

        await _conversationService.AddMessageAsync(sessionId, ConversationRoles.Assistant, assistantText, null, cancellationToken);

        _logger.LogInformation("Processed text chat for device {DeviceCode} session {SessionId}", request.DeviceCode, sessionId);

        return new ChatResponse
        {
            SessionId = sessionId,
            UserText = request.UserText,
            AssistantText = assistantText
        };
    }

    public async Task<ChatResponse> ProcessAudioAsync(string deviceCode, string? sessionId, Stream audioStream, string fileName, CancellationToken cancellationToken)
    {
        var transcript = await _asrProvider.TranscribeAsync(audioStream, fileName, cancellationToken);
        _logger.LogInformation("Fake ASR transcript for device {DeviceCode}: {Transcript}", deviceCode, transcript);

        var textRequest = new ChatTextRequest
        {
            DeviceCode = deviceCode,
            SessionId = sessionId,
            UserText = transcript
        };

        return await ProcessTextAsync(textRequest, cancellationToken);
    }
}
