using BrewHub.Core.Providers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardIoT.Core.MockData
{
    public class MockDataSource : IDataSource
    {
        private readonly Random _random = new();

        private readonly List<string> devices = new();

        private readonly List<string> components = new() { string.Empty };

        private readonly List<string> telemetry = new();
        private readonly List<string> properties = new();

        private const int numdevices = 6;
        private const int numcomponents = 3;
        private const int numtelemetry = 2;
        private const int numproperties = 4;

        private string NextInt64String()
        {
            return (_random.NextInt64() >> 32).ToString("X");
        }

        private double NextDouble1000()
        {
            return Math.Round(_random.NextDouble() * 1000, 1);
        }

        public MockDataSource()
        {
            devices.AddRange(Enumerable.Range(1, numdevices).Select(_ => NextInt64String()));
            components.AddRange(Enumerable.Range(1, numcomponents).Select(_ => NextInt64String()));
            telemetry.AddRange(Enumerable.Range(1, numtelemetry).Select(_ => NextInt64String()));
            properties.AddRange(Enumerable.Range(1, numproperties).Select(_ => NextInt64String()));
        }

        public Task<Dictionary<string, Dictionary<string, object>>> GetLatestDevicePropertiesAsync(string deviceid)
        {
            var result = components.ToDictionary(x => x,x => properties.ToDictionary(y => y, y => (object)NextDouble1000()));
            return Task.FromResult(result);
        }

        public Task<Dictionary<string, Dictionary<string, object>>> GetLatestDeviceTelemetryAllAsync()
        {
            var result = devices.ToDictionary(
                x => x,
                x => components
                        .SelectMany(x => telemetry.Select(y => $"{x}/{y}"))
                        .ToDictionary(x => x, x => (object)NextDouble1000())
            );
            return Task.FromResult(result);
        }

        public Task<Dictionary<string, List<(DateTimeOffset,double)>>> GetSingleDeviceTelemetryAsync(string deviceid, TimeSpan lookback, TimeSpan interval)
        {
            var now = DateTimeOffset.Now;
            var dates = Enumerable.Range(0, (int)Math.Ceiling(lookback / interval)).Select(x => now + x * interval);
            var result = components
                            .ToDictionary(
                                x => x,
                                x => dates.Select(y=>(y,NextDouble1000())).ToList()
                            );
            return Task.FromResult(result);
        }
    }
}
