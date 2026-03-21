using AiSpeaker.Api.Modules.Provider.Abstractions;

namespace AiSpeaker.Api.Modules.Provider.Fake;

public sealed class FakeTtsProvider : ITtsProvider
{
    public Task<string> GenerateAudioAsync(string text, CancellationToken cancellationToken = default)
    {
        var fakeUrl = $"https://example.com/audio/{Guid.NewGuid():N}.wav";
        return Task.FromResult(fakeUrl);
    }
}
