namespace AiSpeaker.Api.Modules.Device.Dtos;

public sealed class DeviceConfigResponse
{
    public required string DeviceCode { get; init; }
    public required string Asr { get; init; }
    public required string Llm { get; init; }
    public required string Tts { get; init; }
}
