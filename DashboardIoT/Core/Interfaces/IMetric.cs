using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardIoT.Core.Interfaces
{
    public interface IMetric
    {
        string Node { get; }
        string Device { get; }
        string Name { get; }
        string Label { get; }
        string Units { get; }
    }
}
