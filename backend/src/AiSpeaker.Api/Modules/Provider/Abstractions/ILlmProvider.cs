namespace AiSpeaker.Api.Modules.Provider.Abstractions;

public interface ILlmProvider
{
    Task<string> ChatAsync(IEnumerable<(string role, string content)> messages, CancellationToken cancellationToken = default);
}
