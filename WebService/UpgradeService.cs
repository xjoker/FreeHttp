﻿using System;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FreeHttp.AutoTest.ParameterizationPick;
using FreeHttp.MyHelper;

namespace FreeHttp.WebService
{
    public class UpgradeService
    {
        private static readonly HttpClient httpClient;

        private readonly MyWebTool.MyHttp myHttp;

        static UpgradeService()
        {
            httpClient = new HttpClient();
        }

        public UpgradeService()
        {
            MyWebTool.MyHttp.EnableServerCertificateValidation = true;
            myHttp = new MyWebTool.MyHttp();
        }

        public string SilentUpgradeUrl { get; private set; }

        public event EventHandler<UpgradeServiceEventArgs> GetUpgradeMes;

        public void StartCheckUpgrade()
        {
            //Task checkUpgradeCheckUpgrade = new Task(CheckUpgrade);
            //checkUpgradeCheckUpgrade.Start();
            //checkUpgradeCheckUpgrade.ContinueWith((task) => { StartCheckUpgrade(); });

            var checkUpgradeTask = new Task<UpgradeServiceEventArgs>(() =>
            {
                //string tempResponse = myHttp.SendData(string.Format(@"{0}freehttp/UpdateCheck/v{1}?dotnetrelease={2}&{3}", ConfigurationData.BaseUrl, System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString(),  UserComputerInfo.GetDotNetRelease(), UserComputerInfo.GetFreeHttpUser()));
                //string isNeedUpdata = FreeHttp.AutoTest.ParameterizationPick.ParameterPickHelper.PickStrParameter("\"isNeedUpdata\":", ",", tempResponse);
                //string isSilentUpgrade = FreeHttp.AutoTest.ParameterizationPick.ParameterPickHelper.PickStrParameter("\"isSilentUpgrade\":", ",", tempResponse);
                //string url = FreeHttp.AutoTest.ParameterizationPick.ParameterPickHelper.PickStrParameter("\"url\":\"", "\",", tempResponse);
                //string message = FreeHttp.AutoTest.ParameterizationPick.ParameterPickHelper.PickStrParameter("\"message\":\"", "\"", tempResponse);
                //if (string.IsNullOrEmpty(isNeedUpdata))
                //{
                //    return new UpgradeServiceEventArgs(false, null);
                //}
                //if (isNeedUpdata == "true")
                //{
                //    if (isSilentUpgrade == "true") SilentUpgradeUrl = url;
                //    return new UpgradeServiceEventArgs(true, url, null, isSilentUpgrade == "true");
                //}
                //if (!string.IsNullOrEmpty(message))
                //{
                //    return new UpgradeServiceEventArgs(true, null, message);
                //}
                //return null;

                try
                {
                    var response = httpClient
                        .GetAsync(string.Format(@"{0}freehttp/UpdateCheck/v{1}?dotnetrelease={2}&{3}",
                            ConfigurationData.BaseUrl, UserComputerInfo.GetFreeHttpVersion(),
                            UserComputerInfo.GetDotNetRelease(), UserComputerInfo.GetFreeHttpUser())).GetAwaiter()
                        .GetResult();
                    if (response.IsSuccessStatusCode)
                    {
                        var updateInfo =
                            MyJsonHelper.JsonDataContractJsonSerializer.JsonStringToObject<UpdateInfo>(response.Content
                                .ReadAsStringAsync().GetAwaiter().GetResult());
                        if (updateInfo == null)
                        {
                            _ = RemoteLogService.ReportLogAsync("JsonStringToObject fail that StartCheckUpgrade",
                                RemoteLogService.RemoteLogOperation.CheckUpgrade, RemoteLogService.RemoteLogType.Error);
                            return new UpgradeServiceEventArgs(false, null);
                        }

                        if (updateInfo.isNeedUpdata && updateInfo.isSilentUpgrade &&
                            !string.IsNullOrEmpty(updateInfo.url)) SilentUpgradeUrl = updateInfo.url;
                        return new UpgradeServiceEventArgs(true, updateInfo);
                    }

                    _ = RemoteLogService.ReportLogAsync("StartCheckUpgrade get error response",
                        RemoteLogService.RemoteLogOperation.CheckUpgrade, RemoteLogService.RemoteLogType.Error);
                    return new UpgradeServiceEventArgs(false, null);
                }
                catch (Exception ex)
                {
                    _ = RemoteLogService.ReportLogAsync(ex.ToString(), RemoteLogService.RemoteLogOperation.Exception,
                        RemoteLogService.RemoteLogType.Error);
                    return new UpgradeServiceEventArgs(false, null);
                }
            });

            checkUpgradeTask.Start();
            checkUpgradeTask.ContinueWith(task =>
            {
                if (checkUpgradeTask.Result != null) GetUpgradeMes(this, checkUpgradeTask.Result);
            });
        }

