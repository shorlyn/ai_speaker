namespace AiSpeaker.Api.Data.Entities;

public sealed class DeviceEntity
{
    public Guid Id { get; set; }
    public string DeviceCode { get; set; } = null!;
    public string DeviceName { get; set; } = null!;
    public string SecretKey { get; set; } = null!;
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedTime { get; set; }
    public DateTime LastOnlineTime { get; set; }

    public ICollection<ConversationSessionEntity> Sessions { get; set; } = new List<ConversationSessionEntity>();
}
