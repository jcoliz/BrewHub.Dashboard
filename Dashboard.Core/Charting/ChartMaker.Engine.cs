// Copyright (C) 2023 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

using Common.ChartJS;

namespace BrewHub.Dashboard.Core.Charting;

public static class ChartMaker
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

        var rawpoints = new Dictionary<string, IEnumerable<Models.Datapoint>>();

        // Collect raw points for each line we want to show...
        foreach(var key in keys)
        {
            // Decompose the key into component and field parts
            var split = key.Split('/');
            if (split.Length > 2)
                throw new ArgumentException($"Unexpected key segments. Allowed=1 or 2. Found = {split.Length}");

            string? component = (split.Length == 2) ? split.First() : null;
            string field = split.Last();

            // Get a subset of the points which match the key, and are ordered by time ascending
            rawpoints[key] = points.Where(x => x.__Component == component && x.__Field == field).OrderBy(x => x.__Time);
        }

        // Bug 1613: Charting: Handle cases where series have different time series
        // Determine the complete set of time points in this domain (touched by at least one key)
        var slices = rawpoints.SelectMany(x => x.Value).Select(x => x.__Time).Distinct().OrderBy(x=>x);

        // We can make the labels out of this
        var labels = slices.Select(x => x.ToString(timeformat));

        // Create each series, ensuring that there is one datapoint for every time slice
        foreach(var key in keys)
        {
            var keypoints = rawpoints[key];
            var iterator = keypoints.GetEnumerator();
            iterator.MoveNext();

            int lastval = 0;
            var datapoints = new List<int>();

            foreach(var slice in slices)
            {
                // We're going to make one data points for each slice value, copying the previous value
                // if it's not there

                if (iterator == null)
                {
                    // No more items, use previous
                    datapoints.Add(lastval);
                }
                else if (iterator.Current.__Time == slice)
                {
                    // Exact match!
                    // Use it, and advance
                    lastval = Convert.ToInt32(iterator.Current.__Value);
                    datapoints.Add(lastval);
                    iterator.MoveNext();
                }
                else if (iterator.Current.__Time > slice)
                {
                    // We're asking for a time value which is missing
                    // Fill in, and don't advance
                    datapoints.Add(lastval);
                }
                else
                {
                    // We're asking for a time value which is BEFORE the
                    // current. This should never happen
                    datapoints.Add(lastval);
                }
            }

            // Add the series
            series.Add( (key, datapoints) );
        }

        return ChartConfig.CreateLineChart(labels, series, palette );
    }

    public static ChartConfig CreateMultiLineChartForSingleComponent(IEnumerable<Models.Datapoint> points, IEnumerable<string> keys, string timeformat)
    {
        var series = new List<(string Label, IEnumerable<int> Data)>();
        IEnumerable<string> labels = Enumerable.Empty<string>();

        // For each line...
        foreach(var key in keys)
        {
            // Get a subset of the points which match the key, and are ordered by time ascending
            var thesepoints = points.Where(x => x.__Field == key).OrderBy(x => x.__Time);
            var data = thesepoints.Select(x => Convert.ToInt32(x.__Value)).AsEnumerable<int>();
            series.Add( (key, data) );

            // Transform them into how the chart config creator wants to see them
            labels = thesepoints.Select(x => x.__Time.ToString(timeformat));

            // TODO: Bug 1613: Charting: Handle cases where series have different time series
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

            // Problem here comes when there are some series that are only on some devices.
            // We need to give one data point per device, no matter how many we actually have.

            var data = labels.Select(d => points.Where(x => x.__Component == component && x.__Field == field && x.__Device == d).Select(x => Convert.ToInt32(x.__Value) ).FirstOrDefault() );

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
