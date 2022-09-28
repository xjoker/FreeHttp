using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FreeHttp.AutoTest.RunTimeStaticData;
using FreeHttp.FiddlerHelper;
using FreeHttp.MyHelper;

namespace FreeHttp.WebService
{
    public class OperationReportService
    {
        private DateTime? nowInTime;

        private readonly OperationDetail operationDetail;

        public OperationReportService()
        {
            operationDetail = new OperationDetail(UserComputerInfo.GetComputerMac(), UserComputerInfo.GetMachineName(),
                UserComputerInfo.UserToken);
            nowInTime = null;
        }

        public List<FiddlerRequestChange> FiddlerRequestChangeRuleList { get; set; } = null;
        public List<FiddlerResponseChange> FiddlerResponseChangeRuleList { get; set; } = null;
        public ActuatorStaticDataCollection StaticDataCollection { get; set; } = null;
        public FiddlerRuleGroup RuleGroup { get; set; } = null;

        public bool HasAnyOperation => operationDetail.OperationDetailCells.Count > 0;

        public void InOperation(DateTime inTime)
        {
            nowInTime = inTime;
        }

        public void OutOperation(DateTime outTime, int requestRuleCount, int responseRuleCount)
        {
            operationDetail.AddCell(nowInTime == null ? outTime : (DateTime)nowInTime, outTime, requestRuleCount,
                responseRuleCount);
            nowInTime = null;
        }

        public async void ReportAsync()
        {
            //task需要在执行时设置CurrentThread.IsBackground，不能确保在设置成功前主线程不退出
            Action reportAction = Report;
            await Task.Run(reportAction);
        }

        public void StartReportThread()
        {
            var reportThread = new Thread(Report);
            reportThread.IsBackground = false; //使用Thread创建的线程其实默认IsBackground就是false
            reportThread.Start();
        }

        private void Report()
        {
            if (Thread.CurrentThread
                .IsBackground) //大部分情况在async方法里使用这种方式也没有效果 1：不能确保线程执行到这里没有被主线程结束，2：对于async方法大部分情况执行这里的代码也是上一个线程，到await 才可能切换线程 （不过仍然可以通过同步的方式启动async方法）
                Thread.CurrentThread.IsBackground = false;
            if (operationDetail.OperationDetailCells.Count > 0)
            {
                string operationBody = null;
                //operationBody = Fiddler.WebFormats.JSON.JsonEncode(this.operationDetail);
                operationBody = MyJsonHelper.JsonDataContractJsonSerializer.ObjectToJsonStr(operationDetail);
                new MyWebTool.MyHttp().SendHttpRequest(
                    string.Format("{0}freehttp/OperationReport", ConfigurationData.BaseUrl), operationBody, "POST",
                    new List<KeyValuePair<string, string>>
                        { new KeyValuePair<string, string>("Content-Type", "application/json") }, false, null, null);
                if (FiddlerRequestChangeRuleList != null || FiddlerResponseChangeRuleList != null)
                    RemoteRuleService.UploadRulesAsync(FiddlerRequestChangeRuleList, FiddlerResponseChangeRuleList,
                        StaticDataCollection, RuleGroup).Wait();
            }
            //System.GC.Collect();
        }

        [DataContract]
        public class OperationDetail
        {
            public OperationDetail(string mac = "FF:FF:FF;FF:FF:FF", string machineName = null, string userToken = null)
            {
                UserMac = mac;
                MachineName = machineName;
                UserToken = userToken;
                OperationDetailCells = new List<OperationDetailCell>();
            }

            [DataMember(Name = "UserToken")] public string UserToken { get; set; }

            [DataMember(Name = "UserMac")] public string UserMac { get; set; }

            [DataMember(Name = "MachineName")] public string MachineName { get; set; }

            [DataMember(Name = "OperationDetailCells")]
            public List<OperationDetailCell> OperationDetailCells { get; set; }

            public void AddCell(DateTime inTime, DateTime outTime, int requestRuleCount, int responseRuleCount)
            {
                if (OperationDetailCells == null) OperationDetailCells = new List<OperationDetailCell>();
                OperationDetailCells.Add(new OperationDetailCell
                {
                    InTime = inTime, OutTime = outTime, RequestRuleCount = requestRuleCount,
                    ResponseRuleCount = responseRuleCount
                });
            }

            public class OperationDetailCell
            {
                public DateTime InTime { get; set; }
                public DateTime OutTime { get; set; }
                public int RequestRuleCount { get; set; }
                public int ResponseRuleCount { get; set; }
            }
        }
    }
}