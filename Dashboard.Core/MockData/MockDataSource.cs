// Copyright (C) 2023 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

using BrewHub.Dashboard.Core.Providers;
using BrewHub.Dashboard.Core.Models;

namespace BrewHub.Dashboard.Core.MockData
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

        public Task<IEnumerable<Datapoint>> GetLatestDevicePropertiesAsync(string deviceid)
        {
            var result = components.SelectMany
            (
                c => properties.Select(
                    t => new Datapoint()
                    {
                        __Device = deviceid,
                        __Component = c,
                        __Field = t,
                        __Value = (object)NextDouble1000()
                    }
                )
            );            
            return Task.FromResult(result);
        }

        public Task<IEnumerable<Datapoint>> GetLatestDeviceTelemetryAllAsync()
        {
            var now = DateTimeOffset.Now;
            var result = devices.SelectMany
            (
                d => components.SelectMany
                (
                    c => telemetry.Select(
                        t => new Datapoint() 
                        { 
                            __Device = d, 
                            __Component = string.IsNullOrEmpty(c) ? null : c, 
                            __Time = now, 
                            __Field = t,  
                            __Value = (object)NextDouble1000()
                        }
                    )
                )
            );

            return Task.FromResult(result);
        }

        public Task<IEnumerable<Datapoint>> GetSingleDeviceTelemetryAsync(string deviceid, TimeSpan lookback, TimeSpan interval)
        {
            var now = DateTimeOffset.Now;
            var dates = Enumerable.Range(0, (int)Math.Ceiling(lookback / interval)).Select(x => now + x * interval);
            var result = components.SelectMany
            (
                c => telemetry.Select(
                    t => new Datapoint()
                    {
                        __Device = deviceid,
                        __Component = c,
                        __Time = now,
                        __Field = t,
                        __Value = (object)NextDouble1000()
                    }
                )
            );
            return Task.FromResult(result);
        }

        public Task<IEnumerable<Datapoint>> GetSingleComponentTelemetryAsync(string deviceid, string? componentid, TimeSpan lookback, TimeSpan interval)
        {
            var now = DateTimeOffset.Now;
            var dates = Enumerable.Range(0, (int)Math.Ceiling(lookback / interval)).Select(x => now + x * interval);
            var result = components.Where(c=>c == (componentid ?? string.Empty)).SelectMany
            (
                c => telemetry.Select(
                    t => new Datapoint()
                    {
                        __Device = deviceid,
                        __Component = string.IsNullOrEmpty(c) ? null : c,
                        __Time = now,
                        __Field = t,
                        __Value = (object)NextDouble1000()
                    }
                )
            );
            return Task.FromResult(result);
        }
    }
}
