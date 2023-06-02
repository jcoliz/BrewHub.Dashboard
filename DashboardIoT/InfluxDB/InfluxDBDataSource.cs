using DashboardIoT.Core.Interfaces;
using InfluxDB.Client;
using InfluxDB.Client.Core.Flux.Domain;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardIoT.InfluxDB
{
    public class InfluxDBDataSource : IDataSource, IDisposable
    {
        private readonly ILogger<InfluxDBDataSource> _logger;
        private readonly InfluxDBClient _influxdbclient;
        private readonly Options _options;

        public class Options
        {
            public const string Section = "InfluxDB";
            public string Url { get; set; }
            public string Token { get; set; }
            public string Org { get; set; }
            public string Bucket { get; set; }
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
                Bucket = options.Bucket;
            }

            private readonly DateTime _SetTimeRangeStart;

            public string TimeRangeStart => _SetTimeRangeStart.ToString("O");

            private readonly DateTime _SetTimeRangeStop;

            public string TimeRangeStop => _SetTimeRangeStop.ToString("O");

            private readonly TimeSpan _SetWindowPeriod;

            public string WindowPeriod => Math.Round(_SetWindowPeriod.TotalSeconds) + "s";

            public string Organization { get; private set; }
            public string Bucket { get; private set; }
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

        public InfluxDBDataSource(IOptions<Options> options, ILogger<InfluxDBDataSource> logger)
        {
            _logger = logger;
            _options = options.Value;
            try
            {
                _influxdbclient = InfluxDBClientFactory.Create(_options.Url, _options.Token);
                _logger.LogInformation("InfluxDB: Created client OK");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InfluxDB: Create client failed");
            }
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

                var flux = $"from(bucket:\"{v.Bucket}\")" +
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
                _logger.LogError(ex, "InfluxDB: Query Failed");
                Console.WriteLine(ex.Message);
                return Enumerable.Empty<IReading>();
            }
        }

        private string ExtractKey(Dictionary<string,object> d)
        {
            return d.ContainsKey("component") switch
            {
                true => $"{d["component"]}/{d["_field"]}",
                false => $"{d["_field"]}"
            };
        }

        public async Task<Dictionary<string, Dictionary<string, object>>> GetLatestDeviceTelemetryAllAsync()
        {
            try
            {
                //
                // Query data
                //

                // TODO: This is where it would be great to have a tag for type=telemetry

                var flux = $"from(bucket:\"{_options.Bucket}\")" +
                    " |> range(start: -10m)" +
                    " |> filter(fn: (r) => r[\"_field\"] == \"workingSet\" or r[\"_field\"] == \"temperature\")" +
                    " |> last()" +
                    " |> keep(columns: [ \"device\", \"component\", \"_field\", \"_value\" ])";

                var fluxTables = await _influxdbclient.GetQueryApi().QueryAsync(flux, _options.Org);

                var result = fluxTables
                    .SelectMany(x => x.Records)
                    .Select(x => x.Values)
                    .GroupBy(x => x["device"].ToString())
                    .OrderBy(x => x.Key)
                    .ToDictionary(x => x.Key, x => x.ToDictionary(ExtractKey, y => y["_value"]));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InfluxDB: Query Failed");
                throw;
            }
        }

        /// <summary>
        /// Get latest value for all metrics for one device
        /// </summary>
        /// <param name="deviceid">Which device</param>
        /// <returns>
        /// Dictionary of component names (or string.empty) to telemetry key-value pairs
        /// </returns>
        public async Task<Dictionary<string, Dictionary<string, object>>> GetLatestDevicePropertiesAsync(string deviceid)
        {
            try
            {
                //
                // Query data
                //

                var flux = $"from(bucket:\"{_options.Bucket}\")" +
                    " |> range(start: -24h)" +
                    $" |> filter(fn: (r) => r[\"device\"] == \"{deviceid}\")" +
                    " |> filter(fn: (r) => r[\"_field\"] != \"Seq\" and r[\"_field\"] != \"__t\")" +
                    " |> last()" +
                    " |> keep(columns: [ \"component\", \"_field\", \"_value\" ])";

                var fluxTables = await _influxdbclient.GetQueryApi().QueryAsync(flux, _options.Org);

                string ExtractComponent(Dictionary<string,object> d)
                {
                    return d.ContainsKey("component") switch
                    {
                        true => $"{d["component"]}",
                        false => string.Empty
                    };
                }

                var result = fluxTables
                    .SelectMany(x => x.Records)
                    .Select(x => x.Values)
                    .GroupBy(ExtractComponent)
                    .OrderBy(x => x.Key)
                    .ToDictionary(x => x.Key, x => x.ToDictionary(y => y["_field"].ToString(), y => y["_value"]));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InfluxDB: Query Failed");
                throw;
            }
        }

        public async Task<Dictionary<string, List<(DateTimeOffset,double)>>> GetSingleDeviceTelemetryAsync(string deviceid, string lookback, string window)
        {
            try
            {
                //
                // Query data
                //

                // TODO: This is where it would be great to have a tag for type=telemetry

                var flux = $"from(bucket:\"{_options.Bucket}\")" +
                    $" |> range(start: -{lookback})" +
                    $" |> filter(fn: (r) => r[\"device\"] == \"{deviceid}\")" +
                    " |> filter(fn: (r) => r[\"_field\"] == \"temperature\")" +
                    " |> keep(columns: [ \"component\", \"_field\", \"_value\", \"_time\" ])" +
                    $" |> aggregateWindow(every: {window}, fn: mean, createEmpty: false)" +
                    "  |> yield(name: \"mean\")";

                /*from(bucket: "dockerism")
                  |> range(start: -24h)
                  |> filter(fn: (r) => r["device"] == "6a535e8e4e6e")
                  |> filter(fn: (r) => r["_field"] == "temperature")
                  |> aggregateWindow(every: 30m, fn: mean, createEmpty: false)  
                  |> keep(columns: [ "_field", "_value", "_time", "component"])
                  |> yield(name: "mean")
                */

                var fluxTables = await _influxdbclient.GetQueryApi().QueryAsync(flux, _options.Org);

                var result = fluxTables
                    .SelectMany(x => x.Records)
                    .Select(x => x.Values)
                    .GroupBy(ExtractKey)
                    .ToDictionary(
                        x => x.Key,
                        x => x.OrderBy(y => y["_time"])
                                .Select(y => (((NodaTime.Instant)y["_time"]).ToDateTimeOffset(),(double)y["_value"]))
                                .ToList()
                    );

#if false
                var result = fluxTables
                    .SelectMany(x => x.Records)
                    .Select(x => x.Values)
                    .GroupBy(x => new { c = ExtractComponent(x), f = x["_field"].ToString() })
                    .GroupBy(x => x.Key.c)
                    .ToDictionary(
                        x => x.Key, 
                        x => x.ToDictionary(
                            y => y.Key.f, 
                            y => y.OrderBy(z => z["_time"])
                                    .Select(z => Convert.ToDouble(z["_value"]))
                                    .ToList()
                        )
                    );
#endif
                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InfluxDB: Query Failed");
                throw;
            }
        }

    }
}
