// Copyright (C) 2023 James Coliz, Jr. <jcoliz@outlook.com> All rights reserved
// Use of this source code is governed by the MIT license (see LICENSE file)

using Common.ChartJS;

namespace BrewHub.Dashboard.Core.Charting;

public static class ChartMaker
{
    public static ChartConfig CreateMultiLineChart(IEnumerable<Models.Datapoint> points, string timeformat)
    {
        var series = new List<(string Label, IEnumerable<int> Data)>();

        // Organize the data into a dictionary with one entry per series
        var rawpoints = points
            .Select(x => (x.__Component,x.__Field))
            .Distinct()
            .ToDictionary(
                key=>key.__Component is not null ? $"{key.__Component}/{key.__Field}" : key.__Field,
                key=>points.Where(x => x.__Component == key.__Component && x.__Field == key.__Field).OrderBy(x => x.__Time)
            );

        // Bug 1613: Charting: Handle cases where series have different time series
        // Determine the complete set of time points in this domain (touched by at least one key)
        var slices = rawpoints.SelectMany(x => x.Value).Select(x => x.__Time).Distinct().OrderBy(x=>x);

        // We can make the labels out of this
        var labels = slices.Select(x => x.ToString(timeformat));

        // Create each series, ensuring that there is one datapoint for every time slice
        foreach(var s in rawpoints)
        {
            var iterator = s.Value.GetEnumerator();
            iterator.MoveNext();

            int lastval = 0;
            var datapoints = new List<int>();

            foreach(var slice in slices)
            {
                // We're going to make one data point for each slice value, copying the previous value
                // if it's not there

                if (iterator?.Current.__Time == slice)
                {
                    // Exact match!
                    // Use it, and advance
                    lastval = Convert.ToInt32(iterator.Current.__Value);
                    iterator.MoveNext();
                }
                datapoints.Add(lastval);
            }

            // Add the series
            series.Add( (s.Key, datapoints) );
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
