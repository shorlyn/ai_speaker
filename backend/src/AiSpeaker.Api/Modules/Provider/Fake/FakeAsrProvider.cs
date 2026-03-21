using AiSpeaker.Api.Modules.Provider.Abstractions;

namespace AiSpeaker.Api.Modules.Provider.Fake;

public sealed class FakeAsrProvider : IAsrProvider
{
    public Task<string> TranscribeAsync(Stream audioStream, string fileName, CancellationToken cancellationToken = default)
    {
        var fakeText = $"[fake-asr transcript of {fileName} at {DateTime.UtcNow:O}]";
        return Task.FromResult(fakeText);
    }
}
