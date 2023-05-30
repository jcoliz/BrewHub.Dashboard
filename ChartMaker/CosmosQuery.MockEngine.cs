namespace ChartMaker.CosmosQuery;

public class MockEngine : ICosmosQueryEngine
{
    public Task<Stream> DoQueryAsync(TimeSpan LookBack, TimeSpan BinInterval, IEnumerable<string> Devices)
    {
        var result = new List<object>();
        var rand = new Random();

        foreach(var device in Devices)
        {
            var time = DateTimeOffset.UtcNow;
            double workingset = 500000.0;
            double t1 = 60.0 + 20.0 * rand.NextDouble();
            double t2 = 80.0 + 40.0 * rand.NextDouble();
            int divisions = (int)Math.Round(LookBack / BinInterval, 0, MidpointRounding.AwayFromZero);
            while(divisions-- > 0)
            {
                result.Add(new DeviceReading() { __Device = device, __Time = time, WorkingSet = workingset });
                result.Add(new TempReading() { __Device = device, __Time = time, __Component = "thermostat1", Temperature = t1 });
                result.Add(new TempReading() { __Device = device, __Time = time, __Component = "thermostat2", Temperature = t2 });

                workingset += workingset * (rand.NextDouble() - 0.5) / 20.0;
                t1 += t1 * (rand.NextDouble() - 0.5) / 20.0;
                t2 += t2 * (rand.NextDouble() - 0.5) / 10.0;
                time -= BinInterval;
            }
        }

        var json = System.Text.Json.JsonSerializer.Serialize(result);
        return Task.FromResult<Stream>(new MemoryStream(System.Text.Encoding.UTF8.GetBytes(json)));
    }

    public class DeviceReading
    {
        public string __Device { get; set; } = string.Empty;
        public DateTimeOffset __Time { get; set; } = DateTimeOffset.MinValue;
        public double WorkingSet { get; set; } = 0;
    }

    public class TempReading
    {
        public string __Device { get; set; } = string.Empty;
        public string __Component { get; set; } = string.Empty;
        public DateTimeOffset __Time { get; set; } = DateTimeOffset.MinValue;
        public double Temperature { get; set; } = 0;
    }

}