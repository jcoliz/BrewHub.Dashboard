using BrewHub.Dashboard.Core.Providers;
using Microsoft.AspNetCore.Mvc;

namespace BrewHub.Dashboard.Controllers;

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
