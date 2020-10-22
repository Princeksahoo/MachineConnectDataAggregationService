using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using System.Data.SqlClient;
using System.Data;
using System.Reflection;
using System.Threading;
using MachineConnectDataAggregationService;
using System.Linq;

namespace TPMMODetailsUpdation
{
    public static class DatabaseAccess
    {
        public static void ExecuteProc(DateTime lastaggDate)
        {
            SqlConnection conn = ConnectionManager.GetConnection();
            SqlCommand cmd = new SqlCommand("[FocasWeb_InsertShift&HourwiseSummary]", conn);
            cmd.Parameters.AddWithValue("@StartDate", lastaggDate.ToString("yyyy-MM-dd HH:mm:ss"));
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.CommandTimeout = 600;
            try
            {
                Logger.WriteDebugLog("Executing proc \"FocasWeb_InsertShift&HourwiseSummary\" " + " for Date = " + lastaggDate.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
                Logger.WriteDebugLog("Completed Executing proc \"FocasWeb_InsertShift&HourwiseSummary\" ");
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog(ex.Message);
            }
            finally
            {
                if (conn != null) conn.Close();
            }

        }
        internal static appData GetMachineConnectData(DateTime dateVal)
        {
            SqlConnection conn = ConnectionManager.GetConnection();
            DataTable dt = new DataTable();
            DataTable dtMachine = new DataTable();
            DataTable dtDashboard = new DataTable();
            DataTable dtPartCount = new DataTable();
            DataTable dtTimes = new DataTable();
            DataTable dtStoppages = new DataTable();
            DataTable dtAlarmsDatails = new DataTable();
            DataTable dtAlarmsSummary = new DataTable();
            DataTable dtAlarmsSolution = new DataTable();

            appData appData = new appData();
            appData.Shifts = new List<Shift>();
            appData.PlantMachine = new List<PlantMachines>();
            appData.AlarmsSolution = new List<AlarmSolution>();
            var ss = dateVal.ToString("yyyy-MM-dd");
            try
            {
                var cmd = new SqlCommand("[dbo].[FocasWeb_ViewShift&HourwiseSummary]", conn);
                cmd.CommandType = System.Data.CommandType.StoredProcedure;
                cmd.CommandTimeout = 120;
                cmd.Parameters.AddWithValue("@Date", dateVal.ToString("yyyy-MM-dd"));//dateVal);
                SqlDataReader rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        int shiftId = 0;
                        Shift shift = new Shift();
                        if (int.TryParse(rdr["ShiftID"].ToString(), out shiftId))
                            shift.ShiftId = shiftId;
                        shift.ShiftName = rdr["ShiftName"].ToString();
                        appData.Shifts.Add(shift);
                    }
                    rdr.NextResult();
                    while (rdr.Read())
                    {
                        int plantId = 0;
                        PlantMachines plantMachines = new PlantMachines();
                        if (int.TryParse(rdr["PlantCode"].ToString(), out plantId))
                            plantMachines.PlantID = plantId;
                        plantMachines.PlantName = rdr["Plantid"].ToString();
                        appData.PlantMachine.Add(plantMachines);
                    }
                    rdr.NextResult();

                    //--MachineInformation---
                    dtMachine.Load(rdr);
                    //---Dashboard Information-----                 
                    dtDashboard.Load(rdr);
                    //-----PartCount Information---
                    dtPartCount.Load(rdr);

                    dtTimes.Load(rdr);

                    dtStoppages.Load(rdr);

                    dtAlarmsDatails.Load(rdr);

                    dtAlarmsSummary.Load(rdr);

                    //dtAlarmsSolution.Load(rdr);

                    if (dtMachine.Rows.Count > 0)
                    {
                        for (int i = 0; i < appData.PlantMachine.Count; i++)
                        {
                            appData.PlantMachine[i].Machines = new List<Machine>();
                            foreach (DataRow item in dtMachine.Select("PlantID = '" + appData.PlantMachine[i].PlantName + "'"))
                            {
                                int machineId = 0;
                                Machine machine = new Machine();
                                if (int.TryParse(item["MCInterface"].ToString(), out machineId))
                                    machine.MachineID = machineId;//Convert.ToInt32(item["MCInterface"].ToString());
                                machine.MachineName = item["MachineId"].ToString();
                                machine.LastProgram = item["RunningPart"].ToString();
                                machine.Downtime = item["Stoppages"].ToString();
                                machine.Status = item["McStatus"].ToString();
                                machine.Color = item["color"].ToString();
                                if (int.TryParse(item["PartsCount"].ToString(), out machineId))
                                    machine.PartsCount = machineId;// int.Parse(item["PartsCount"].ToString());

                                machine.MachineMTB = item["MachineMTB"].ToString().ToUpper();

                                appData.PlantMachine[i].Machines.Add(machine);
                            }
                        }

                        appData.appDataList = new List<AppDataList>();
                        AppDataList appdataDay = new AppDataList();
                        appdataDay.Key = dateVal.ToString("yyyy-MM-dd");
                        appdataDay.DayData = new List<DayData>();

                        for (int i = 0; i < appData.PlantMachine.Count; i++)
                        {

                            foreach (Machine item in appData.PlantMachine[i].Machines)
                            {
                                DayData dayData1 = new DayData();
                                dayData1.DataDate = DateTime.Now.Date;
                                dayData1.MachineID = item.MachineID;
                                dayData1.MachineName = item.MachineName;
                                dayData1.PlantID = appData.PlantMachine[i].PlantID;
                                dayData1.PlantName = appData.PlantMachine[i].PlantName;

                                dayData1.PartsCountData = new List<PartsCountData>();
                                dayData1.PartCountChartData = new List<PartCountChartData>();

                                dayData1.TimesData = new List<TimesData>();
                                dayData1.TimesChartData = new List<TimesChartData>();

                                dayData1.StoppagesData = new List<StoppagesData>();
                                dayData1.AlarmsDetails = new List<AlarmData>();
                                dayData1.AlarmsSummary = new List<AlarmData>();

                                dayData1.DashboardData = new DashboardData();
                                dayData1.DashboardData.DashboardDetails = new List<DashboardDetails>();
                                foreach (DataRow item1 in dtDashboard.Select("PlantID = '" + appData.PlantMachine[i].PlantName + "' AND Machineid = '" + item.MachineName + "'"))
                                {
                                    DashboardDetails dashboardDetails = new DashboardDetails();
                                    dashboardDetails.NavId = item1["NavID"].ToString();
                                    dashboardDetails.ColHeaderText = item1["DisplayName"].ToString();

                                    dashboardDetails.ShiftData = new List<string>();
                                    foreach (var shifts in appData.Shifts)
                                    {
                                        dashboardDetails.ShiftData.Add(item1[shifts.ShiftName].ToString());
                                    }

                                    //dashboardDetails.ShiftData.Add(item1["A"].ToString());
                                    //dashboardDetails.ShiftData.Add(item1["B"].ToString());
                                    //dashboardDetails.ShiftData.Add(item1["C"].ToString());


                                    if (item1["NavID"].ToString().Equals("menu.partsCount", StringComparison.OrdinalIgnoreCase))
                                    {
                                        dayData1.DashboardData.PartCount = item1["DayValue"].ToString();
                                    }
                                    else if (item1["NavID"].ToString().Equals("menu.downtime", StringComparison.OrdinalIgnoreCase))
                                    {
                                        dayData1.DashboardData.DownTime = item1["DayValue"].ToString();
                                    }
                                    dayData1.DashboardData.DashboardDetails.Add(dashboardDetails);
                                }

                                dayData1.PartsCountData = new List<PartsCountData>();

                                //for each shift and machine fill the part count data
                                foreach (Shift shift in appData.Shifts)
                                {
                                    PartsCountData partCountData = new PartsCountData();
                                    PartCountChartData partCountChartData = new PartCountChartData();
                                    partCountData.ShiftId = partCountChartData.ShiftId = shift.ShiftId;
                                    partCountData.ShiftName = partCountChartData.ShiftName = shift.ShiftName;

                                    partCountData.details = new List<PartsCountDetails>();
                                    partCountChartData.ChartData = new Chart<PartsCountSeries>();

                                    partCountChartData.ChartData.categories = new List<string>();
                                    partCountChartData.ChartData.series = new List<PartsCountSeries>();

                                    var datarow = dtPartCount.Select("PlantID = '" + appData.PlantMachine[i].PlantName + "' AND Machineid = '" + item.MachineName + "' AND Shiftid = " + shift.ShiftId);

                                    //----for part count chart----START--     
                                    List<string> programs = datarow.Select(r => r["ProgramID"].ToString()).Distinct().ToList();
                                    programs.Remove("");
                                    List<int> id = datarow.Select(rr => Convert.ToInt32(rr["HourID"].ToString())).Distinct().ToList();
                                    foreach (string prog in programs)
                                    {
                                        List<int> dataSeries = new List<int>();
                                        id.ForEach(o => dataSeries.Add(0));//dataSeries.Add(int.MinValue));

                                        PartsCountSeries prog1 = new PartsCountSeries() { name = prog, data = dataSeries };
                                        partCountChartData.ChartData.series.Add(prog1);
                                        foreach (DataRow row in datarow.Where(r => r["ProgramID"].ToString() == prog))
                                        {
                                            int hourId = Int32.Parse(row["HourID"].ToString()) - 1;
                                            dataSeries[hourId] = Int32.Parse(row["PartCount"].ToString());
                                        }
                                    }
                                    //----for part count chart----END-- 
                                    foreach (DataRow item1 in datarow)
                                    {
                                        PartsCountDetails partShift = new PartsCountDetails();
                                        partShift.HourText = Convert.ToDateTime(item1["HourStart"].ToString()).ToString("htt").ToLower() + "-" + Convert.ToDateTime(item1["HourEnd"].ToString()).ToString("htt").ToLower();
                                        partShift.Program = item1["ProgramID"].ToString();
                                        partShift.PartsCount = item1["PartCount"].ToString();
                                        partCountData.details.Add(partShift);

                                        //----for part count chart------     
                                        if (partCountChartData.ChartData.categories.Contains(partShift.HourText) == false)
                                        {
                                            partCountChartData.ChartData.categories.Add(partShift.HourText);
                                        }
                                    }
                                    dayData1.PartsCountData.Add(partCountData);
                                    dayData1.PartCountChartData.Add(partCountChartData);
                                }

                                dayData1.TimesData = new List<TimesData>();
                                //for each shift and machine fill the times data
                                foreach (Shift shift in appData.Shifts)
                                {
                                    TimesData timeData = new TimesData();
                                    TimesChartData timeChart = new TimesChartData();
                                    timeData.ShiftId = timeChart.ShiftId = shift.ShiftId;
                                    timeData.ShiftName = timeChart.ShiftName = shift.ShiftName;

                                    timeData.details = new List<TimesDetails>();

                                    timeChart.ChartData = new Chart<TimeDataSeries>();
                                    TimeDataSeries power = new TimeDataSeries() { name = "Power On Time", data = new List<int>() };
                                    TimeDataSeries operating = new TimeDataSeries() { name = "Operating Time", data = new List<int>() };
                                    TimeDataSeries cutting = new TimeDataSeries() { name = "CuttingTime", data = new List<int>() };

                                    timeChart.ChartData.categories = new List<string>();
                                    timeChart.ChartData.series = new List<TimeDataSeries>();
                                    timeChart.ChartData.series.Add(power);
                                    timeChart.ChartData.series.Add(operating);
                                    timeChart.ChartData.series.Add(cutting);



                                    foreach (DataRow item1 in dtTimes.Select("PlantID = '" + appData.PlantMachine[i].PlantName + "' AND Machineid = '" + item.MachineName + "' AND Shiftid = " + shift.ShiftId))
                                    {
                                        TimesDetails timeShift = new TimesDetails();

                                        timeShift.HourText = Convert.ToDateTime(item1["HourStart"].ToString()).ToString("htt").ToLower() + "-" + Convert.ToDateTime(item1["HourEnd"].ToString()).ToString("htt").ToLower();

                                        timeShift.PowerNo = item1["PowerOntime"].ToString();
                                        timeShift.Operating = item1["OperatingTime"].ToString();
                                        timeShift.Cutting = item1["CuttingTime"].ToString();
                                        timeData.TotalPOT = item1["TotalPOT"].ToString();
                                        timeData.TotalOT = item1["TotalOT"].ToString();
                                        timeData.TotalCT = item1["TotalCT"].ToString();
                                        timeData.details.Add(timeShift);

                                        //for chart
                                        timeChart.ChartData.categories.Add(timeShift.HourText);
                                        power.data.Add(int.Parse(item1["PowerOntimeInt"].ToString()));
                                        operating.data.Add(int.Parse(item1["OperatingTimeInt"].ToString()));
                                        cutting.data.Add(int.Parse(item1["CuttingTimeInt"].ToString()));

                                    }
                                    dayData1.TimesData.Add(timeData);
                                    dayData1.TimesChartData.Add(timeChart);
                                }

                                dayData1.StoppagesData = new List<StoppagesData>();
                                //for each shift and machine fill the stoppages data
                                foreach (Shift shift in appData.Shifts)
                                {
                                    StoppagesData stoppageData = new StoppagesData();
                                    stoppageData.ShiftId = shift.ShiftId;
                                    stoppageData.ShiftName = shift.ShiftName;
                                    stoppageData.details = new List<StoppagesDetails>();
                                    foreach (DataRow item1 in dtStoppages.Select("PlantID = '" + appData.PlantMachine[i].PlantName + "' AND Machineid = '" + item.MachineName + "' AND Shiftid = " + shift.ShiftId))
                                    {

                                        StoppagesDetails stoppageShift = new StoppagesDetails();
                                        if (string.IsNullOrEmpty(item1["BatchStart"].ToString()))
                                            stoppageShift.Fromtime = null;// Convert.ToDateTime(item1["BatchStart"].ToString());
                                        else
                                            stoppageShift.Fromtime = Convert.ToDateTime(item1["BatchStart"].ToString());
                                        if (string.IsNullOrEmpty(item1["BatchEnd"].ToString()))
                                            stoppageShift.ToTime = null;// Convert.ToDateTime(item1["BatchEnd"].ToString());
                                        else
                                            stoppageShift.ToTime = Convert.ToDateTime(item1["BatchEnd"].ToString());
                                        stoppageShift.Duration = item1["Stoppagetime"].ToString();
                                        stoppageData.details.Add(stoppageShift);
                                    }
                                    dayData1.StoppagesData.Add(stoppageData);
                                }

                                //-----for each Alarm Details--------------------
                                foreach (Shift shift in appData.Shifts)
                                {
                                    AlarmData alarmData = new AlarmData();
                                    alarmData.ShiftId = shift.ShiftId;
                                    alarmData.ShiftName = shift.ShiftName;
                                    alarmData.details = new List<AlarmDetails>();
                                    foreach (DataRow item1 in dtAlarmsDatails.Select("PlantID = '" + appData.PlantMachine[i].PlantName + "' AND Machineid = '" + item.MachineName + "' AND Shiftid = " + shift.ShiftId))
                                    {
                                        int alarm = 0;
                                        AlarmDetails alarmDetails = new AlarmDetails();
                                        if (int.TryParse(item1["AlarmNo"].ToString(), out alarm))
                                            alarmDetails.alarmNo = alarm;
                                        if (string.IsNullOrEmpty(item1["Fromtime"].ToString()))
                                            alarmDetails.StartTime = null;// Convert.ToDateTime(item1["Fromtime"].ToString());
                                        else
                                            alarmDetails.StartTime = Convert.ToDateTime(item1["Fromtime"].ToString());

                                        if (string.IsNullOrEmpty(item1["Totime"].ToString()))
                                            alarmDetails.EndTime = null;// Convert.ToDateTime(item1["Totime"].ToString());
                                        else
                                            alarmDetails.EndTime = Convert.ToDateTime(item1["Totime"].ToString());
                                        alarmDetails.Duration = item1["Duration"].ToString();
                                        alarmDetails.Message = item1["AlarmMSG"].ToString();
                                        if (int.TryParse(item1["Shiftcount"].ToString(), out alarm))
                                            alarmData.shiftCount = alarm;
                                        alarmData.TotalDuration = item1["Totalduration"].ToString();
                                        alarmData.details.Add(alarmDetails);
                                    }
                                    dayData1.AlarmsDetails.Add(alarmData);
                                }
                                //---for each Alarm Summary--------------
                                foreach (Shift shift in appData.Shifts)
                                {
                                    AlarmData alarmData = new AlarmData();
                                    alarmData.ShiftId = shift.ShiftId;
                                    alarmData.ShiftName = shift.ShiftName;
                                    alarmData.details = new List<AlarmDetails>();
                                    foreach (DataRow item1 in dtAlarmsSummary.Select("PlantID = '" + appData.PlantMachine[i].PlantName + "' AND Machineid = '" + item.MachineName + "' AND Shiftid = " + shift.ShiftId))
                                    {
                                        int alarm = 0;
                                        AlarmDetails alarmDetails = new AlarmDetails();
                                        if (int.TryParse(item1["AlarmNo"].ToString(), out alarm))
                                            alarmDetails.alarmNo = alarm;
                                        if (string.IsNullOrEmpty(item1["Lastseen"].ToString()))
                                            alarmDetails.LastSeen = null;// Convert.ToDateTime(item1["Lastseen"].ToString());
                                        else
                                            alarmDetails.LastSeen = Convert.ToDateTime(item1["Lastseen"].ToString());
                                        alarmDetails.Message = item1["AlarmMSG"].ToString();
                                        if (int.TryParse(item1["NoOfOccurences"].ToString(), out alarm))
                                            alarmDetails.NoOfOcc = alarm;
                                        alarmData.details.Add(alarmDetails);
                                    }
                                    dayData1.AlarmsSummary.Add(alarmData);
                                }
                                appdataDay.DayData.Add(dayData1);

                            }

                        }
                        appData.appDataList.Add(appdataDay);
                    }
                    //if (dtAlarmsSolution.Columns.Count > 0)
                    //{
                    //    foreach (DataRow item in dtAlarmsSolution.Rows)
                    //    {
                    //        int slNo = 0;
                    //        AlarmSolution alarmSolution = new AlarmSolution();
                    //        if (int.TryParse(item["Slno"].ToString(), out slNo))
                    //            alarmSolution.SlNo = slNo;
                    //        if (int.TryParse(item["AlarmNo"].ToString(), out slNo))
                    //            alarmSolution.AlarmNo = slNo;
                    //        alarmSolution.ImageName = item["FilePath"].ToString();
                    //        alarmSolution.Description = item["Description"].ToString();
                    //        alarmSolution.Cause = item["Cause"].ToString();
                    //        alarmSolution.Solution = item["Solution"].ToString();
                    //        alarmSolution.MTB = item["MTB"].ToString().ToUpper();
                    //        appData.AlarmsSolution.Add(alarmSolution);
                    //    }
                    //}



                }
                if (!rdr.IsClosed) rdr.Close();
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog(ex.ToString());
            }

