namespace AiSpeaker.Api.Data.Entities;

public sealed class ConversationMessageEntity
{
    public Guid Id { get; set; }
    public Guid ConversationSessionId { get; set; }
    public string Role { get; set; } = null!;
    public string Content { get; set; } = null!;
    public string? AudioUrl { get; set; }
    public int? DurationMs { get; set; }
    public DateTime CreatedTime { get; set; }

    public ConversationSessionEntity? ConversationSession { get; set; }
}
