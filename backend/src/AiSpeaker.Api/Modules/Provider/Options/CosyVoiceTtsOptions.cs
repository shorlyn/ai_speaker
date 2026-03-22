namespace AiSpeaker.Api.Modules.Provider.Options;

public sealed class CosyVoiceTtsOptions
{
    public string Provider { get; set; } = "Fake";
    public string? BaseUrl { get; set; }
    public string? ApiKey { get; set; }
    public string Model { get; set; } = "cosyvoice-1.0";
    public string Voice { get; set; } = "cn_female_001";
    public string Format { get; set; } = "wav";
    public int TimeoutSeconds { get; set; } = 15;
    public int MaxTextLength { get; set; } = 400;
}
