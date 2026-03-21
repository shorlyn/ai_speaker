using System.Security.Cryptography;
using AiSpeaker.Api.Data;
using AiSpeaker.Api.Data.Entities;
using AiSpeaker.Api.Modules.Device.Dtos;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AiSpeaker.Api.Modules.Device.Services;

public sealed class DeviceService : IDeviceService
{
    private readonly AiSpeakerDbContext _dbContext;
    private readonly ILogger<DeviceService> _logger;

    public DeviceService(AiSpeakerDbContext dbContext, ILogger<DeviceService> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<DeviceRegisterResponse> RegisterAsync(DeviceRegisterRequest request, CancellationToken cancellationToken)
    {
        var device = await _dbContext.Devices.FirstOrDefaultAsync(
            d => d.DeviceCode == request.DeviceCode,
            cancellationToken);

        if (device is null)
        {
            device = new DeviceEntity
            {
                Id = Guid.NewGuid(),
                DeviceCode = request.DeviceCode,
                DeviceName = request.DeviceName,
                SecretKey = GenerateSecretKey(),
                CreatedTime = DateTime.UtcNow,
                LastOnlineTime = DateTime.UtcNow
            };

            _dbContext.Devices.Add(device);
        }
        else
        {
            device.DeviceName = request.DeviceName;
            device.LastOnlineTime = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Device {DeviceCode} registered/updated.", device.DeviceCode);

        return new DeviceRegisterResponse
        {
            DeviceCode = device.DeviceCode,
            DeviceName = device.DeviceName,
            SecretKey = device.SecretKey
        };
    }

    public async Task<DeviceStatusResponse> HeartbeatAsync(DeviceHeartbeatRequest request, CancellationToken cancellationToken)
    {
        var device = await _dbContext.Devices.FirstOrDefaultAsync(
            d => d.DeviceCode == request.DeviceCode,
            cancellationToken);

        if (device is null)
        {
            device = new DeviceEntity
            {
                Id = Guid.NewGuid(),
                DeviceCode = request.DeviceCode,
                DeviceName = "Unnamed",
                SecretKey = GenerateSecretKey(),
                CreatedTime = DateTime.UtcNow,
                LastOnlineTime = DateTime.UtcNow
            };

            _dbContext.Devices.Add(device);
        }
        else
        {
            device.LastOnlineTime = DateTime.UtcNow;
        }

        await _dbContext.SaveChangesAsync(cancellationToken);

        return new DeviceStatusResponse
        {
            DeviceCode = device.DeviceCode,
            LastOnlineTime = device.LastOnlineTime
        };
    }

    public Task<DeviceConfigResponse> GetConfigAsync(string deviceCode, CancellationToken cancellationToken)
    {
        var config = new DeviceConfigResponse
        {
            DeviceCode = deviceCode,
            Asr = "fake-asr",
            Llm = "ollama",
            Tts = "fake-tts"
        };

        return Task.FromResult(config);
    }

    private static string GenerateSecretKey() => Convert.ToBase64String(RandomNumberGenerator.GetBytes(16));
}
