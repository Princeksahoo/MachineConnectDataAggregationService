using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachineConnectDataAggregationService
{
    class DashboardData
    {

        public string PartCount { get; set; }
        public string DownTime { get; set; }
        public List<DashboardDetails> DashboardDetails { get; set; }
    }
}
