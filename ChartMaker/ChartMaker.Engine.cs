using Common.ChartJS;
using System.Linq;

namespace ChartMaker;
public static class Engine
{
    public static ChartConfig CreateBarChart(IEnumerable<Models.Datapoint> points, string key, string timeformat)
    {
        // Decompose the key into component and field parts
        var split = key.Split('/');
        if (split.Length > 2)
            throw new ArgumentException($"Unexpected key segments. Allowed=1 or 2. Found = {split.Length}");

        string? component = (split.Length == 2) ? split.First() : null;
        string field = split.Last();
        
        // Get a subset of the points which match the key, and are ordered by time ascending
        var thesepoints = points.Where(x => x.__Component == component && x.__Field == field).OrderBy(x => x.__Time).Select(x=>( x.__Time.ToString(timeformat), Convert.ToInt32(x.__Value) ));

        return ChartConfig.CreateBarChart(thesepoints, palette );
    }

    public static ChartConfig CreateLineChart(IEnumerable<Models.Datapoint> points, string key, string timeformat)
    {
        // Decompose the key into component and field parts
        var split = key.Split('/');
        if (split.Length > 2)
            throw new ArgumentException($"Unexpected key segments. Allowed=1 or 2. Found = {split.Length}");

        string? component = (split.Length == 2) ? split.First() : null;
        string field = split.Last();
        
        // Get a subset of the points which match the key, and are ordered by time ascending
        var thesepoints = points.Where(x => x.__Component == component && x.__Field == field).OrderBy(x => x.__Time);

        // Transform them into how the chart config creator wants to see them
        var labels = thesepoints.Select(x => x.__Time.ToString(timeformat));
        var data = thesepoints.Select(x => Convert.ToInt32(x.__Value)).AsEnumerable<int>();
        var series = new[] { (key, data) };

        return ChartConfig.CreateLineChart(labels, series, palette );
    }

    public static ChartConfig CreateMultiLineChart(IEnumerable<Models.Datapoint> points, IEnumerable<string> keys, string timeformat)
    {
        var series = new List<(string Label, IEnumerable<int> Data)>();
        IEnumerable<string> labels = Enumerable.Empty<string>();

        // For each line...
        foreach(var key in keys)
        {
            // Decompose the key into component and field parts
            var split = key.Split('/');
            if (split.Length > 2)
                throw new ArgumentException($"Unexpected key segments. Allowed=1 or 2. Found = {split.Length}");

            string? component = (split.Length == 2) ? split.First() : null;
            string field = split.Last();

            // Get a subset of the points which match the key, and are ordered by time ascending
            var thesepoints = points.Where(x => x.__Component == component && x.__Field == field).OrderBy(x => x.__Time);
            var data = thesepoints.Select(x => Convert.ToInt32(x.__Value)).AsEnumerable<int>();
            series.Add( (key, data) );

            // Transform them into how the chart config creator wants to see them
            labels = thesepoints.Select(x => x.__Time.ToString(timeformat));

            // TODO: This does NOT deal with the series not having identical labels.
            // That needs to be fixed.
        }
        
        return ChartConfig.CreateLineChart(labels, series, palette );
    }

    public static ChartConfig CreateMultiDeviceBarChart(IEnumerable<Models.Datapoint> points, IEnumerable<string> keys)
    {
        // Labels are the devices
        var labels = points.GroupBy(x=>x.__Device).Select(x => x.Key).OrderBy(x=>x);

        // One series per key
        var series = new List<(string Label, IEnumerable<int> Data)>();
        foreach(var key in keys)
        {
            // Decompose the key into component and field parts
            var split = key.Split('/');
            if (split.Length > 2)
                throw new ArgumentException($"Unexpected key segments. Allowed=1 or 2. Found = {split.Length}");

            string? component = (split.Length == 2) ? split.First() : null;
            string field = split.Last();

            // Each data point is a device
            var data = points.Where(x => x.__Component == component && x.__Field == field).OrderBy(x => x.__Device).Select(x => Convert.ToInt32(x.__Value)).AsEnumerable<int>();
            series.Add( (key, data) );
        }
        return ChartConfig.CreateMultiBarChart(labels, series, palette);
    }

    private static readonly ChartColor[] palette = new ChartColor[]
    {
        new ChartColor("540D6E"),
        new ChartColor("EE4266"),
        new ChartColor("FFD23F"),
        new ChartColor("875D5A"),
        new ChartColor("FFD3DA"),
        new ChartColor("8EE3EF"),
        new ChartColor("7A918D"),
    };

}
