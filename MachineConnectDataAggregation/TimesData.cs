using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachineConnectDataAggregationService
{
    class TimesData
    {
        public string ShiftName { get; set; }
        public int ShiftId { get; set; }
        //---Total Count Data-----
        public string TotalPOT { get; set; }
        public string TotalOT { get; set; }
        public string TotalCT { get; set; }
        public List<TimesDetails> details = new List<TimesDetails>();
    }
}
