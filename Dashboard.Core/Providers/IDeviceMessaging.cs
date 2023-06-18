namespace BrewHub.Dashboard.Core.Providers;

/// <summary>
/// Provides a means to communicate information to devices
/// </summary>
public interface IDeviceMessaging
{
    Task SendDesiredPropertyAsync(string deviceid, string? componentid, string metric, object value);

    Task SendCommandAsync(string deviceid, string? componentid, string metric, object value);
}