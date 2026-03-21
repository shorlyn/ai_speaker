using System.Linq;
using System.Text;
using AiSpeaker.Api.Modules.Provider.Abstractions;

namespace AiSpeaker.Api.Modules.Provider.Fake;

public sealed class FakeLlmProvider : ILlmProvider
{
    public Task<string> ChatAsync(IEnumerable<(string role, string content)> messages, CancellationToken cancellationToken = default)
    {
        var sb = new StringBuilder();
        sb.Append("[fake-llm reply] ");
        var lastUserMessage = messages.LastOrDefault(m => string.Equals(m.role, "user", StringComparison.OrdinalIgnoreCase)).content;
        if (!string.IsNullOrWhiteSpace(lastUserMessage))
        {
            sb.Append("You said: ");
            sb.Append(lastUserMessage);
        }
        else
        {
            sb.Append("No user input detected.");
        }

        return Task.FromResult(sb.ToString());
    }
}
