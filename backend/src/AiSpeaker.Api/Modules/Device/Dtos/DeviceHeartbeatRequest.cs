using System.ComponentModel.DataAnnotations;

namespace AiSpeaker.Api.Modules.Device.Dtos;

public sealed class DeviceHeartbeatRequest
{
    [Required]
    public required string DeviceCode { get; init; }
}
