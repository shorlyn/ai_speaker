namespace AiSpeaker.Api.Modules.Device.Dtos;

public sealed class DeviceStatusResponse
{
    public required string DeviceCode { get; init; }
    public required DateTime LastOnlineTime { get; init; }
}