            finally
            {
                conn.Close();
            }
            return appData;
        }

        internal static void ExecuteRuntimeChartDataProc(DateTime LogicalDayStart)
        {
            SqlConnection conn = ConnectionManager.GetConnection();
            string calledProc = @"s_GetFocasWeb_InsertRuntimeChartData";
            try
            {
                SqlCommand cmd = new SqlCommand(calledProc, conn);
                cmd.Parameters.AddWithValue("@DateTime", LogicalDayStart.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.CommandTimeout = 600;
                Logger.WriteDebugLog("Executing proc \"GetFocasWeb_InsertRuntimeChartData\" " + " for Date = " + LogicalDayStart.ToString("yyyy-MM-dd HH:mm:ss"));
                cmd.ExecuteNonQuery();
                Logger.WriteDebugLog("Completed Executing proc \"GetFocasWeb_InsertRuntimeChartData\" ");
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog(ex.Message);
            }
            finally
            {
                if (conn != null) conn.Close();
            }
        }

        internal static DateTime lastRuntimeAggDate()
        {
            SqlConnection conn = ConnectionManager.GetConnection();
            SqlCommand cmd = null;
            SqlDataReader rdr = null;
            DateTime lastAggDT = DateTime.Now;
            string query = @"select  isnull((min(RecordEndtime)), (select min(cnctimestamp) from Focas_LiveData)) as MaxBatchTS from Focas_RunDownTimeAggTrail";

            try
            {
                cmd = new SqlCommand(query, conn);
                cmd.CommandTimeout = 300;
                rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    if (rdr.Read())
                    {
                        if (!DBNull.Value.Equals(rdr["MaxBatchTS"]))
                            lastAggDT = Convert.ToDateTime(rdr["MaxBatchTS"].ToString());                      
                        else 
                            lastAggDT = DateTime.Now;
                    }
                }
            }
            catch(Exception ex)
            {
                Logger.WriteErrorLog("Exception in lastRuntimeAggDate() : " + ex.Message);
            }
            finally
            {
                if (rdr != null) rdr.Close();
                if (conn != null) conn.Close();
            }

            return lastAggDT;
        }

