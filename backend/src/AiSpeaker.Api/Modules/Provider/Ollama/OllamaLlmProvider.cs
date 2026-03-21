using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using AiSpeaker.Api.Modules.Provider.Abstractions;
using AiSpeaker.Api.Modules.Provider.Options;
using Microsoft.Extensions.Options;

namespace AiSpeaker.Api.Modules.Provider.Ollama;

public sealed class OllamaLlmProvider : ILlmProvider
{
    private const string SystemInstruction =
        "你是家庭语音助手。请用简洁、自然、口语化的中文回答。不要使用 markdown、标题、加粗、列表符号、代码块、表情符号。回答尽量控制在 2 到 4 句内，内容要适合直接语音播报。";

    private readonly HttpClient _httpClient;
    private readonly OllamaOptions _options;
    private readonly ILogger<OllamaLlmProvider> _logger;

    public OllamaLlmProvider(HttpClient httpClient, IOptions<OllamaOptions> options, ILogger<OllamaLlmProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<string> ChatAsync(IEnumerable<(string role, string content)> messages, CancellationToken cancellationToken = default)
    {
        var prompt = BuildPrompt(messages);
        var payload = new GenerateRequest
        {
            Model = _options.Model,
            Prompt = prompt
        };

        var json = JsonSerializer.Serialize(payload, SerializerOptions);
        using var request = new StringContent(json, Encoding.UTF8, "application/json");

        try
        {
            using var response = await _httpClient.PostAsync("api/generate", request, cancellationToken);
            if (!response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsStringAsync(cancellationToken);
                _logger.LogWarning("LLM request failed with status {Status}: {Body}", response.StatusCode, body);
                return "[llm-error]";
            }

            var assistantText = await ParseResponseAsync(response, cancellationToken);
            return string.IsNullOrWhiteSpace(assistantText) ? "[llm-error]" : assistantText!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "LLM chat call failed, returning fallback message");
            return "[llm-error]";
        }
    }

    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private static string BuildPrompt(IEnumerable<(string role, string content)> messages)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"System: {SystemInstruction}");
        sb.AppendLine();

        foreach (var (role, content) in messages)
        {
            var label = role.ToLowerInvariant() switch
            {
                "system" => "System",
                "assistant" => "Assistant",
                "user" => "User",
                _ => "Message"
            };
            sb.AppendLine($"{label}: {content}");
            sb.AppendLine();
        }
        return sb.ToString().Trim();
    }

    private static async Task<string?> ParseResponseAsync(HttpResponseMessage response, CancellationToken cancellationToken)
    {
        var body = await response.Content.ReadAsStringAsync(cancellationToken);
        if (string.IsNullOrWhiteSpace(body))
        {
            return null;
        }

        var builder = new StringBuilder();
        using var reader = new StringReader(body);
        string? line;
        var parsedAny = false;
        while ((line = reader.ReadLine()) != null)
        {
            line = line.Trim();
            if (line.Length == 0)
            {
                continue;
            }

            if (TryExtractResponse(line, out var chunk))
            {
                parsedAny = true;
                builder.Append(chunk);
            }
        }

        if (parsedAny)
        {
            return builder.ToString();
        }

        // Fallback: treat entire body as single JSON payload.
        if (TryExtractResponse(body, out var single))
        {
            return single;
        }

        return null;
    }

    private static bool TryExtractResponse(string json, out string? response)
    {
        try
        {
            var obj = JsonSerializer.Deserialize<GenerateResponse>(json, SerializerOptions);
            if (!string.IsNullOrWhiteSpace(obj?.Response))
            {
                response = obj!.Response;
                return true;
            }
        }
        catch
        {
            // ignore parse errors for streaming chunks
        }

        response = null;
        return false;
    }

    private sealed record GenerateRequest
    {
        public required string Model { get; init; }
        public required string Prompt { get; init; }
    }

    private sealed record GenerateResponse
    {
        public string? Response { get; init; }
    }
}
