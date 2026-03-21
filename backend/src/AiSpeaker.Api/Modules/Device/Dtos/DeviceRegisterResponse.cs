namespace AiSpeaker.Api.Modules.Device.Dtos;

public sealed class DeviceRegisterResponse
{
    public required string DeviceCode { get; init; }
    public required string DeviceName { get; init; }
    public required string SecretKey { get; init; }
}
