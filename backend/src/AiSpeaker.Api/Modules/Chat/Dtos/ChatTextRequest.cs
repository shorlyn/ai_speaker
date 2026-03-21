using System.ComponentModel.DataAnnotations;

namespace AiSpeaker.Api.Modules.Chat.Dtos;

public sealed class ChatTextRequest
{
    [Required]
    public required string DeviceCode { get; init; }

    public string? SessionId { get; init; }

    [Required]
    public required string UserText { get; init; }
}
