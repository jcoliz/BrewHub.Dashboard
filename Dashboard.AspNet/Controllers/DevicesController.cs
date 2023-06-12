using BrewHub.Dashboard.Core.Dtmi;
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

    [HttpGet]
    [Route("")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    public async Task<ActionResult> Devices()
    {
        _logger.LogInformation("Request: Devices");

        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();
        var devices = data.Select(x => x.__Device).Distinct();

        return Ok(devices);
    }

    [HttpGet]
    [Route("[action]")]
    [ProducesResponseType(typeof(Core.Dtmi.Slab[]),StatusCodes.Status200OK)]
    public async Task<ActionResult> Slabs()
    {
        _logger.LogInformation("Request: Device Slabs");

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

    [HttpGet]
    [Route("{id}")]
    [ProducesResponseType(typeof(Core.Dtmi.Slab[]),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Device([FromRoute] string id)
    {
        _logger.LogInformation("Request: Device {device}", id);

        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();
        var devices = data.Select(x => x.__Device).Distinct();

        if (!devices.Contains(id))
            return NotFound();

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

    //        public async Task OnGetAsync(TimeframeEnum t, string id)

    [HttpGet]
    [Route("{id}/Chart/{timeframe}")]
    [ProducesResponseType(typeof(Common.ChartJS.ChartConfig),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeviceChart([FromRoute] string id, [FromRoute] TimeframeEnum timeframe)
    {
        _logger.LogInformation("Request: Device Chart {device}", id);

        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();
        var devices = data.Select(x => x.__Device).Distinct();

        if (!devices.Contains(id))
            return NotFound();


        var lookback = timeframe switch
        {
            TimeframeEnum.Minutes => TimeSpan.FromMinutes(5), // "5m",
            TimeframeEnum.Hour => TimeSpan.FromHours(1), //"1h",
            TimeframeEnum.Hours => TimeSpan.FromHours(4), //"4h",
            TimeframeEnum.Day => TimeSpan.FromHours(24), //"24h",
            TimeframeEnum.Week => TimeSpan.FromDays(7), //"7d",
            TimeframeEnum.Month => TimeSpan.FromDays(28), //"28d",
            _ => throw new NotImplementedException()
        };

        var bininterval = timeframe switch
        {
            TimeframeEnum.Minutes =>  TimeSpan.FromSeconds(20), //"20s",
            TimeframeEnum.Hour =>  TimeSpan.FromMinutes(2), //"2m",
            TimeframeEnum.Hours =>  TimeSpan.FromMinutes(15), //"15m",
            TimeframeEnum.Day =>  TimeSpan.FromHours(1), //"1h",
            TimeframeEnum.Week =>  TimeSpan.FromDays(1), //"1d",
            TimeframeEnum.Month => TimeSpan.FromDays(7), //"7d",
            _ => throw new NotImplementedException()
        };

        var labelformat = timeframe switch
        {
            TimeframeEnum.Minutes => "mm:ss",
            TimeframeEnum.Hour or 
            TimeframeEnum.Hours or 
            TimeframeEnum.Day => "H:mm",
            TimeframeEnum.Week or 
            TimeframeEnum.Month => "M/dd",
            _ => throw new NotImplementedException()
        };

        // Get historical telemetry data for all components on this device
        data = await _datasource.GetSingleDeviceTelemetryAsync(id, lookback, bininterval);
        var dtmi = new DeviceModelDetails();
        var result = BrewHub.Dashboard.Core.Charting.ChartMaker.CreateMultiLineChart(data, dtmi.VisualizeTelemetryDevice, labelformat);

        return Ok(result);
    }

    [HttpGet]
    [Route("{id}/component/{component}")]
    [ProducesResponseType(typeof(Core.Dtmi.Slab[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Component ([FromRoute] string id, [FromRoute] string? component )
    {
        _logger.LogInformation("Request: Device {device} Component {component}", id, component);

        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();
        var devices = data.Select(x => x.__Device).Distinct();

        if (!devices.Contains(id))
            return NotFound();

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

    [HttpPost]
    [Route("{id}/component/{component}/command/{command}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult ExecuteCommand([FromBody] object payload, [FromRoute] string id, [FromRoute] string component, [FromRoute] string command)
    {
        _logger.LogInformation("OK Command Device {device} Component {component} payload {payload}", id, component ?? "null", payload);

        // Not found if we don't know about the device, or component
        if (!_devices.Contains(id))
            return NotFound();

        // Bad request if fails DTMI, e.g. wrong command or payload

        return Ok();
    }

    [HttpPost]
    [Route("{id}/component/{component}/property/{property}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult SetProperty([FromBody] object payload, [FromRoute] string id, [FromRoute] string component, [FromRoute] string property)
    {
        _logger.LogInformation("OK Property Device {device} Component {component} payload {payload}", id, component ?? "null", payload);

        // Not found if we don't know about the device, or component
        if (!_devices.Contains(id))
            return NotFound();

        // Bad request if fails DTMI, e.g. wrong command or payload

        return Ok();
    }
}
