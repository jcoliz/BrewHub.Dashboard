using DashboardIoT.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardIoT.Core.MockData
{
    public class MockDataSource : IDataSource
    {
        private readonly Dictionary<string, List<MockReading>> _metrics = new()
        {
            {
                "Distillery",
                new()
                {
                    new MockReading() { Topic = "sensors/thermocouple/Still-1/Top/Temp", Last = 35.0f },
                    new MockReading() { Topic = "sensors/thermocouple/Still-1/Middle/Temp", Last = 49.0f },
                    new MockReading() { Topic = "sensors/thermocouple/Still-1/Bottom/Temp", Last = 41.0f },
                    new MockReading() { Topic = "sensors/thermocouple/Still-2/Top/Temp", Last = 64.0f },
                    new MockReading() { Topic = "sensors/thermocouple/Still-2/Middle/Temp", Last = 69.0f },
                    new MockReading() { Topic = "sensors/thermocouple/Still-2/Bottom/Temp", Last = 60.0f },
                    new MockReading() { Topic = "sensors/thermocouple/Still-3/Top/Temp", Last = 50.0f },
                    new MockReading() { Topic = "sensors/thermocouple/Still-3/Middle/Temp", Last = 61.0f },
                    new MockReading() { Topic = "sensors/thermocouple/Still-3/Bottom/Temp", Last = 63.0f },
                }
            },
            {
                "Home",
                new()
                {
                    new MockReading() { Topic = "//Office/Desk/Temp", Units = "°C" },
                    new MockReading() { Topic = "//Office/Desk/Humidity", Units = "%RH" },
                }
            }
        };                       

        private readonly Random _random = new();

        public Task<Dictionary<string, Dictionary<string, object>>> GetLatestDevicePropertiesAsync(string deviceid)
        {
            throw new NotImplementedException();
        }

        public Task<Dictionary<string, Dictionary<string, object>>> GetLatestDeviceTelemetryAllAsync()
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<IReading>> GetMomentaryReadingsAsync(string site)
        {
            return Task.FromResult<IEnumerable<IReading>>(GetReadings(site));
        }

        public Task<IEnumerable<IReading>> GetSeriesReadingsAsync(string site, TimeSpan _, int divisions)
        {
            var result = _metrics[site].Select(x => new MockReading(x) { Values = Enumerable.Range(0, divisions).Select(d => NextRandomReading()) });

            return Task.FromResult<IEnumerable<IReading>>(result);
        }

        private IEnumerable<MockReading> GetReadings(string site)
        {
            foreach (var reading in _metrics[site])
                reading.Last = NextRandomReading();

            return _metrics[site];
        }

        Task<Dictionary<string, List<(DateTimeOffset,double)>>> IDataSource.GetSingleDeviceTelemetryAsync(string deviceid, string lookback, string window)
        {
            throw new NotImplementedException();
        }

        private double NextRandomReading() => _random.NextDouble() * 40.0 + 30.0;
    }
}
