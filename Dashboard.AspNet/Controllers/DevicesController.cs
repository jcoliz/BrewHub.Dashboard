using Microsoft.AspNetCore.Mvc;

namespace BrewHub.Dashboard.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class DisplayController : ControllerBase
{
    private readonly ILogger<DisplayController> _logger;

    private readonly string[] _devices = new[] { "A", "B", "C" };

    public DisplayController(ILogger<DisplayController> logger)
    {
        _logger = logger;
    }

    [HttpGet]
    [Route("devices")]
    [ProducesResponseType(typeof(string[]), StatusCodes.Status200OK)]
    public ActionResult Devices()
    {
        _logger.LogInformation("Request: Devices");

        return Ok(_devices);
    }

    [HttpGet]
    [Route("devices/slabs")]
    [ProducesResponseType(typeof(Core.Dtmi.Slab[]),StatusCodes.Status200OK)]
    public ActionResult DeviceSlabs()
    {
        _logger.LogInformation("Request: Device Slabs");

        var slabs = _devices.Select(x=> new Core.Dtmi.Slab(){ Header = $"Device {x}" });

        return Ok(slabs);
    }

    [HttpGet]
    [Route("device/{id}")]
    [ProducesResponseType(typeof(Core.Dtmi.Slab[]),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult Device([FromRoute] string id)
    {
        _logger.LogInformation("Request: Device {device}", id);

        if (!_devices.Contains(id))
            return NotFound();

        return Ok(new Core.Dtmi.Slab[] { new(){ Header = $"Device {id}" } });
    }

    [HttpGet]
    [Route("device/{id}/component/{component}")]
    [ProducesResponseType(typeof(Core.Dtmi.Slab[]), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public ActionResult Component ([FromRoute] string id, [FromRoute] string component )
    {
        _logger.LogInformation("Request: Device {device} Component {component}", id, component);

        if (!_devices.Contains(id))
            return NotFound();

        return Ok(new Core.Dtmi.Slab[] { new(){ Header = $"Device {id}", ComponentId = component } });
    }

    [HttpPost]
    [Route("device/{id}/component/{component}/command/{command}")]
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
    [Route("device/{id}/component/{component}/property/{property}")]
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
