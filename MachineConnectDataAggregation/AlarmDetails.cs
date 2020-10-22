using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachineConnectDataAggregationService
{
    class AlarmDetails
    {
        public int alarmNo { get; set; }
        public string Message { get; set; }
        public DateTime? StartTime { get; set; }
        public DateTime? EndTime { get; set; }
        public string Duration { get; set; }
        public DateTime? LastSeen { get; set; }
        public int NoOfOcc { get; set; }
    }
}
