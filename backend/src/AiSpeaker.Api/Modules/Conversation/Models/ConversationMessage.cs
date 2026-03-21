namespace AiSpeaker.Api.Modules.Conversation.Models;

public sealed class ConversationMessage
{
    public ConversationMessage(string role, string content, string? audioUrl)
    {
        Role = role;
        Content = content;
        AudioUrl = audioUrl;
        CreatedTime = DateTime.UtcNow;
    }

    public string Role { get; }
    public string Content { get; }
    public string? AudioUrl { get; }
    public DateTime CreatedTime { get; }
}
