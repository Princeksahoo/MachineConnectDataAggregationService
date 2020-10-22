using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachineConnectDataAggregationService
{
    class StoppagesData
    {
        public string ShiftName { get; set; }
        public int ShiftId { get; set; }
        public List<StoppagesDetails> details = new List<StoppagesDetails>();
    }
}
