using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachineConnectDataAggregationService
{
    class PlantMachines
    {
        public string PlantName { get; set; }
        public int PlantID { get; set; }
        public List<Machine> Machines { get; set; }
    }
}
