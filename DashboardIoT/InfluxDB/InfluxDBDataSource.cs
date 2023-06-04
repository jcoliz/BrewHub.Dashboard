using BrewHub.Core.Providers;
using InfluxDB.Client;
using InfluxDB.Client.Core.Flux.Domain;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using System.Text.RegularExpressions;

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

        private string ComponentSlashField(Dictionary<string,object> d)
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
                    .ToDictionary(x => x.Key, x => x.ToDictionary(ComponentSlashField, y => y["_value"]));

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

                string ComponentOrEmpty(Dictionary<string,object> d)
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
                    .GroupBy(ComponentOrEmpty)
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

        public async Task<Dictionary<string, List<(DateTimeOffset,double)>>> GetSingleDeviceTelemetryAsync(string deviceid, TimeSpan lookback, TimeSpan interval)
        {
            try
            {
                //
                // Query data
                //

                // Convert timespan into flux time construct
                Regex regex = new Regex("^[PT]+(?<value>.+)");
                string lookbackstr = regex.Match(XmlConvert.ToString(lookback)).Groups["value"].Value.ToLowerInvariant();
                string intervalstr = regex.Match(XmlConvert.ToString(interval)).Groups["value"].Value.ToLowerInvariant();

                // TODO: This is where it would be great to have a tag for type=telemetry
                var flux = $"from(bucket:\"{_options.Bucket}\")" +
                    $" |> range(start: -{lookbackstr})" +
                    $" |> filter(fn: (r) => r[\"device\"] == \"{deviceid}\")" +
                     " |> filter(fn: (r) => r[\"_field\"] == \"temperature\")" +
                     " |> keep(columns: [ \"component\", \"_field\", \"_value\", \"_time\" ])" +
                    $" |> aggregateWindow(every: {intervalstr}, fn: mean, createEmpty: false)" +
                     " |> yield(name: \"mean\")";

                var fluxTables = await _influxdbclient.GetQueryApi().QueryAsync(flux, _options.Org);

                var result = fluxTables
                    .SelectMany(x => x.Records)
                    .Select(x => x.Values)
                    .GroupBy(ComponentSlashField)
                    .ToDictionary(
                        x => x.Key,
                        x => x.OrderBy(y => y["_time"])
                                .Select(y => (((NodaTime.Instant)y["_time"]).ToDateTimeOffset(),(double)y["_value"]))
                                .ToList()
                    );

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
