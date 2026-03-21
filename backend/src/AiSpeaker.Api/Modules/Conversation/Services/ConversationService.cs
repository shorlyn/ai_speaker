using System.Linq;
using System.Security.Cryptography;
using AiSpeaker.Api.Data;
using AiSpeaker.Api.Data.Entities;
using AiSpeaker.Api.Modules.Conversation.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiSpeaker.Api.Modules.Conversation.Services;

public sealed class ConversationService : IConversationService
{
    private readonly AiSpeakerDbContext _dbContext;
    private readonly ILogger<ConversationService> _logger;

    public ConversationService(AiSpeakerDbContext dbContext, ILogger<ConversationService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<string> GetOrCreateSessionAsync(string deviceCode, string? sessionId, CancellationToken cancellationToken)
    {
        var normalizedSessionId = sessionId?.Trim();

        if (!string.IsNullOrEmpty(normalizedSessionId))
        {
            var existingSession = await _dbContext.ConversationSessions
                .AsNoTracking()
                .FirstOrDefaultAsync(s => s.SessionId == normalizedSessionId, cancellationToken);

            if (existingSession is not null)
            {
                return existingSession.SessionId;
            }
        }

        var device = await EnsureDeviceAsync(deviceCode, cancellationToken);

        var newSessionId = string.IsNullOrEmpty(normalizedSessionId)
            ? Guid.NewGuid().ToString("N")
            : normalizedSessionId!;

        var session = new ConversationSessionEntity
        {
            Id = Guid.NewGuid(),
            DeviceId = device.Id,
            SessionId = newSessionId,
            StartTime = DateTime.UtcNow,
            LastMessageTime = DateTime.UtcNow
        };

        _dbContext.ConversationSessions.Add(session);
        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Created conversation session {SessionId} for device {DeviceCode}", newSessionId, deviceCode);

        return session.SessionId;
    }

    public async Task AddMessageAsync(string sessionId, string role, string content, string? audioUrl, CancellationToken cancellationToken)
    {
        var session = await _dbContext.ConversationSessions
            .FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);

        if (session is null)
        {
            _logger.LogWarning("Conversation session {SessionId} not found when adding message.", sessionId);
            return;
        }

        session.LastMessageTime = DateTime.UtcNow;

        var entity = new ConversationMessageEntity
        {
            Id = Guid.NewGuid(),
            ConversationSessionId = session.Id,
            Role = role,
            Content = content,
            AudioUrl = audioUrl,
            CreatedTime = DateTime.UtcNow
        };

        _dbContext.ConversationMessages.Add(entity);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ConversationMessage>> GetRecentMessagesAsync(string sessionId, int take, CancellationToken cancellationToken)
    {
        var session = await _dbContext.ConversationSessions
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.SessionId == sessionId, cancellationToken);

        if (session is null)
        {
            return Array.Empty<ConversationMessage>();
        }

        var entities = await _dbContext.ConversationMessages
            .Where(m => m.ConversationSessionId == session.Id)
            .OrderByDescending(m => m.CreatedTime)
            .Take(take)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return entities
            .OrderBy(m => m.CreatedTime)
            .Select(m => new ConversationMessage(m.Role, m.Content, m.AudioUrl))
            .ToList();
    }

    private async Task<DeviceEntity> EnsureDeviceAsync(string deviceCode, CancellationToken cancellationToken)
    {
        var device = await _dbContext.Devices
            .FirstOrDefaultAsync(d => d.DeviceCode == deviceCode, cancellationToken);

        if (device is not null)
        {
            return device;
        }

        device = new DeviceEntity
        {
            Id = Guid.NewGuid(),
            DeviceCode = deviceCode,
            DeviceName = "Unnamed",
            SecretKey = Convert.ToBase64String(RandomNumberGenerator.GetBytes(16)),
            CreatedTime = DateTime.UtcNow,
            LastOnlineTime = DateTime.UtcNow
        };

        _dbContext.Devices.Add(device);
        await _dbContext.SaveChangesAsync(cancellationToken);

        return device;
    }
}
