namespace AiSpeaker.Api.Modules.Provider.Options;

public sealed class OllamaOptions
{
    public string Provider { get; init; } = "Ollama";
    public required string BaseUrl { get; init; }
    public required string Model { get; init; }
    public string? ApiKey { get; init; }
}
