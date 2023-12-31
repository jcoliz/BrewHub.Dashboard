// Copyright (C) 2023 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

using BrewHub.Dashboard.Core.Dtmi;
using BrewHub.Dashboard.Core.Providers;
using BrewHub.Dashboard.Core.Display;
using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using System.Text.Json;
using BrewHub.Dashboard.Core.Models;

namespace BrewHub.Dashboard.Api;

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
    private readonly DeviceModelRepository _dtmi;
    private readonly DisplayMetricGroupBuilder _metricgroupbuilder;

    private readonly IEnumerable<IDeviceMessaging> _messagingservices;

    public DevicesController(ILogger<DevicesController> logger, IDataSource datasource, IEnumerable<IDeviceMessaging> messagingservices)
    {
        _logger = logger;
        _datasource = datasource;
        _dtmi = new();
        _metricgroupbuilder = new(_dtmi);
        _messagingservices = messagingservices;
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
        // TODO: REname this. It actually returns ALL metrics, and it's
        // up to us to narrow them.
        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();

        // Narrow to all metrics which are expected to show up at the solution level
        // Make slabs out of those
        var slabs = 
            data
                .Where(x => _dtmi.IsMetricShownAtLevel(x, DeviceModelMetricVisualizationLevel.Solution))
                .GroupBy(x => x.__Device)
                .Select(_metricgroupbuilder.FromDevice)
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

        // This is to help test...
        // Bug 1618: Unhandled exceptions in API controller should form a proper problem details response
        if (device == "crash")
            throw new ApplicationException("User-initiated failure");

        if (!(await DoesDeviceExistAsync(device)))
            return Problem(
                title: "Not Found",
                statusCode: StatusCodes.Status404NotFound,
                instance: Request.Path,
                detail: $"Device {device} does not exist in the database"
            );

        // Query InfluxDB, compose into UI slabs
        var data = await _datasource.GetLatestDevicePropertiesAsync(device);
        var dtmi = new DeviceModelRepository();
        var slabs = 
            data
                .Where(x => _dtmi.IsMetricShownAtLevel(x, DeviceModelMetricVisualizationLevel.Device))
                .GroupBy(x => x.__Component ?? "device")
                .Select(_metricgroupbuilder.FromComponent)
                .ToArray();

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

        // Note that this flow is pretty inefficient. It involves THREE queries to the server, when
        // it really could be optimized to complete in ONE
        if (!(await DoesDeviceExistAsync(device)))
            return Problem(
                title: "Not Found",
                statusCode: StatusCodes.Status404NotFound,
                instance: Request.Path,
                detail: $"Device {device} does not exist in the database"
            );

        if (!(await DoesDeviceAndComponentExistAsync(device,component)))
            return Problem(
                title: "Not Found",
                statusCode: StatusCodes.Status404NotFound,
                instance: Request.Path,
                detail: $"Component {component} does not exist in the database on device {device}"
            );

        // Query InfluxDB, compose into UI slabs
        var data = await _datasource.GetLatestDevicePropertiesAsync(device);
        var dtmi = new DeviceModelRepository();
        var metrics = data
            .Where(x => component == (x.__Component ?? "device"))
            .Where(x => _dtmi.IsMetricShownAtLevel(x, DeviceModelMetricVisualizationLevel.Component));
        var slabs = _metricgroupbuilder.ManyFromComponent(metrics);

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
#if false
        var dtmi = new DeviceModelDetails();
        if (!dtmi.GetCommands(component).Any(x=>x.Id == command))
        {
            _logger.LogError("{caller}: {status} Not known command {command}","ExecuteCommand",StatusCodes.Status400BadRequest,command);
            return BadRequest();
        }
#endif
        return NoContent();
    }

    /// <summary>
    /// Set property on device
    /// </summary>
    /// <param name="payload">Details for the property. Uses all fields except __Time</param>
    [HttpPost]
    [Route("[action]")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SetProperty([FromBody] Datapoint payload)
    {
        _logger.LogInformation("SetProperty: {payload}", JsonSerializer.Serialize(payload));

        if (!(await DoesDeviceAndComponentExistAsync(payload.__Device,payload.__Component ?? "device")))
            return NotFound();

        // Bad request if fails DTMI, e.g. wrong command or payload
        if (!_dtmi.IsMetricWritable(payload))
        {
            _logger.LogError("{caller}: {status} Not a writable property {property}","SetProperty",StatusCodes.Status400BadRequest,payload.__Field);
            return BadRequest();
        }

        if (_messagingservices.Any())
        {
            try
            {
                var service = _messagingservices.First();

                // At this point, payload is a string-kind JsonElement as an object. We need a string
                // before moving on to the next stage in processing
                JsonElement el = (JsonElement)payload.__Value;
                var strpayload = el.GetString();

                // Device clients expect the value to be serialized in the correct type for what sort of property
                // it is (for Azure IoT compat). So here, we need to type the value correctly, based on model
                // (which is in the Datapoint payload), using the value as a string sent from the frontend
                // See Bug #1608: Json type mismatch sending desired properties, for more discussion
                object typedvalue = _dtmi.UnformatMetricValue(payload, strpayload!);

                var dpsend = new Datapoint()
                {
                    __Device = payload.__Device,
                    __Component = payload.__Component,
                    __Model = payload.__Model,
                    __Field = payload.__Field,
                    __Value = typedvalue

                };
                await service.SendDesiredPropertyAsync(dpsend);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SetProperty: Failed to send property");
            }
        }
        else
        {
            _logger.LogWarning("SetProperty: No messaging services configured, ignoring this request.");
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
