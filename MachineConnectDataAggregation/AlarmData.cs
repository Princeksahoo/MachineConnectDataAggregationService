using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachineConnectDataAggregationService
{
    class AlarmData
    {
        public string ShiftName { get; set; }
        public int ShiftId { get; set; }
        public int shiftCount { get; set; }
        public string TotalDuration { get; set; }
        public List<AlarmDetails> details = new List<AlarmDetails>();
    }
}
