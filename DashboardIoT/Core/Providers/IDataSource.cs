using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ChartMaker.Models;

namespace BrewHub.Core.Providers
{
    public interface IDataSource
    {
        /// <summary>
        /// Get latest value for all telemetry from all devices
        /// </summary>
        /// <returns>
        /// Dictionary of device names to component/metric key-value pairs
        /// </returns>
        public Task<IEnumerable<Datapoint>> GetLatestDeviceTelemetryAllAsync();

        /// <summary>
        /// Get latest value for all metrics for one device
        /// </summary>
        /// <param name="deviceid">Which device</param>
        /// <returns>
        /// Dictionary of component names (or string.empty) to metric key-value pairs
        /// </returns>
        public Task<IEnumerable<Datapoint>> GetLatestDevicePropertiesAsync(string deviceid);

        /// <summary>
        /// Get latest value for all metrics for one device
        /// </summary>
        /// <param name="deviceid">Which device</param>
        /// <param name="lookback">How far back from now to look</param>
        /// <param name="interval">How much time should each data point cover</param>
        /// <returns>
        /// Dictionary of component/field names to list of time/values
        /// </returns>
        public Task<IEnumerable<Datapoint>> GetSingleDeviceTelemetryAsync(string deviceid, TimeSpan lookback, TimeSpan interval);
    }
}
