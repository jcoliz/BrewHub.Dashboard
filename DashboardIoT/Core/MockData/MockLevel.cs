using DashboardIoT.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardIoT.Core.MockData
{
    public class MockLevel : ILevel
    {
        public IMetric Metric { get; set; }

        public double Value { get; set; }

        public LevelSeverityEnum Severity { get; set; }

        public LevelSideEnum Side { get; set; }
    }
}
