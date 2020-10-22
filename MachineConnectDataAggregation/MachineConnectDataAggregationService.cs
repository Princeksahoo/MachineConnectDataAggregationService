using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Configuration;
using System.IO;
using System.Reflection;
using System.Security.Permissions;
using Newtonsoft.Json;
using System.Web;
using System.Web.Configuration;
using Newtonsoft.Json.Serialization;
using System.Timers;
using System.Globalization;
using MachineConnectDataAggregationService;
using System.Xml;
using System.Net.Http;
using System.Threading.Tasks;
using System.Runtime.Remoting;

namespace TPMMODetailsUpdation
{
    partial class MachineConnectDataAggregationService : ServiceBase
    {
        List<Thread> threads = new List<Thread>();

        string appPath = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
        string exportFileName = "";
        private readonly object padlock = new object();
        private readonly object padlockRuntimeChartAgg = new object();
        private volatile bool stopping = false;
        string timeIntervalToProcessFile = string.Empty;
        HttpClient client = new HttpClient();

        public MachineConnectDataAggregationService()
        {
            InitializeComponent();
        }

        [SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.ControlAppDomain)]
        protected override void OnStart(string[] args)
        {
            System.Threading.Thread.CurrentThread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
            AppDomain.CurrentDomain.UnhandledException += new UnhandledExceptionEventHandler(CurrentDomain_UnhandledException);

            Thread.CurrentThread.Name = "ServiceThread";
            if (!Directory.Exists(appPath + "\\Logs\\"))
            {
                Directory.CreateDirectory(appPath + "\\Logs\\");
            }
            try
            {
                ThreadStart job = new ThreadStart(DataAggregationService);              
                Thread thread = new Thread(job);
                thread.Name = "DataAggregationService";
                thread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                thread.Start();
                threads.Add(thread);
                Logger.WriteDebugLog("Service Started Successfully");
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog(ex.ToString());
            }

            try
            {
                ThreadStart job = new ThreadStart(RuntimeChartDataAggregationService);
                Thread thread = new Thread(job);
                thread.Name = "RuntimeChartDataAggregationService";
                thread.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                thread.Start();
                threads.Add(thread);
                Logger.WriteDebugLog("Service Started Successfully");
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog(ex.ToString());
            }

        }