        public void TrySilentUpgrade()
        {
            if (string.IsNullOrEmpty(SilentUpgradeUrl)) return;
            var silentUpgradeThread = new Thread(StartSilentUpgrade);
            silentUpgradeThread.IsBackground = false; //使用Thread创建的线程其实默认IsBackground就是false
            silentUpgradeThread.Start();
        }

        private void StartSilentUpgrade()
        {
            if (Thread.CurrentThread
                .IsBackground) //大部分情况在async方法里使用这种方式也没有效果 1：不能确保线程执行到这里没有被主线程结束，2：对于async方法大部分情况执行这里的代码也是上一个线程，到await 才可能切换线程 （不过仍然可以通过同步的方式启动async方法）
                Thread.CurrentThread.IsBackground = false;
            //_ = await RemoteLogService.ReportLogAsync() 父进程可能先结束，不会管以异步启动的任务是否完成
            RemoteLogService.ReportLogAsync("start SilentUpgrade", RemoteLogService.RemoteLogOperation.SilentUpgrade)
                .Wait();
            //MyHelper.SelfUpgradeHelp.UpdateDllAsync("https://lulianqi.com/file/FreeHttpUpgradeFile").Wait();
            SelfUpgradeHelp.UpdateDllAsync(SilentUpgradeUrl).ContinueWith(result =>
            {
                if (!string.IsNullOrEmpty(result.Result))
                    RemoteLogService.ReportLogAsync(result.Result, RemoteLogService.RemoteLogOperation.SilentUpgrade,
                        RemoteLogService.RemoteLogType.Error).Wait();
                else
                    RemoteLogService.ReportLogAsync("SilentUpgrade complete",
                        RemoteLogService.RemoteLogOperation.SilentUpgrade).Wait();
            }).Wait();
        }


        /// <summary>
        ///     give up maintenance
        /// </summary>
        public void StartCheckUpgradeThread()
        {
            var checkUpgradeTaskThread = new Thread(CheckUpgrade);
            checkUpgradeTaskThread.Name = "CheckUpgradeTaskThread";
            checkUpgradeTaskThread.Priority = ThreadPriority.Normal;
            checkUpgradeTaskThread.IsBackground = true;
            checkUpgradeTaskThread.Start();
        }

        /// <summary>
        ///     give up maintenance
        /// </summary>
        private void CheckUpgrade()
        {
            var tempResponse = myHttp.SendData(string.Format(@"{0}freehttp/UpdateCheck/v1.1?user={1}",
                ConfigurationData.BaseUrl, UserComputerInfo.GetComputerMac()));
            var isNeedUpdata = ParameterPickHelper.PickStrParameter("\"isNeedUpdata\":", ",", tempResponse);
            var url = ParameterPickHelper.PickStrParameter("\"url\":", ",", tempResponse);
            var message = ParameterPickHelper.PickStrParameter("\"message\":", ",", tempResponse);
            if (string.IsNullOrEmpty(isNeedUpdata)) GetUpgradeMes(this, new UpgradeServiceEventArgs(false, null));
            if (isNeedUpdata == "true")
                GetUpgradeMes(this, new UpgradeServiceEventArgs(true, new UpdateInfo { url = url }));
        }

        [DataContract]
        public class UpdateInfo
        {
            /// <summary>
            ///     是否需要更新
            /// </summary>
            [DataMember]
            public bool isNeedUpdata { get; set; }

            /// <summary>
            ///     是否进行静默升级
            /// </summary>
            [DataMember]
            public bool isSilentUpgrade { get; set; }

            /// <summary>
            ///     是否显示辅助提示信息 （与messageFlag,message,url,isForceEnter配合使用）
            ///     如果isShowMessage为true，显示辅助message，messageFlag控制是否总是显示，url如果有值用户点击确认时进入url链接，isForceEnter控制是否仅显示确认按钮
            /// </summary>
            [DataMember]
            public bool isShowMessage { get; set; }

            /// <summary>
            ///     信息标识，含有该标识辅助信息每个用户只会显示一次
            /// </summary>
            [DataMember]
            public string messageFlag { get; set; }

            /// <summary>
            ///     升级文件地址
            /// </summary>
            [DataMember]
            public string url { get; set; }

            /// <summary>
            ///     辅助提示信息
            /// </summary>
            [DataMember]
            public string message { get; set; }

            /// <summary>
            ///     仅显示确认按钮
            /// </summary>
            [DataMember]
            public bool isForceEnter { get; set; }

            /// <summary>
            ///     用户的uuid
            /// </summary>
            [DataMember]
            public string uuid { get; set; }
        }

        public class UpgradeServiceEventArgs : EventArgs
        {
            public UpgradeServiceEventArgs(bool isSuccess, UpdateInfo upgradeInfo)
            {
                IsSuccess = isSuccess;
                UpgradeInfo = upgradeInfo;
            }

            public bool IsSuccess { get; set; }
            public UpdateInfo UpgradeInfo { get; set; }
        }
    }
}