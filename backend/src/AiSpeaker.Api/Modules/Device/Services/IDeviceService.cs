using AiSpeaker.Api.Modules.Device.Dtos;

namespace AiSpeaker.Api.Modules.Device.Services;

public interface IDeviceService
{
    Task<DeviceRegisterResponse> RegisterAsync(DeviceRegisterRequest request, CancellationToken cancellationToken);
    Task<DeviceStatusResponse> HeartbeatAsync(DeviceHeartbeatRequest request, CancellationToken cancellationToken);
    Task<DeviceConfigResponse> GetConfigAsync(string deviceCode, CancellationToken cancellationToken);
}