        void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs args)
        {
            Exception e = args.ExceptionObject as Exception;
            Logger.WriteErrorLog("UnhandledException caught : " + e.ToString());
            Logger.WriteErrorLog("Runtime terminating:" + args.IsTerminating);
            Logger.WriteErrorLog(args.ToString());
        }

        protected override void OnStop()
        {
            if (string.IsNullOrEmpty(Thread.CurrentThread.Name))
            {
                Thread.CurrentThread.Name = "ServiceThread";
            }
            stopping = true;
            lock (padlock)
            {
                Monitor.Pulse(padlock);
            }
            lock (padlockRuntimeChartAgg)
            {
                Monitor.Pulse(padlockRuntimeChartAgg);
            }
            Thread.SpinWait(60000 * 10);
            try
            {
                Logger.WriteDebugLog("Service Stop request has come!!! ");
                Logger.WriteDebugLog("Thread count is: " + threads.Count.ToString());
                foreach (Thread thread in threads)
                {
                    Logger.WriteDebugLog("Stopping the thread - " + thread.Name);
                    thread.Abort();
                }
                threads.Clear();
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog(ex.Message);
            }
            Logger.WriteDebugLog("Service has stopped.");
            // TODO: Add code here to perform any tear-down necessary to stop your service.
        }

        internal void StartDebug()
        {
            OnStart(null);
        }

        private void DataAggregationService()
        {
            Logger.WriteDebugLog(string.Format("{0} thread started file Processing.", Thread.CurrentThread.Name.ToString()));

            timeIntervalToProcessFile = ConfigurationManager.AppSettings["TimeInterval"].ToString();
            int timeIntervalInMin = 0;
            int.TryParse(timeIntervalToProcessFile, out timeIntervalInMin);

            if (timeIntervalInMin <= 1) timeIntervalInMin = 1;


            DateTime LastAggDate = DatabaseAccess.lastAggDate();
            DateTime LogicalDayStart = DatabaseAccess.GetLogicalDayStart(LastAggDate);

            if (!string.IsNullOrEmpty(CloudWebAPIURLToUploadFile) && CloudWebAPIURLToUploadFile.Length>1)
            {
                client.BaseAddress = new Uri(CloudWebAPIURLToUploadFile);
                client.DefaultRequestHeaders.Accept.Clear();
                client.DefaultRequestHeaders.Add("ClientID", "Spoorthi"); //TODO                      
            }
            

            try
            {
                GetAlarmSolutionDataData();
            }
            catch { }

            while (!stopping)
            {
                try
                {
                    if (DateTime.Now.Minute % 10 == 0 || DateTime.Now.Minute % 11 == 0 || DateTime.Now.Minute % 12 == 0)
                    {
                        GetAlarmSolutionDataData();
                    }
                }
                catch (Exception exx)
                {
                    Logger.WriteErrorLog(exx.ToString());
                }

                try
                {                                   
                    while (LogicalDayStart <= DateTime.Now)
                    {
                        DatabaseAccess.ExecuteProc(LogicalDayStart);
                        //to do call new proc pass logical end date-datetime to method only
                        //if(string.IsNullOrEmpty(IISDataFolderPath) == false)
                        GetMachineConnectData(LogicalDayStart);
                        LogicalDayStart = LogicalDayStart.AddDays(1);
                    }
                    //LogicalDayStart = DateTime.Now; DatabaseAccess.GetLogicalDayStart(LastAggDate);       
                    LogicalDayStart = DatabaseAccess.GetLogicalDayStart(DateTime.Now);       
                }
                catch (Exception exx)
                {
                    Logger.WriteErrorLog(exx.ToString());
                }
                finally
                {
                    lock (padlock)
                    {
                        Monitor.Wait(padlock, TimeSpan.FromMinutes(timeIntervalInMin));
                    }
                }
            }
        }

        #region Done by Prince - Aggregation InsertRuntimeChartData
        private void RuntimeChartDataAggregationService()
        {
            Logger.WriteDebugLog(string.Format("{0} thread started Processing.", Thread.CurrentThread.Name.ToString()));

            timeIntervalToProcessFile = ConfigurationManager.AppSettings["TimeIntervalForRuntimeChartDataAggregation"].ToString();
            int timeIntervalInMin = 0;
            int.TryParse(timeIntervalToProcessFile, out timeIntervalInMin);
           
            if (timeIntervalInMin <= 1) timeIntervalInMin = 1;

            DateTime LastAggDate = DatabaseAccess.lastRuntimeAggDate();
            DateTime LogicalDayStart = DatabaseAccess.GetLogicalDayStart(LastAggDate);

            while (!stopping)
            {
                try
                {
                    while (LogicalDayStart <= DateTime.Now)
                    {

                        DatabaseAccess.ExecuteRuntimeChartDataProc(LogicalDayStart);
                        LogicalDayStart = LogicalDayStart.AddDays(1);
                    }
                    LogicalDayStart = DatabaseAccess.GetLogicalDayStart(DateTime.Now);
                }
                catch (Exception exx)
                {
                    Logger.WriteErrorLog(exx.ToString());
                }
                finally
                {
                    lock (padlockRuntimeChartAgg)
                    {
                        Monitor.Wait(padlockRuntimeChartAgg, TimeSpan.FromMinutes(timeIntervalInMin));
                    }
                }
            }
        }
        #endregion

        #region -------------Path of Service---------------
        static public string IISDataFolderPath
        {
            get
            {              
                return ConfigurationManager.AppSettings["IISDataFolderPath"].ToString();
            }
        }

        static public string CloudWebAPIURLToUploadFile
        {
            get
            {
                return string.IsNullOrEmpty(ConfigurationManager.AppSettings["CloudWebAPIURLToUploadFile"].ToString()) ? "" : ConfigurationManager.AppSettings["CloudWebAPIURLToUploadFile"].ToString().Trim(new char[] { '/' }) + "/";
            }
        }
        #endregion

        public void GetMachineConnectData(DateTime date)
        {
            appData appdata = new appData();
            appdata = DatabaseAccess.GetMachineConnectData(date);
            try
            {
                if (appdata.PlantMachine.Count > 0)
                {
                    string json = JsonConvert.SerializeObject(appdata,
                                           new JsonSerializerSettings
                                           {
                                               ContractResolver = new CamelCasePropertyNamesContractResolver()
                                           });
                    var dtname = date.ToString("yyyy-MM-dd");


                    if (!string.IsNullOrEmpty(IISDataFolderPath))
                    {
                        if (!Directory.Exists(IISDataFolderPath))
                        {
                            try
                            {
                                Directory.CreateDirectory(IISDataFolderPath);
                            }
                            catch (Exception exx)
                            {
                                Logger.WriteErrorLog(exx.ToString());
                            }
                        }
                        if (Directory.Exists(IISDataFolderPath))
                        {
                            File.WriteAllText(Path.Combine(IISDataFolderPath, dtname + ".json"), json);
                        }
                    }
                    if (!string.IsNullOrEmpty(CloudWebAPIURLToUploadFile))
                    {
                        if (!Directory.Exists(Path.Combine(appPath, "Data")))
                        {
                            Directory.CreateDirectory(Path.Combine(appPath, "Data"));
                        }
                        var filePath = Path.Combine(appPath, "data", dtname + ".json");

                        File.WriteAllText(filePath, json);
                        Upload(Path.Combine(filePath));
                    }

                    //////////////////
                    //if (!Directory.Exists(IISDataFolderPath))
                    //{
                    //    Directory.CreateDirectory(IISDataFolderPath);
                    //}
                    //File.WriteAllText(Path.Combine(IISDataFolderPath, dtname + ".json"), json);
                }
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog(ex.ToString());               
            }
        }

        public void Upload(string fileName)
        {
            ByteArrayContent fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(fileName));
            var response = client.PostAsync("API/UploadJsonFile?fileName=" + Path.GetFileName(fileName), fileContent).Result;
            if (response.IsSuccessStatusCode)
            {
                Logger.WriteDebugLog(response.StatusCode.ToString() + ". Successfully uploaded" + Path.GetFileName(fileName));
            }
            else
            {
                Logger.WriteDebugLog(response.ToString() + ". Upload Failed...." + Path.GetFileName(fileName));
            }
        }

        public void GetAlarmSolutionDataData()
        {
            
            try
            {
                List<AlarmSolution> empList = DatabaseAccess.GetAlarmSolutionData();
                if (empList.Count > 0)
                {
                    string json = JsonConvert.SerializeObject(empList,
                                           new JsonSerializerSettings
                                           {
                                               ContractResolver = new CamelCasePropertyNamesContractResolver()
                                           });

                    if (!string.IsNullOrEmpty(CloudWebAPIURLToUploadFile))
                    {
                        if (!Directory.Exists(Path.Combine(appPath, "Data")))
                        {
                            Directory.CreateDirectory(Path.Combine(appPath, "Data"));
                        }
                        var filePath = Path.Combine(appPath, "Data", "AlarmsMaster.json");

                        File.WriteAllText(filePath, json);
                        UploadAlarmSolutionDataDetails(Path.Combine(filePath));
                    }

                }
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog(ex.ToString());

            }

        }

        public void UploadAlarmSolutionDataDetails(string fileName)
        {

            ByteArrayContent fileContent = new ByteArrayContent(System.IO.File.ReadAllBytes(fileName));
            var response = client.PostAsync("API/UploadAlarmSolutionData?fileName=" + Path.GetFileName(fileName), fileContent).Result;
            if (response.IsSuccessStatusCode)
            {
                Logger.WriteDebugLog(response.StatusCode.ToString() + ". AlarmSolutionData Successfully uploaded" + Path.GetFileName(fileName));
            }
            else
            {
                Logger.WriteDebugLog(response.ToString() + ". AlarmSolutionData Upload Failed...." + Path.GetFileName(fileName));
            }
        }

    }
}
