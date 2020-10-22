using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachineConnectDataAggregationService
{
    class Machine
    {
        public int MachineID { get; set; }
        public string MachineName { get; set; }
        public int OEE { get; set; }
        public string Downtime { get; set; }
        public int PartsCount { get; set; }
        public string LastProgram { get; set; }
        public string Status { get; set; }
        public string Color { get; set; }

        public string MachineMTB { get; set; }
    }
}
