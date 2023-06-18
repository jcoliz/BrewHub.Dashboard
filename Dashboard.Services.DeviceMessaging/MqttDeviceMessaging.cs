namespace Dashboard.Services.DeviceMessaging;

using BrewHub.Dashboard.Core.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

/// <summary>
/// Provides messaging to devices over MQTT
/// </summary>
public class MqttDeviceMessaging: IDeviceMessaging
{
    private readonly ILogger<MqttDeviceMessaging> logger;

    private readonly string basetopic;

    private readonly MqttOptions options;

    public MqttDeviceMessaging(ILogger<MqttDeviceMessaging> _logger, IOptions<MqttOptions> _options)
    {
        logger = _logger;

        if (_options is null)
            throw new ApplicationException("Must set MQTT options");

        options = _options.Value;

        basetopic = $"{options.Topic}/{options.Site}";
    }

    /// <summary>
    /// Bring up connection to MQTT server
    /// </summary>
    /// <returns></returns>
    public Task ConnectAsync()
    {
        throw new NotImplementedException();
    }

    public Task SendDesiredPropertyAsync(string deviceid, string? componentid, string metric, object value)
    {
        throw new NotImplementedException();
    }

    public Task SendCommandAsync(string deviceid, string? componentid, string metric, object value)
    {
        throw new NotImplementedException();
    }
}