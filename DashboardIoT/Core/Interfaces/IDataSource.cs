using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardIoT.Core.Interfaces
{
    public interface IDataSource
    {
        public Task<IEnumerable<IReading>> GetMomentaryReadingsAsync(string site);
        public Task<IEnumerable<IReading>> GetSeriesReadingsAsync(string site, TimeSpan span, int divisions);
    }
}
