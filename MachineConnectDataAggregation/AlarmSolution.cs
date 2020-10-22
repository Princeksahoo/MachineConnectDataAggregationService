using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachineConnectDataAggregationService
{
    class AlarmSolution
    {
        public int AlarmNo { get; set; }
        public int SlNo { get; set; }
        public string Cause { get; set; }
        public string Solution { get; set; }
        public string ImageName { get; set; }

        public string MTB { get; set; }

        public string Description { get; set; }
    }
}
