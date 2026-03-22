using System.IO;
using AiSpeaker.Api.Modules.Provider.Abstractions;
using AiSpeaker.Api.Modules.Provider.Models;

namespace AiSpeaker.Api.Modules.Provider.Fake;

public sealed class FakeTtsProvider : ITtsProvider
{
    public Task<TtsAudioResult> GenerateAudioAsync(string text, CancellationToken cancellationToken = default)
    {
        var normalizedText = string.IsNullOrWhiteSpace(text)
            ? "你好，欢迎使用 AI Speaker"
            : text.Trim();

        var audioBytes = GenerateTone(normalizedText);
        return Task.FromResult(new TtsAudioResult("audio/wav", audioBytes));
    }

    private static byte[] GenerateTone(string text)
    {
        const int sampleRate = 16_000;
        const short bitsPerSample = 16;
        const short channels = 1;

        var durationSeconds = Math.Clamp(text.Length / 6.0, 0.7, 2.5);
        var sampleCount = (int)(sampleRate * durationSeconds);
        var data = new byte[sampleCount * 2];
        var frequency = 320 + (text.Length % 280);

        for (var i = 0; i < sampleCount; i++)
        {
            var amplitude = Math.Sin(2 * Math.PI * frequency * i / sampleRate) * short.MaxValue * 0.15;
            var sample = (short)amplitude;
            data[2 * i] = (byte)(sample & 0xFF);
            data[2 * i + 1] = (byte)((sample >> 8) & 0xFF);
        }

        using var ms = new MemoryStream();
        using var writer = new BinaryWriter(ms, System.Text.Encoding.ASCII, leaveOpen: true);

        writer.Write("RIFF"u8.ToArray());
        writer.Write(36 + data.Length);
        writer.Write("WAVE"u8.ToArray());
        writer.Write("fmt "u8.ToArray());
        writer.Write(16);
        writer.Write((short)1);
        writer.Write(channels);
        writer.Write(sampleRate);
        writer.Write(sampleRate * channels * bitsPerSample / 8);
        writer.Write((short)(channels * bitsPerSample / 8));
        writer.Write(bitsPerSample);
        writer.Write("data"u8.ToArray());
        writer.Write(data.Length);
        writer.Write(data);

        writer.Flush();
        return ms.ToArray();
    }
}
