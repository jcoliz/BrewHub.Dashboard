using DashboardIoT.Core.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DashboardIoT.Core.MockData
{
    public class MockReading: IReading
    {
        public MockReading()
        {

        }

        public MockReading(IMetric copy)
        {
            Segments = new string[] { string.Empty, copy.Node, copy.Device, copy.Name };
            Units = copy.Units;
        }

        public string Topic
        {
            get
            {
                return string.Join('/', Segments);
            }
            set
            {
                Segments = value.Split('/');
            }
        }
        private string[] Segments;

        public string Name => Segments[4];
        public string Node => Segments[2];
        public string Device => Segments[3];
        public double Last 
        { 
            get
            {
                return Raw + Adjustment;
            }
            set
            {
                Raw = value - Adjustment;
            }
        }

        public double Raw { get; private set; }

        public double Adjustment { get; set; }

        public string Units { get; set; }

        public string Label => $"{Node} {Device} {Name}";

        public IEnumerable<double> Values { get; set; }
    }
}
