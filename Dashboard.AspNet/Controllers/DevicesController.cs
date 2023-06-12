using BrewHub.Dashboard.Core.Dtmi;
using BrewHub.Dashboard.Core.Providers;
using BrewHub.Dashboard.Core.Display;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using Microsoft.AspNetCore.Cors;

namespace BrewHub.Dashboard.Controllers;

public enum TimeframeEnum { Minutes = 0, Hour, Hours, Day, Week, Month };

/// <summary>
/// Read and modify device metrics
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DevicesController : ControllerBase
{
    private readonly IDataSource _datasource;
    private readonly ILogger<DevicesController> _logger;

    public DevicesController(ILogger<DevicesController> logger, IDataSource datasource)
    {
        _logger = logger;
        _datasource = datasource;
    }

    /// <summary>
    /// Get list of all device names
    /// </summary>
    /// <returns>Device names</returns>
    [HttpGet]
    [Route("")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    public async Task<ActionResult> Devices()
    {
        _logger.LogInformation("Devices");

        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();
        var devices = data.Select(x => x.__Device).Distinct();

        return Ok(devices);
    }

    /// <summary>
    /// Get summary slabs for all devices
    /// </summary>
    /// <returns>Summaries</returns>
    [HttpGet]
    [Route("[action]")]
    [ProducesResponseType(typeof(DisplayMetricGroup[]),StatusCodes.Status200OK)]
    public async Task<ActionResult> Slabs()
    {
        _logger.LogInformation("Slabs");

        // Pull raw telemetry from database
        // TODO: Need to get all telemetry in this call
        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();
        var dtmi = new DeviceModelDetails();

        var slabs = data
                .GroupBy(x => x.__Device)
                .Select(dtmi!.FromDeviceComponentTelemetry)
                .ToArray();

        return Ok(slabs);
    }

    /// <summary>
    /// Get details for a single device
    /// </summary>
    /// <param name="device">Which device</param>
    /// <returns>Detail slabs, one for each component</returns>
    [HttpGet]
    [Route("{device}")]
    [ProducesResponseType(typeof(DisplayMetricGroup[]),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Device([FromRoute] string device)
    {
        _logger.LogInformation("Device: {device}", device);

        if (!(await DoesDeviceExistAsync(device)))
            return NotFound();

        // Query InfluxDB, compose into UI slabs
        var data = await _datasource.GetLatestDevicePropertiesAsync(device);
        var dtmi = new DeviceModelDetails();
        var slabs = data.GroupBy(x => x.__Component ?? string.Empty).Select(dtmi!.FromComponent).ToArray();

        return Ok(slabs);
    }

    /// <summary>
    /// Get details for a single component
    /// </summary>
    /// <param name="device">Which device</param>
    /// <param name="component">Which component, or "device" for device details</param>
    /// <returns>Detail slabs, one for each kind of metric</returns>
    [HttpGet]
    [Route("{device}/Component/{component}")]
    [ProducesResponseType(typeof(DisplayMetricGroup[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Component ([FromRoute] string device, [FromRoute] string component )
    {
        _logger.LogInformation("Component: Device {device} Component {component}", device, component);

        if (!(await DoesDeviceAndComponentExistAsync(device,component)))
            return NotFound();

        // Query InfluxDB, compose into UI slabs
        var data = await _datasource.GetLatestDevicePropertiesAsync(device);
        var dtmi = new DeviceModelDetails();
        var metrics = data.Where(x => component == (x.__Component ?? "device"));
        var slabs = dtmi!.FromSingleComponent(metrics);

        return Ok(slabs);
    }

    /// <summary>
    /// Execute command on device
    /// </summary>
    /// <param name="payload">Parameters to send to command</param>
    /// <param name="device">Name of device</param>
    /// <param name="component">Name of component, or "device"</param>
    /// <param name="command">Name of command</param>
    [HttpPost]
    [Route("{device}/Component/{component}/Command/{command}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExecuteCommand([FromBody] object payload, [FromRoute] string device, [FromRoute] string component, [FromRoute] string command)
    {
        _logger.LogInformation("ExecuteCommand: Device {device} Component {component} payload {payload}", device, component, payload);

        if (!(await DoesDeviceAndComponentExistAsync(device,component)))
            return NotFound();

        // Bad request if fails DTMI, e.g. wrong command or payload
        var dtmi = new DeviceModelDetails();
        if (!dtmi.GetCommands(component).Any(x=>x.Id == command))
        {
            _logger.LogError("{caller}: {status} Not known command {command}","ExecuteCommand",StatusCodes.Status400BadRequest,command);
            return BadRequest();
        }

        return NoContent();
    }

    /// <summary>
    /// Set property on device
    /// </summary>
    /// <param name="payload">Value to set on property</param>
    /// <param name="device">Name of device</param>
    /// <param name="component">Name of component, or "device"</param>
    /// <param name="property">Name of property</param>
    [HttpPost]
    [Route("{device}/Component/{component}/Property/{property}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetProperty([FromBody] object payload, [FromRoute] string device, [FromRoute] string component, [FromRoute] string property)
    {
        _logger.LogInformation("SetProperty: Device {device} Component {component} payload {payload}", device, component, payload);

        if (!(await DoesDeviceAndComponentExistAsync(device,component)))
            return NotFound();

        // Bad request if fails DTMI, e.g. wrong command or payload
        var dtmi = new DeviceModelDetails();
        if (!dtmi.IsMetricWritable(property))
        {
            _logger.LogError("{caller}: {status} Not a writable property {property}","SetProperty",StatusCodes.Status400BadRequest,property);
            return BadRequest();
        }

        return NoContent();
    }

    private async Task<bool> DoesDeviceExistAsync(string device, [CallerMemberName] string caller = "")
    {
        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();
        var devices = data.Select(x => x.__Device).Distinct();
        var result = devices.Contains(device);

        if (!result)
            _logger.LogError("{caller}: {status} Unknown device {device}",caller,StatusCodes.Status404NotFound,device);

        return result;
    }

    private async Task<bool> DoesDeviceAndComponentExistAsync(string device, string component, [CallerMemberName] string caller = "")
    {
        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();
        var devices = data.Select(x => x.__Device).Distinct();
        var result = devices.Contains(device);

        if (result)
        {
            var components = data.Where(x => x.__Device == device).Select(x=>x.__Component ?? "device").Distinct();
            result = components.Contains(component);
            if (!result)
                _logger.LogError("{caller}: {status} Unknown component {component}",caller,StatusCodes.Status404NotFound,component);
        }
        else
        {
            _logger.LogError("{caller}: {status} Unknown device {device}",caller,StatusCodes.Status404NotFound,device);
        }

        return result;
    }

}
