using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachineConnectDataAggregationService
{
    class TimesChartData
    {
        public string ShiftName { get; set; }
        public int ShiftId { get; set; }
        public Chart<TimeDataSeries> ChartData { get; set; }
    }
}
