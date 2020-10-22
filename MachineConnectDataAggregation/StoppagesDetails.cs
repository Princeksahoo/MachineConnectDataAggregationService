using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachineConnectDataAggregationService
{
    class StoppagesDetails
    {
        public DateTime? Fromtime { get; set; }
        public DateTime? ToTime { get; set; }
        public string Duration { get; set; }
    }
}
