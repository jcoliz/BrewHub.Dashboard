using BrewHub.Dashboard.Core.Dtmi;
using BrewHub.Dashboard.Core.Providers;
using BrewHub.Dashboard.Core.Display;
using Microsoft.AspNetCore.Mvc;

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

        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();
        var devices = data.Select(x => x.__Device).Distinct();

        if (!devices.Contains(device))
        {
            _logger.LogError("Device: {status} Unknown device {device}",StatusCodes.Status404NotFound,device);
            return NotFound();
        }

        // Query InfluxDB, compose into UI slabs
        data = await _datasource.GetLatestDevicePropertiesAsync(device);
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

        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();
        var devices = data.Select(x => x.__Device).Distinct();
        if (!devices.Contains(device))
        {
            _logger.LogError("Component: {status} Unknown device {device}",StatusCodes.Status404NotFound,device);
            return NotFound();
        }
        var components = data.Where(x => x.__Device == device).Select(x=>x.__Component ?? "device").Distinct();
        if (!components.Contains(component))
        {
            _logger.LogError("Component: {status} Unknown component {component}",StatusCodes.Status404NotFound,component);
            return NotFound();
        }

        var props = await _datasource.GetLatestDevicePropertiesAsync(device);
        var componentprops = props.Where(x => component == (x.__Component ?? "device"));

        if (!componentprops.Any())
        {
            _logger.LogError("Component: {status} No data for component {component}",StatusCodes.Status404NotFound,component);
            return NotFound();
        }

        // Query InfluxDB, compose into UI slabs
        data = await _datasource.GetLatestDevicePropertiesAsync(device);
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
        _logger.LogInformation("ExecuteCommand: Device {device} Component {component} payload {payload}", device, component ?? "null", payload);

        // Not found if we don't know about the device, or component
        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();
        var devices = data.Select(x => x.__Device).Distinct();
        if (!devices.Contains(device))
        {
            _logger.LogError("ExecuteCommand: {status} Unknown device {device}",StatusCodes.Status404NotFound,device);
            return NotFound();
        }
        var components = data.Where(x => x.__Device == device).Select(x=>x.__Component ?? "device").Distinct();
        if (!components.Contains(device))
        {
            _logger.LogError("ExecuteCommand: {status} Unknown component {component}",StatusCodes.Status404NotFound,component);
            return NotFound();
        }

        // Bad request if fails DTMI, e.g. wrong command or payload

        return Ok();
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
        _logger.LogInformation("SetProperty: Device {device} Component {component} payload {payload}", device, component ?? "null", payload);

        // Not found if we don't know about the device, or component
        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();
        var devices = data.Select(x => x.__Device).Distinct();
        if (!devices.Contains(device))
        {
            _logger.LogError("SetProperty: {status} Unknown device {device}",StatusCodes.Status404NotFound,device);
            return NotFound();
        }
        var components = data.Where(x => x.__Device == device).Select(x=>x.__Component ?? "device").Distinct();
        if (!components.Contains(device))
        {
            _logger.LogError("SetProperty: {status} Unknown component {component}",StatusCodes.Status404NotFound,component);
            return NotFound();
        }

        // Bad request if fails DTMI, e.g. wrong command or payload

        return Ok();
    }
}
