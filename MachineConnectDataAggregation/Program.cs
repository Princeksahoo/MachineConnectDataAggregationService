using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;

namespace TPMMODetailsUpdation
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main()
        {

#if (!DEBUG)

            CultureInfo culture = CultureInfo.CreateSpecificCulture("en");
            CultureInfo.DefaultThreadCurrentCulture = culture;
            Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            ServiceBase[] ServicesToRun;
            ServicesToRun = new ServiceBase[] { new MachineConnectDataAggregationService() };
            ServiceBase.Run(ServicesToRun);
#else

            MachineConnectDataAggregationService service = new MachineConnectDataAggregationService();
            service.StartDebug();
          
            System.Threading.Thread.Sleep(System.Threading.Timeout.Infinite);
#endif

        }
    }
}
