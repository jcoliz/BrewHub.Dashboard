using BrewHub.Dashboard.Core.Dtmi;
using BrewHub.Dashboard.Core.Models;
using BrewHub.Dashboard.Core.Providers;
using Microsoft.AspNetCore.Mvc;

namespace BrewHub.Dashboard.Controllers;

public enum TimeframeEnum { Minutes = 0, Hour, Hours, Day, Week, Month };

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DevicesController : ControllerBase
{
    private readonly IDataSource _datasource;
    private readonly ILogger<DevicesController> _logger;

    private readonly string[] _devices = new[] { "A", "B", "C" };

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
    [ProducesResponseType(typeof(Core.Dtmi.Slab[]),StatusCodes.Status200OK)]
    public async Task<ActionResult> Slabs()
    {
        _logger.LogInformation("Slabs");

        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();

        var slabs = data
                    .GroupBy(x => x.__Device)
                    .Select(x =>
                        new Core.Dtmi.Slab()
                        {
                            Header = x.Key,
                            Properties = x.Select(y => new Core.Dtmi.KeyValueUnits() { Key = y.__Field, Value = y.__Value.ToString() ?? "null" })
                        }
                    )
                    .ToArray();

        return Ok(slabs);
    }

    /// <summary>
    /// Get summary chart of all device telemetry
    /// </summary>
    /// <returns>Summary chart</returns>
    [HttpGet]
    [Route("[action]")]
    [ProducesResponseType(typeof(Common.ChartJS.ChartConfig),StatusCodes.Status200OK)]
    public async Task<ActionResult> Chart()
    {
        _logger.LogInformation("Chart");

        // Pull raw telemetry from database
        // TODO: Need to get all telemetry in this call
        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();
        var dtmi = new DeviceModelDetails();
        var result = BrewHub.Dashboard.Core.Charting.ChartMaker.CreateMultiDeviceBarChart(data, dtmi.VisualizeTelemetryTop);

        return Ok(result);
    }

    /// <summary>
    /// Get details for a single device
    /// </summary>
    /// <param name="id">Which device</param>
    /// <returns>Detail slabs, one for each component</returns>
    [HttpGet]
    [Route("{id}")]
    [ProducesResponseType(typeof(Core.Dtmi.Slab[]),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Device([FromRoute] string id)
    {
        _logger.LogInformation("Device: {device}", id);

        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();
        var devices = data.Select(x => x.__Device).Distinct();

        if (!devices.Contains(id))
        {
            _logger.LogError("Device: {status} Unknown device {device}",StatusCodes.Status404NotFound,id);
            return NotFound();
        }

        var props = await _datasource.GetLatestDevicePropertiesAsync(id);

        var slabs = props
                    .GroupBy(x => x.__Component ?? "device")
                    .Select(x =>
                        new Core.Dtmi.Slab()
                        {
                            Header = x.Key,
                            Properties = x.Select(y => new Core.Dtmi.KeyValueUnits() { Key = y.__Field, Value = y.__Value.ToString() ?? "null" })
                        }
                    )
                    .ToArray();

        return Ok(slabs);
    }

    /// <summary>
    /// Get telemetry across all components for this device
    /// </summary>
    /// <param name="id">Which device</param>
    /// <param name="timeframe">Timeframe to covert</param>
    /// <returns>Chart</returns>
    [HttpGet]
    [Route("{id}/Chart/{timeframe}")]
    [ProducesResponseType(typeof(Common.ChartJS.ChartConfig),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeviceChart([FromRoute] string id, [FromRoute] TimeframeEnum timeframe)
    {
        _logger.LogInformation("Request: Device Chart {device}", id);

        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();
        var devices = data.Select(x => x.__Device).Distinct();

        if (!devices.Contains(id))
        {
            _logger.LogError("DeviceChart: {status} Unknown device {device}",StatusCodes.Status404NotFound,id);
            return NotFound();
        }

        TimeSpan lookback = TimeSpan.Zero;
        TimeSpan bininterval = TimeSpan.Zero;
        string labelformat = string.Empty;

        try
        {
            lookback = timeframe switch
            {
                TimeframeEnum.Minutes => TimeSpan.FromMinutes(5), // "5m",
                TimeframeEnum.Hour => TimeSpan.FromHours(1), //"1h",
                TimeframeEnum.Hours => TimeSpan.FromHours(4), //"4h",
                TimeframeEnum.Day => TimeSpan.FromHours(24), //"24h",
                TimeframeEnum.Week => TimeSpan.FromDays(7), //"7d",
                TimeframeEnum.Month => TimeSpan.FromDays(28), //"28d",
                _ => throw new NotImplementedException()
            };

            bininterval = timeframe switch
            {
                TimeframeEnum.Minutes =>  TimeSpan.FromSeconds(20), //"20s",
                TimeframeEnum.Hour =>  TimeSpan.FromMinutes(2), //"2m",
                TimeframeEnum.Hours =>  TimeSpan.FromMinutes(15), //"15m",
                TimeframeEnum.Day =>  TimeSpan.FromHours(1), //"1h",
                TimeframeEnum.Week =>  TimeSpan.FromDays(1), //"1d",
                TimeframeEnum.Month => TimeSpan.FromDays(7), //"7d",
                _ => throw new NotImplementedException()
            };

            labelformat = timeframe switch
            {
                TimeframeEnum.Minutes => "mm:ss",
                TimeframeEnum.Hour or 
                TimeframeEnum.Hours or 
                TimeframeEnum.Day => "H:mm",
                TimeframeEnum.Week or 
                TimeframeEnum.Month => "M/dd",
                _ => throw new NotImplementedException()
            };
        }
        catch (NotImplementedException)
        {
            _logger.LogError("DeviceChart: {status} Unxpected timeframe {timeframe}",StatusCodes.Status400BadRequest,timeframe);
            return BadRequest();
        }

        // Get historical telemetry data for all components on this device
        data = await _datasource.GetSingleDeviceTelemetryAsync(id, lookback, bininterval);
        var dtmi = new DeviceModelDetails();
        var result = BrewHub.Dashboard.Core.Charting.ChartMaker.CreateMultiLineChart(data, dtmi.VisualizeTelemetryDevice, labelformat);

        return Ok(result);
    }

    /// <summary>
    /// Get details for a single component
    /// </summary>
    /// <param name="id">Which device</param>
    /// <param name="component">Which component, or "device" for device details</param>
    /// <returns>Detail slabs, one for each kind of metric</returns>
    [HttpGet]
    [Route("{id}/Component/{component}")]
    [ProducesResponseType(typeof(Core.Dtmi.Slab[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Component ([FromRoute] string id, [FromRoute] string? component )
    {
        _logger.LogInformation("Component: Device {device} Component {component}", id, component);

        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();
        var devices = data.Select(x => x.__Device).Distinct();
        if (!devices.Contains(id))
        {
            _logger.LogError("Component: {status} Unknown device {device}",StatusCodes.Status404NotFound,id);
            return NotFound();
        }
        var components = data.Where(x => x.__Device == id).Select(x=>x.__Component ?? "device").Distinct();
        if (!components.Contains(id))
        {
            _logger.LogError("Component: {status} Unknown component {component}",StatusCodes.Status404NotFound,component);
            return NotFound();
        }

        var props = await _datasource.GetLatestDevicePropertiesAsync(id);

        var componentprops = props.Where(x => component == (x.__Component ?? "device"));

        if (!componentprops.Any())
            return NotFound();

        var slabs = componentprops
                    .GroupBy(x => x.__Component ?? "device")
                    .Select(x =>
                        new Core.Dtmi.Slab()
                        {
                            Header = x.Key,
                            Properties = x.Select(y => new Core.Dtmi.KeyValueUnits() { Key = y.__Field, Value = y.__Value.ToString() ?? "null" })
                        }
                    )
                    .ToArray();

        return Ok(slabs);
    }

    /// <summary>
    /// Execute command on device
    /// </summary>
    /// <param name="payload">Parameters to send to command</param>
    /// <param name="id">Name of device</param>
    /// <param name="component">Name of component, or "device"</param>
    /// <param name="command">Name of command</param>
    [HttpPost]
    [Route("{id}/Component/{component}/Command/{command}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ExecuteCommand([FromBody] object payload, [FromRoute] string id, [FromRoute] string component, [FromRoute] string command)
    {
        _logger.LogInformation("ExecuteCommand: Device {device} Component {component} payload {payload}", id, component ?? "null", payload);

        // Not found if we don't know about the device, or component
        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();
        var devices = data.Select(x => x.__Device).Distinct();
        if (!devices.Contains(id))
        {
            _logger.LogError("ExecuteCommand: {status} Unknown device {device}",StatusCodes.Status404NotFound,id);
            return NotFound();
        }
        var components = data.Where(x => x.__Device == id).Select(x=>x.__Component ?? "device").Distinct();
        if (!components.Contains(id))
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
    /// <param name="id">Name of device</param>
    /// <param name="component">Name of component, or "device"</param>
    /// <param name="property">Name of property</param>
    [HttpPost]
    [Route("{id}/Component/{component}/Property/{property}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetProperty([FromBody] object payload, [FromRoute] string id, [FromRoute] string component, [FromRoute] string property)
    {
        _logger.LogInformation("SetProperty: Device {device} Component {component} payload {payload}", id, component ?? "null", payload);

        // Not found if we don't know about the device, or component
        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();
        var devices = data.Select(x => x.__Device).Distinct();
        if (!devices.Contains(id))
        {
            _logger.LogError("SetProperty: {status} Unknown device {device}",StatusCodes.Status404NotFound,id);
            return NotFound();
        }
        var components = data.Where(x => x.__Device == id).Select(x=>x.__Component ?? "device").Distinct();
        if (!components.Contains(id))
        {
            _logger.LogError("SetProperty: {status} Unknown component {component}",StatusCodes.Status404NotFound,component);
            return NotFound();
        }

        // Bad request if fails DTMI, e.g. wrong command or payload

        return Ok();
    }
}
