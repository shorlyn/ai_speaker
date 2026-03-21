using System.ComponentModel.DataAnnotations;

namespace AiSpeaker.Api.Modules.Device.Dtos;

public sealed class DeviceRegisterRequest
{
    [Required]
    public required string DeviceCode { get; init; }

    [Required]
    public required string DeviceName { get; init; }
}
