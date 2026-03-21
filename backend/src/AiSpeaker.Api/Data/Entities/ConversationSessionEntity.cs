namespace AiSpeaker.Api.Data.Entities;

public sealed class ConversationSessionEntity
{
    public Guid Id { get; set; }
    public Guid DeviceId { get; set; }
    public string SessionId { get; set; } = null!;
    public DateTime StartTime { get; set; }
    public DateTime LastMessageTime { get; set; }

    public DeviceEntity? Device { get; set; }
    public ICollection<ConversationMessageEntity> Messages { get; set; } = new List<ConversationMessageEntity>();
}
