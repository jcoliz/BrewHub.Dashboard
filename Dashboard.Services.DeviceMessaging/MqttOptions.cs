namespace Dashboard.Services.DeviceMessaging;

public class MqttOptions
{
    public static string Section => "MQTT";

    public string? Server { get; set; }

    public int Port { get; set; } = 1883;

    public string? Topic { get; set; }

    public string Site { get; set; } = "none";
}
