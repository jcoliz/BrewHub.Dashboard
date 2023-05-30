using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardIoT.Core.Interfaces
{
    public enum LevelSeverityEnum { Invalid = 0, Warning, Error, OutOfRange }

    public enum LevelSideEnum { Invalid = 0, High, Low }

    public interface ILevel
    {
        IMetric Metric { get; }

        double Value { get; }

        LevelSeverityEnum Severity { get; }
        LevelSideEnum Side { get; }
    }
}
