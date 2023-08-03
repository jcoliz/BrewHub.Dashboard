// Copyright (C) 2023 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

namespace Dashboard.Services.DeviceMessaging;

using System.Text.Json;
using BrewHub.Dashboard.Core.Models;
using BrewHub.Dashboard.Core.Providers;
using BrewHub.Protocol.Mqtt;
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
    private readonly ILogger<MqttDeviceMessaging> _logger;

    private readonly MqttOptions _options;

    private readonly MessageGenerator _messagegenerator;

    private IManagedMqttClient? mqttClient;

    public MqttDeviceMessaging(ILogger<MqttDeviceMessaging> logger, IOptions<MqttOptions> options)
    {
        _logger = logger;

        if (options?.Value is null)
            throw new ApplicationException("Must set MQTT options in configuration");

        _options = options.Value;
        _messagegenerator = new MessageGenerator(_options);
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

    public async Task SendDesiredPropertyAsync(Datapoint point)
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

        var props = new Dictionary<string, object>() { { point.__Field, point.__Value } };

        // Create message

        var (topic, payload) = _messagegenerator.Generate(MessageGenerator.MessageKind.Command, point.__Device, point.__Component, point.__Model, props);

        // Send it

        var json = System.Text.Json.JsonSerializer.Serialize(payload);
        
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