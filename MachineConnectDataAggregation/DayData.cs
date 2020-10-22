using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachineConnectDataAggregationService
{
    class DayData
    {
        public DateTime DataDate { get; set; }
        public int PlantID { get; set; }
        public string PlantName { get; set; }
        public int MachineID { get; set; }
        public string MachineName { get; set; }

        public DashboardData DashboardData { get; set; }
        public List<PartsCountData> PartsCountData { get; set; }
        public List<TimesData> TimesData { get; set; }
        public List<StoppagesData> StoppagesData { get; set; }
        public List<AlarmData> AlarmsSummary { get; set; }
        public List<AlarmData> AlarmsDetails { get; set; }
        public List<TimesChartData> TimesChartData { get; set; }
        public List<PartCountChartData> PartCountChartData { get; set; }
        public List<AlarmSolution> AlarmsSolution { get; set; }
    }
}
