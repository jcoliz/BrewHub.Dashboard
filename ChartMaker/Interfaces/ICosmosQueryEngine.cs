using System.IO;

namespace ChartMaker.CosmosQuery;

public interface ICosmosQueryEngine
{
    Task<Stream> DoQueryAsync(TimeSpan LookBack, TimeSpan BinInterval, IEnumerable<string> Devices);
}