        internal static DateTime lastAggDate()
        {
            object SEDate = null;
            SqlConnection conn = ConnectionManager.GetConnection();
            SqlCommand cmd = new SqlCommand("select  isnull(max([Date]), (select min(cnctimestamp) from Focas_LiveData)) as MaxAggDate from FocasWeb_ShiftwiseSummary", conn);
            cmd.CommandTimeout = 180;
            //DateTime lastAggDate = (DateTime)cmd.ExecuteScalar();

            try
            {
                SEDate = cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog("GENERATED ERROR : \n" + ex.ToString());
            }
            finally
            {
                if (conn != null)
                {
                    conn.Close();
                }
            }
            if (SEDate == null || Convert.IsDBNull(SEDate))
            {
                return DateTime.Now.Date.AddDays(-1);
            }
            return Convert.ToDateTime(SEDate);
        }

        public static DateTime GetLogicalDayStart(DateTime currentTime)
        {
            SqlConnection Con = ConnectionManager.GetConnection();
            SqlCommand cmd = new SqlCommand("SELECT dbo.f_GetLogicalDayStart( '" + currentTime.ToString("yyyy-MM-dd HH:mm:ss") + "')", Con);
            cmd.CommandTimeout = 360;
            object SEDate = null;
            try
            {
                SEDate = cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog("GENERATED ERROR : \n" + ex.ToString());
            }
            finally
            {
                if (Con != null)
                {
                    Con.Close();
                }
            }
            if (SEDate == null || Convert.IsDBNull(SEDate))
            {
                return DateTime.Now.Date.AddDays(-1);
            }
            return Convert.ToDateTime(SEDate);
        }

