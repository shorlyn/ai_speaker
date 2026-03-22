namespace AiSpeaker.Api.Modules.Provider.Models;

public sealed class TtsAudioResult
{
    public TtsAudioResult(string contentType, byte[] audioBytes)
    {
        ContentType = contentType;
        AudioBytes = audioBytes;
    }

    public string ContentType { get; }
    public byte[] AudioBytes { get; }
}
