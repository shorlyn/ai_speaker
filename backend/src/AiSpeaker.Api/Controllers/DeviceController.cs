using AiSpeaker.Api.Modules.Device.Dtos;
using AiSpeaker.Api.Modules.Device.Services;
using Microsoft.AspNetCore.Mvc;

namespace AiSpeaker.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public sealed class DeviceController : ControllerBase
{
    private readonly IDeviceService _deviceService;

    public DeviceController(IDeviceService deviceService)
    {
        _deviceService = deviceService;
    }

    [HttpPost("register")]
    public async Task<ActionResult<DeviceRegisterResponse>> RegisterAsync([FromBody] DeviceRegisterRequest request, CancellationToken cancellationToken)
    {
        var response = await _deviceService.RegisterAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpPost("heartbeat")]
    public async Task<ActionResult<DeviceStatusResponse>> HeartbeatAsync([FromBody] DeviceHeartbeatRequest request, CancellationToken cancellationToken)
    {
        var response = await _deviceService.HeartbeatAsync(request, cancellationToken);
        return Ok(response);
    }

    [HttpGet("config")]
    public async Task<ActionResult<DeviceConfigResponse>> GetConfigAsync([FromQuery] string deviceCode, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(deviceCode))
        {
            return BadRequest("deviceCode is required");
        }

        var response = await _deviceService.GetConfigAsync(deviceCode, cancellationToken);
        return Ok(response);
    }
}
