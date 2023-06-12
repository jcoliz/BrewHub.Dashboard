using BrewHub.Dashboard.Core.Dtmi;
using BrewHub.Dashboard.Core.Models;
using BrewHub.Dashboard.Core.Providers;
using Microsoft.AspNetCore.Mvc;

namespace BrewHub.Dashboard.Controllers;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class ChartsController : ControllerBase
{
    private readonly IDataSource _datasource;
    private readonly ILogger<ChartsController> _logger;

    public ChartsController(ILogger<ChartsController> logger, IDataSource datasource)
    {
        _logger = logger;
        _datasource = datasource;
    }

    /// <summary>
    /// Get summary chart of all device telemetry at the current moment
    /// </summary>
    /// <returns>Chart.js data object</returns>
    [HttpGet]
    [Route("[action]")]
    [ProducesResponseType(typeof(Common.ChartJS.ChartConfig),StatusCodes.Status200OK)]
    public async Task<ActionResult> Telemetry()
    {
        _logger.LogInformation("Telemetry");

        // Pull raw telemetry from database
        // TODO: Need to get all telemetry in this call
        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();
        var dtmi = new DeviceModelDetails();
        var result = BrewHub.Dashboard.Core.Charting.ChartMaker.CreateMultiDeviceBarChart(data, dtmi.VisualizeTelemetryTop);

        return Ok(result);
    }

    /// <summary>
    /// Get telemetry for a single device, including all components
    /// </summary>
    /// <param name="device">Which device</param>
    /// <param name="timeframe">Timeframe to covert</param>
    /// <returns>Chart.js data object</returns>
    [HttpGet]
    [Route("{device}/Telemetry/{timeframe}")]
    [ProducesResponseType(typeof(Common.ChartJS.ChartConfig),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> DeviceChart([FromRoute] string device, [FromRoute] TimeframeEnum timeframe)
    {
        _logger.LogInformation("DeviceChart: {device}", device);

        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();
        var devices = data.Select(x => x.__Device).Distinct();

        if (!devices.Contains(device))
        {
            _logger.LogError("DeviceChart: {status} Unknown device {device}",StatusCodes.Status404NotFound,device);
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
        data = await _datasource.GetSingleDeviceTelemetryAsync(device, lookback, bininterval);
        var dtmi = new DeviceModelDetails();
        var result = BrewHub.Dashboard.Core.Charting.ChartMaker.CreateMultiLineChart(data, dtmi.VisualizeTelemetryDevice, labelformat);

        return Ok(result);
    }

    /// <summary>
    /// Get telemetry for a single component on a single device
    /// </summary>
    /// <param name="device">Which device</param>
    /// <param name="component">Which component, or "device"</param>
    /// <param name="timeframe">Timeframe to covert</param>
    /// <returns>Chart.js data object</returns>
    [HttpGet]
    [Route("{device}/Component/{component}/Telemetry/{timeframe}")]
    [ProducesResponseType(typeof(Common.ChartJS.ChartConfig),StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> ComponentChart([FromRoute] string device, [FromRoute] string component, [FromRoute] TimeframeEnum timeframe)
    {
        _logger.LogInformation("ComponentChart: {device}/{component}", device, component);

        var data = await _datasource.GetLatestDeviceTelemetryAllAsync();
        var devices = data.Select(x => x.__Device).Distinct();
        if (!devices.Contains(device))
        {
            _logger.LogError("ComponentChart: {status} Unknown device {device}",StatusCodes.Status404NotFound,device);
            return NotFound();
        }
        var components = data.Where(x => x.__Device == device).Select(x=>x.__Component ?? "device").Distinct();
        if (!components.Contains(device))
        {
            _logger.LogError("ComponentChart: {status} Unknown component {component}",StatusCodes.Status404NotFound,component);
            return NotFound();
        }

        return Ok(new Common.ChartJS.ChartConfig());
    }

}