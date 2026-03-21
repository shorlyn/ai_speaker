namespace AiSpeaker.Api.Modules.Provider.Abstractions;

public interface IAsrProvider
{
    Task<string> TranscribeAsync(Stream audioStream, string fileName, CancellationToken cancellationToken = default);
}
