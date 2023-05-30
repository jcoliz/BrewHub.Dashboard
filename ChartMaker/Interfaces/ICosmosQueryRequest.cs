namespace ChartMaker.CosmosQuery;

public interface ICosmosQueryRequest
{
    /// <summary>
    /// Which devices to include in the query, or NULL for include ALL devices
    /// </summary>
    IEnumerable<string>? Devices { get; }

    /// <summary>
    /// How far back to start with the data
    /// </summary>
    TimeSpan LookBack { get; }

    /// <summary>
    /// How much time should be included in each 'bin' of collected data
    /// </summary>
    TimeSpan BinInterval { get; }
}