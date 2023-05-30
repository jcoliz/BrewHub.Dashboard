using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardIoT.Core.Interfaces
{
    public interface IReading: IMetric
    {
        public IEnumerable<double> Values { get; }
        public double Last { get; }
        public double Adjustment { get; }
    }
}
