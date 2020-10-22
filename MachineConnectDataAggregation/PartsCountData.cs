using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachineConnectDataAggregationService
{
    class PartsCountData
    {
        public string ShiftName { get; set; }
        public int ShiftId { get; set; }
        public List<PartsCountDetails> details = new List<PartsCountDetails>();
    }
}
