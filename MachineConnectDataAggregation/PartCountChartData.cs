using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachineConnectDataAggregationService
{
    class PartCountChartData
    {
        public string ShiftName { get; set; }
        public int ShiftId { get; set; }
        public Chart<PartsCountSeries> ChartData { get; set; }
    }
}
