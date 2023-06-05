using BrewHub.Dashboard.Core.Models;
using System.Collections.Generic;
using System.Linq;

namespace BrewHub.Dashboard.Core.Dtmi;

/// <summary>
/// Provides the information needed to display data from a single top-level device model properly
/// </summary>
/// <remarks>
/// Currently this is hard-coded. In the future, it should be more dynamic
/// </remarks>
public class DeviceModelDetails: IDeviceModel
{
    /// <summary>
    /// Human-readable name of this schema
    /// </summary>
    public string SchemaName => "Temperature Controller";

    /// <summary>
    /// Telemetry values to be shown when looking at the solution overall
    /// </summary>
    public IEnumerable<string> VisualizeTelemetryTop => new[] { "thermostat1/temperature", "thermostat2/temperature" };

    /// <summary>
    /// Telemetry values to be shown when looking at a single device
    /// </summary>
    public IEnumerable<string> VisualizeTelemetryDevice => new[] { "thermostat1/temperature", "thermostat2/temperature" };

    /// <summary>
    /// Given a metric id, return the human-readable name
    /// </summary>
    /// <param name="metricid">Schema-defined identifier for metric (could be separated by `/`)</param>
    /// <returns>Human readable name</returns>
    public string MapMetricName(string metricid) => metricid switch
    {
        "thermostat1/temperature" => "Thermostat1/Temperature",
        "thermostat2/temperature" => "Thermostat2/Temperature",
        "serialNumber" => "Serial Number",
        "thermostat1" => "Thermostat One",
        "thermostat2" => "Thermostat Two",
        "deviceInformation" => "Device Information",
        "manufacturer" => "Manufacturer",
        "model" => "Device Model",
        "swVersion" => "Software Version",
        "osName" => "Operating System",
        "processorArchitecture" => "Processor Architecture",
        "totalStorage" => "Total Storage",
        "totalMemory" => "Total Memory",
        "temperature" => "Temperature",
        "maxTempSinceLastReboot" => "Max Temperature Since Reboot",
        "targetTemperature" => "Target Temperature",
        "workingSet" => $"Working Set",
        "telemetryPeriod" => "Telemetry Period",
        _ => metricid
    };

    /// <summary>
    /// Format a metric to human-readable form
    /// </summary>
    /// <param name="metric"></param>
    /// <returns></returns>
    public string FormatMetricValue(Datapoint metric) => metric.__Field switch
    {
        "maxTempSinceLastReboot" or
        "thermostat1/temperature" or
        "thermostat2/temperature" or
        "temperature"
                => $"{metric.__Value:F1}°C",
        "targetTemperature" => $"{metric.__Value:F1}",
        "workingSet" => $"{(double)metric.__Value/7812.5:F1}MB",
        "totalStorage" or
        "totalMemory" 
            => (double)metric.__Value switch
            {
                > 1000000 => $"{(double)metric.__Value/1000000:F1} GB",
                > 1000 => $"{(double)metric.__Value/1000:F1} MB",
                _ => $"{(double)metric.__Value:F1} kB"
            },
        _ => metric.__Value.ToString()
    };        

    /// <summary>
    /// Whether a specific metric is a writable property
    /// </summary>
    /// <param name="metricid"></param>
    /// <returns></returns>
    public bool IsMetricWritable(string metricid) => metricid switch
    {
        "targetTemperature" or
        "telemetryPeriod" => true,
        _ => false
    };

    /// <summary>
    /// Returns the units to be displayed with a metric, or null if metric is not writable, or no units need to be displayed
    /// </summary>
    /// <param name="metricid"></param>
    /// <returns></returns>
    public string GetWritableUnits(string metricid) => metricid switch
    {
        "targetTemperature" => "°C",
        _ => null
    };

    /// <summary>
    /// Return all the commands for a given component id
    /// </summary>
    /// <param name="componentid"></param>
    /// <returns></returns>
    public IEnumerable<KeyValueUnits> GetCommands(string componentid) => componentid switch
    {
        "" => new List<KeyValueUnits>()
                        {
                            new() { Key = "Reboot", Value = "Delay", Units = "s" }
                        }
        ,
        "thermostat1" or
        "thermostat2" => new List<KeyValueUnits>()
                        {
                            new() { Key = "Get Max-Min report", Value = "Since", Units = "D/T" }                                    
                        },
        _ => Enumerable.Empty<KeyValueUnits>()
    };
}