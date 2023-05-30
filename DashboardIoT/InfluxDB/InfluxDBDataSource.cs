using DashboardIoT.Core.Interfaces;
using InfluxDB.Client;
using InfluxDB.Client.Core.Flux.Domain;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardIoT.InfluxDB
{
    public class InfluxDBDataSource : IDataSource, IDisposable
    {
        private readonly InfluxDBClient _influxdbclient;
        private readonly Options _options;

        public class Options
        {
            public const string Section = "InfluxDB";
            public string Server { get; set; }
            public string Token { get; set; }
            public string Org { get; set; }
            public string DefaultBucket { get; set; }
        }

        public class QueryVariables
        {
            public QueryVariables(Options options, TimeSpan span, int divisions)
            {
                var now = DateTime.Now;
                _SetTimeRangeStart = now - span;
                _SetTimeRangeStop = now;
                _SetWindowPeriod = span/divisions;

                Organization = options.Org;
                DefaultBucket = options.DefaultBucket;
            }

            private readonly DateTime _SetTimeRangeStart;

            public string TimeRangeStart => _SetTimeRangeStart.ToString("O");

            private readonly DateTime _SetTimeRangeStop;

            public string TimeRangeStop => _SetTimeRangeStop.ToString("O");

            private readonly TimeSpan _SetWindowPeriod;

            public string WindowPeriod => Math.Round(_SetWindowPeriod.TotalSeconds) + "s";

            public string Organization { get; private set; }
            public string DefaultBucket { get; private set; }
        }

        public class Reading : IReading
        {
            public double Last { get; private set; }

            public string Node { get; set; }

            public string Device { get; set; }

            public string Name 
            { 
                get
                {
                    return _Name;
                }
                set
                {
                    if (value == "tempc")
                    {
                        _Name = "temp";
                        Units = "°C";
                    }
                    else
                    {
                        _Name = value;
                        Units = _unitsmap.GetValueOrDefault(value);
                    }
                }
            }

            private string _Name;

            public string Label => Name;

            public string Units { get; set; }

            public IEnumerable<double> Values 
            { 
                get
                {
                    return _Values;
                }
                set
                {
                    _Values = value;
                    Last = value.Last();
                }
            }

            private IEnumerable<double> _Values;

            public double Adjustment { get; set; }

            public static Reading FromFluxRecords(IEnumerable<FluxRecord> rs) =>
                new Reading() { Name = rs.Last().GetField(), Node = (string)rs.Last().GetValueByKey("node"), Device = (string)rs.Last().GetValueByKey("device"), Values = rs.Select(r => (double)r.GetValue()) };

            public static Reading FromFluxTable(FluxTable t) =>
                FromFluxRecords(t.Records);

            private readonly Dictionary<string, string> _unitsmap = new()
            {
                { "temp", "°F" },
                { "humidity", "%RH" }
            };
        }

        public InfluxDBDataSource(IOptions<Options> options)
        {
            _options = options.Value;
            _influxdbclient = InfluxDBClientFactory.Create(_options.Server, _options.Token);
        }

        public void Dispose()
        {
            _influxdbclient.Dispose();
        }

        public async Task<IEnumerable<IReading>> GetMomentaryReadingsAsync(string site) =>
            await GetSeriesReadingsAsync(site, TimeSpan.FromMinutes(5), 5);

        public async Task<IEnumerable<IReading>> GetSeriesReadingsAsync(string site, TimeSpan span, int divisions)
        {
            try
            {
                //
                // Query data
                //

                var v = new QueryVariables(_options, span, divisions);

                var flux = $"from(bucket:\"{v.DefaultBucket}\")" +
                    $" |> range(start: {v.TimeRangeStart}, stop:{v.TimeRangeStop})" +
                    $" |> filter(fn: (r) => r[\"_measurement\"] == \"zlan\")" +
                    $" |> filter(fn: (r) => r[\"site\"] == \"{site.ToLower()}\")" +
                    $" |> aggregateWindow(every: {v.WindowPeriod}, fn: mean, createEmpty: false)";

                var fluxTables = await _influxdbclient.GetQueryApi().QueryAsync(flux, v.Organization);

                // Each table is a single "reading", where a "reading" is really a metric plus a group of readings
                return fluxTables.Select(Reading.FromFluxTable);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Enumerable.Empty<IReading>();
            }
        }
    }
}