        public static DateTime GetLogicalDayEnd(DateTime LRunDay)
        {
            object SEDate = null;
            SqlConnection Con = ConnectionManager.GetConnection();
            try
            {
                SqlCommand cmd = new SqlCommand("SELECT dbo.f_GetLogicalDayEnd( '" + string.Format("{0:yyyy-MM-dd HH:mm:ss}", LRunDay.AddSeconds(1)) + "')", Con);
                cmd.CommandTimeout = 360;
                SEDate = cmd.ExecuteScalar();
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog("GENERATED ERROR : \n" + ex.ToString());
            }
            finally
            {
                if (Con != null)
                {
                    Con.Close();
                }
            }
            if (SEDate == null || Convert.IsDBNull(SEDate))
            {
                return DateTime.Now.Date.AddDays(1);//.ToString("yyyy-MM-dd 06:00:00");
            }
            return Convert.ToDateTime(SEDate);
        }
        internal static List<AlarmSolution> GetAlarmSolutionData()
        {
            AlarmSolution alarmSolution = null;
            List<AlarmSolution> alarmSolutionData = new List<AlarmSolution>();
            SqlConnection con = ConnectionManager.GetConnection();
            SqlDataReader rdr = null;
            string query = @"SELECT Slno, AlarmNo, Flag, FilePath, Description, Cause, Solution, MTB FROM  Focas_AlarmMaster where ( AlarmNo < 1150  OR AlarmNo > 1172 ) Order by MTB,Alarmno";
            try
            {
                SqlCommand cmd = new SqlCommand(query, con);
                cmd.CommandType = CommandType.Text;
                rdr = cmd.ExecuteReader();
                if (rdr.HasRows)
                {
                    while (rdr.Read())
                    {
                        alarmSolution = new AlarmSolution();
                        alarmSolution.SlNo = Convert.ToInt32(rdr["Slno"]);
                        alarmSolution.AlarmNo = Convert.ToInt32(rdr["AlarmNo"]);
                        alarmSolution.Description = rdr["Description"].ToString();
                        alarmSolution.Cause = rdr["Cause"].ToString();
                        alarmSolution.Solution = rdr["Solution"].ToString();
                        alarmSolutionData.Add(alarmSolution);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.WriteErrorLog(ex.Message);
            }
            return alarmSolutionData;
        }

    }
}
