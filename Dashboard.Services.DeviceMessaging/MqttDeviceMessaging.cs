namespace Dashboard.Services.DeviceMessaging;

using BrewHub.Dashboard.Core.Providers;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;

/// <summary>
/// Provides messaging to devices over MQTT
/// </summary>
public class MqttDeviceMessaging: IDeviceMessaging
{
    public class Options
    {
        public static string Section => "MQTT";

        public string? Server { get; set; }

        public int Port { get; set; } = 1883;

        public string Topic { get; set; } = "empty";

        public string Site { get; set; } = "none";

        public string? ClientId { get; set; }
    }


    private readonly ILogger<MqttDeviceMessaging> _logger;

    private readonly string _basetopic;

    private readonly Options _options;

    private IManagedMqttClient? mqttClient;

    public MqttDeviceMessaging(ILogger<MqttDeviceMessaging> logger, IOptions<Options> options)
    {
        _logger = logger;

        if (options?.Value is null)
            throw new ApplicationException("Must set MQTT options in configuration");

        _options = options.Value;

        if (_options.Server is null)
            throw new ApplicationException("Must set MQTT:Server value in configuration");
        if (_options.ClientId is null)
            throw new ApplicationException("Must set MQTT:ClientId value in configuration");

        _basetopic = $"{_options.Topic}/{_options.Site}";        
    }

    /// <summary>
    /// Bring up connection to MQTT server
    /// </summary>
    /// <returns></returns>
    public async Task ConnectAsync()
    {
            MqttClientOptionsBuilder builder = new MqttClientOptionsBuilder()
                                        .WithClientId(_options.ClientId)
                                        .WithTcpServer(_options.Server, Convert.ToInt32(_options.Port));

            ManagedMqttClientOptions createoptions = new ManagedMqttClientOptionsBuilder()
                                    .WithAutoReconnectDelay(TimeSpan.FromSeconds(30))
                                    .WithClientOptions(builder.Build())
                                    .Build();

            mqttClient = new MqttFactory().CreateManagedMqttClient();

            mqttClient!.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnConnected);
            mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnDisconnected);            
            mqttClient.ConnectingFailedHandler = new ConnectingFailedHandlerDelegate(OnConnectingFailed);

            mqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(a => {
                _logger.LogInformation("Message recieved: {payload}", System.Text.Encoding.UTF8.GetString(a.ApplicationMessage.Payload));
            });

            await mqttClient.StartAsync(createoptions);

            _logger.LogDebug("Connection: Connecting on {server}:{port}",_options.Server,_options.Port);
    }

    private void OnConnected(MqttClientConnectedEventArgs obj)
    {
        _logger.LogInformation("Connection: OK.");
    }

    private void OnConnectingFailed(ManagedProcessFailedEventArgs obj)
    {
        _logger.LogError(obj.Exception, "Connection: Failed.");
    }

    private void OnDisconnected(MqttClientDisconnectedEventArgs obj)
    {
        _logger
            .LogError(
                "Connection: Error, Disconnected. {Reason} {was} {type} {message}", 
                obj.Reason, 
                obj.ClientWasConnected,
                obj.Exception?.GetType().Name ?? "(null)", obj.Exception?.Message ?? "(null)"
            );
    }


    public async Task SendDesiredPropertyAsync(string deviceid, string? componentid, string metric, object value)
    {
        if (mqttClient is null)
        {
            // Need to connect
            await ConnectAsync();

            // Wait until connected (or timeout)
            while (!mqttClient!.IsConnected)
            {
                await Task.Delay(500);
            }
        }
        
        var json = System.Text.Json.JsonSerializer.Serialize(value);
        
        var topic = string.IsNullOrEmpty(componentid) ? $"{_basetopic}/NCMD/{deviceid}" : $"{_basetopic}/NCMD/{deviceid}/{componentid}";

        var message = new MqttApplicationMessageBuilder()
            .WithTopic(topic)
            .WithPayload(json)
            .WithExactlyOnceQoS()
            .WithRetainFlag()
            .Build();

        await mqttClient!.PublishAsync(message, CancellationToken.None); // Since 3.0.5 with CancellationToken

        // Log about it
        _logger.LogInformation("Message: Sent {topic} {message}", topic, json);
    }

    public Task SendCommandAsync(string deviceid, string? componentid, string metric, object value)
    {
        throw new NotImplementedException();
    }
}