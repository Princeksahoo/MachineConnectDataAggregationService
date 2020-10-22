using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachineConnectDataAggregationService
{
    class Chart<T>
    {
        public List<string> categories { get; set; }
        public List<T> series { get; set; }
    }
}
