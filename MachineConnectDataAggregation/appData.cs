using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MachineConnectDataAggregationService
{
    class appData
    {
        public List<PlantMachines> PlantMachine { get; set; }
        public List<Shift> Shifts { get; set; }
        public List<AppDataList> appDataList { get; set; }

        public static List<AlarmSolution> AlarmsSolution { get; set; }
    }
}
