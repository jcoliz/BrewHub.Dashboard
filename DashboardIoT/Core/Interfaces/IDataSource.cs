using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardIoT.Core.Interfaces
{
    public interface IDataSource
    {
        // (Deprecated) V1 design
        public Task<IEnumerable<IReading>> GetMomentaryReadingsAsync(string site);
        public Task<IEnumerable<IReading>> GetSeriesReadingsAsync(string site, TimeSpan span, int divisions);

        // NEW V2 design

        /// <summary>
        /// Get latest value for all telemetry from all devices
        /// </summary>
        /// <returns>
        /// Dictionary of device names to telemetry key-value pairs
        /// </returns>
        public Task<Dictionary<string,Dictionary<string,object>>> GetLatestDeviceTelemetryAllAsync();

        /// <summary>
        /// Get latest value for all metrics for one device
        /// </summary>
        /// <param name="deviceid">Which device</param>
        /// <returns>
        /// Dictionary of component names (or string.empty) to telemetry key-value pairs
        /// </returns>
        public Task<Dictionary<string, Dictionary<string, object>>> GetLatestDevicePropertiesAsync(string deviceid);

    }
}
