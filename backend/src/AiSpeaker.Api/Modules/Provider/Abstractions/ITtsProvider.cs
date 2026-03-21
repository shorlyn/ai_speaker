namespace AiSpeaker.Api.Modules.Provider.Abstractions;

public interface ITtsProvider
{
    Task<string> GenerateAudioAsync(string text, CancellationToken cancellationToken = default);
}
