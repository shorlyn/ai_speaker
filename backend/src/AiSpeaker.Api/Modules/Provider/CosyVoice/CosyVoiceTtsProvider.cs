using System.Text;
using System.Text.Json;
using AiSpeaker.Api.Modules.Provider.Abstractions;
using AiSpeaker.Api.Modules.Provider.Models;
using AiSpeaker.Api.Modules.Provider.Options;
using Microsoft.Extensions.Options;

namespace AiSpeaker.Api.Modules.Provider.CosyVoice;

public sealed class CosyVoiceTtsProvider : ITtsProvider
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    private readonly HttpClient _httpClient;
    private readonly CosyVoiceTtsOptions _options;
    private readonly ILogger<CosyVoiceTtsProvider> _logger;

    public CosyVoiceTtsProvider(
        HttpClient httpClient,
        IOptions<CosyVoiceTtsOptions> options,
        ILogger<CosyVoiceTtsProvider> logger)
    {
        _httpClient = httpClient;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<TtsAudioResult> GenerateAudioAsync(string text, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            throw new ArgumentException("TTS text is required.", nameof(text));
        }

        var normalized = text.Trim();
        var maxLength = Math.Max(1, _options.MaxTextLength);
        if (normalized.Length > maxLength)
        {
            _logger.LogWarning("TTS text exceeded limit ({Length} > {Limit}). Truncating.", normalized.Length, maxLength);
            normalized = normalized[..maxLength];
        }

        var requestBody = new
        {
            model = _options.Model,
            input = normalized,
            voice = _options.Voice,
            format = _options.Format
        };

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, "v1/audio/speech")
        {
            Content = new StringContent(JsonSerializer.Serialize(requestBody, SerializerOptions), Encoding.UTF8, "application/json")
        };

        var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken);

        if (response.IsSuccessStatusCode)
        {
            var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
            var contentType = response.Content.Headers.ContentType?.MediaType ?? "audio/wav";
            return new TtsAudioResult(contentType, bytes);
        }

        var errorContent = await response.Content.ReadAsStringAsync(cancellationToken);
        _logger.LogError(
            "CosyVoice TTS failed with status {Status}. Response: {Body}",
            (int)response.StatusCode,
            string.IsNullOrWhiteSpace(errorContent) ? "<empty>" : errorContent);

        throw new InvalidOperationException($"CosyVoice TTS failed with status {(int)response.StatusCode}.");
    }
}
