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
using ChartMaker.Models;

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

        private Datapoint FluxToDatapoint(Dictionary<string,object> d)
        {
            return new Datapoint() 
            { 
                __Device = d["device"].ToString(), 
                __Component = d.GetValueOrDefault("component")?.ToString(),
                __Time = d.ContainsKey("_time") ? ((NodaTime.Instant)d["_time"]).ToDateTimeOffset() : DateTimeOffset.MinValue,
                __Field = d["_field"].ToString(), 
                __Value = d["_value"]
            };
        }

        public async Task<IEnumerable<Datapoint>> GetLatestDeviceTelemetryAllAsync()
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
                    .Select(x => FluxToDatapoint(x.Values));

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
        public async Task<IEnumerable<Datapoint>> GetLatestDevicePropertiesAsync(string deviceid)
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
                    " |> keep(columns: [ \"device\", \"component\", \"_field\", \"_value\" ])";

                var fluxTables = await _influxdbclient.GetQueryApi().QueryAsync(flux, _options.Org);

                var result = fluxTables
                    .SelectMany(x => x.Records)
                    .Select(x => FluxToDatapoint(x.Values));

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "InfluxDB: Query Failed");
                throw;
            }
        }

        public async Task<IEnumerable<Datapoint>> GetSingleDeviceTelemetryAsync(string deviceid, TimeSpan lookback, TimeSpan interval)
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
                     " |> keep(columns: [ \"device\", \"component\", \"_field\", \"_value\", \"_time\" ])" +
                    $" |> aggregateWindow(every: {intervalstr}, fn: mean, createEmpty: false)" +
                     " |> yield(name: \"mean\")";

                var fluxTables = await _influxdbclient.GetQueryApi().QueryAsync(flux, _options.Org);

                var result = fluxTables
                    .SelectMany(x => x.Records)
                    .Select(x => FluxToDatapoint(x.Values));

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
