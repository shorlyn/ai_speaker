using AiSpeaker.Api.Modules.Provider.Models;

namespace AiSpeaker.Api.Modules.Provider.Abstractions;

public interface ITtsProvider
{
    Task<TtsAudioResult> GenerateAudioAsync(string text, CancellationToken cancellationToken = default);
}
