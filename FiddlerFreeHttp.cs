using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Forms;
using Fiddler;
using FreeHttp.AutoTest.RunTimeStaticData;
using FreeHttp.FiddlerHelper;
using FreeHttp.FreeHttpControl;
using FreeHttp.MyHelper;
using FreeHttp.Resources;
using FreeHttp.WebService;

/*******************************************************************************
* Copyright (c) 2018 lulianqi
* All rights reserved.
* 
* 文件名称: 
* 内容摘要: mycllq@hotmail.com
* 
* 历史记录:
* 日	  期:   20181103           创建人: lulianqi [mycllq@hotmail.com]
* 描    述: 创建
*
* 历史记录:
* 日	  期:                      修改:  
* 描    述: 
*******************************************************************************/

[assembly: RequiredVersion("2.3.5.0")]

namespace FreeHttp
{
    public class FiddlerFreeHttp : IAutoTamper, IDisposable
    {
        private bool isCheckedUpdata; //是否已经成功完成更新检查，如果检查失败会被重新设置为false
        private bool isInFreeHttpTab; //是否在正在FreeHttp Tab页中
        private bool isOnLoad; //是否已经加载过tab
        private FreeHttpWindow myFreeHttpWindow;
        private Image myIco;
        private OperationReportService operationReportService;
        private TabPage tabPage;
        private UpgradeService upgradeService;

        public bool IsSkipConnectTunnels
        {
            get
            {
                if (myFreeHttpWindow != null && myFreeHttpWindow.ModificSettingInfo != null)
                    return myFreeHttpWindow.ModificSettingInfo.IsSkipConnectTunnels;
                return true;
            }
        }

        public bool IsSkipUiHide
        {
            get
            {
                if (myFreeHttpWindow != null && myFreeHttpWindow.ModificSettingInfo != null)
                    return myFreeHttpWindow.ModificSettingInfo.IsSkipUiHide;
                return true;
            }
        }

        public bool IsOnlyMatchFistTamperRule
        {
            get
            {
                if (myFreeHttpWindow != null && myFreeHttpWindow.ModificSettingInfo != null)
                    return myFreeHttpWindow.ModificSettingInfo.IsOnlyMatchFirstTamperRule;
                return false;
            }
        }

        public bool IsHideFreeHttpSession => isCheckedUpdata && myFreeHttpWindow.ModificSettingInfo.IsHideSelfSession;

        public void OnBeforeUnload()
        {
            SerializableHelper.SerializeRuleList(myFreeHttpWindow.RequestRuleListView,
                myFreeHttpWindow.ResponseRuleListView);
            SerializableHelper.SerializeData(myFreeHttpWindow.ModificSettingInfo, "FreeHttp\\FreeHttpSetting.xml");
            SerializableHelper.SerializeContractData(myFreeHttpWindow.StaticDataCollection,
                "FreeHttp\\FreeHttpStaticDataCollection.xml");
            SerializableHelper.SerializeContractData(myFreeHttpWindow.ModificRuleGroup,
                "FreeHttp\\FreeHttpModificRuleGroup.xml");

            if (isInFreeHttpTab)
                operationReportService.OutOperation(DateTime.Now, myFreeHttpWindow.RequestRuleListView.Items.Count,
                    myFreeHttpWindow.ResponseRuleListView.Items.Count);
            if (operationReportService.HasAnyOperation)
            {
                operationReportService.StaticDataCollection = myFreeHttpWindow.StaticDataCollection.IsEmpty
                    ? null
                    : myFreeHttpWindow.StaticDataCollection;
                operationReportService.RuleGroup = myFreeHttpWindow.ModificRuleGroup.IsEmpty
                    ? null
                    : myFreeHttpWindow.ModificRuleGroup;
                operationReportService.FiddlerRequestChangeRuleList = myFreeHttpWindow.FiddlerRequestChangeList;
                operationReportService.FiddlerResponseChangeRuleList = myFreeHttpWindow.FiddlerResponseChangeList;
                operationReportService.StartReportThread();
            }

            // 阻断升级
            //upgradeService.TrySilentUpgrade();
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        public void OnLoad()
        {
            AddFiddlerObjectLog("OnLoad");
            var workPath = $"{Directory.GetCurrentDirectory()}\\FreeHttp";
            if (!isOnLoad)
            {
                tabPage = new TabPage();
                tabPage.Text = "Free Http(MOD)";
                if (FiddlerApplication.UI.tabsViews.ImageList != null)
                {
                    myIco = MyResource.freehttp;
                    FiddlerApplication.UI.tabsViews.ImageList.Images.Add(myIco);
                    tabPage.ImageIndex = FiddlerApplication.UI.tabsViews.ImageList.Images.Count - 1;
                }

                try
                {
                    if (!Directory.Exists(workPath))
                    {
                        AddFiddlerObjectLog($"Create working directory {workPath}");
                        Directory.CreateDirectory(workPath);
                    }

                    AddFiddlerObjectLog("load configuration");
                    myFreeHttpWindow = new FreeHttpWindow(SerializableHelper.DeserializeRuleList(),
                        SerializableHelper.DeserializeData<FiddlerModificSettingInfo>("FreeHttp\\FreeHttpSetting.xml"),
                        SerializableHelper.DeserializeContractData<ActuatorStaticDataCollection>(
                            "FreeHttp\\FreeHttpStaticDataCollection.xml"),
                        SerializableHelper.DeserializeContractData<FiddlerRuleGroup>(
                            "FreeHttp\\FreeHttpModificRuleGroup.xml"));
                }
                catch (Exception ex)
                {
                    AddFiddlerObjectLog($"load configuration fial ,{ex.Message}");
                }
                finally
                {
                    if (myFreeHttpWindow == null) myFreeHttpWindow = new FreeHttpWindow(null, null, null, null);
                }

                myFreeHttpWindow.OnUpdataFromSession += myFreeHttpWindow_OnUpdataFromSession;
                myFreeHttpWindow.OnGetSessionRawData += myFreeHttpWindow_OnGetSessionRawData;
                myFreeHttpWindow.OnGetSessionEventArgs += MyFreeHttpWindow_OnGetSessionEventArgs;
                myFreeHttpWindow.OnGetSessionSeekHead += myFreeHttpWindow_OnGetSessionSeekHead;
                myFreeHttpWindow.OnShowInIndependentWindow += MyFreeHttpWindow_OnShowInIndependentWindow;
                myFreeHttpWindow.Dock = DockStyle.Fill;
                myFreeHttpWindow.Enter += myFreeHttpWindow_Enter;
                tabPage.Controls.Add(myFreeHttpWindow);
                FiddlerApplication.UI.tabsViews.TabPages.Add(tabPage);
                FiddlerApplication.UI.Deactivate += UI_Deactivate;
                FiddlerApplication.UI.tabsViews.SelectedIndexChanged += tabsViews_SelectedIndexChanged;
                FiddlerApplication.OnWebSocketMessage += FiddlerApplication_OnWebSocketMessage;
                FiddlerApplication.UI.tabsViews.ParentChanged += TabsViews_ParentChanged;

                // 关闭在线服务
                //upgradeService = new UpgradeService();
                //upgradeService.GetUpgradeMes += upgradeService_GetUpgradeMes;
                operationReportService = new OperationReportService();
                isOnLoad = true;
            }
        }

        public void AutoTamperRequestAfter(Session oSession)
        {
            //throw new NotImplementedException();
        }

        public void AutoTamperRequestBefore(Session oSession)
        {
            if (!isOnLoad) return;
            if (IsHideFreeHttpSession && oSession.oRequest.host == "api.lulianqi.com") oSession["ui-hide"] = "true";
            if (myFreeHttpWindow.IsRequestRuleEnable)
            {
                //IsRequestRuleEnable is more efficient then string comparison (so if not IsRequestRuleEnable the string comparison will not execute)
                if (IsSkipUiHide && oSession["ui-hide"] != null) return;
                if (IsSkipConnectTunnels && oSession.RequestMethod == "CONNECT") return;
                var matchItems =
                    FiddlerSessionHelper.FindMatchTamperRule(oSession, myFreeHttpWindow.FiddlerRequestChangeList, true);
                if (matchItems != null && matchItems.Count > 0)
                    foreach (var matchItem in matchItems)
                    {
                        var nowFiddlerRequestChange = (FiddlerRequestChange)matchItem;
                        var tempListViewItem = myFreeHttpWindow.FindListViewItemFromRule(matchItem);
                        FreeHttpWindow.MarkMatchRule(tempListViewItem);
                        MarkSession(oSession);
                        ShowMes($"macth the [requst rule {tempListViewItem.SubItems[0].Text}] with {oSession.fullUrl}");
                        FiddlerSessionTamper.ModificSessionRequest(oSession, nowFiddlerRequestChange, ShowError,
                            ShowMes);
                        if (IsOnlyMatchFistTamperRule) break;
                    }
            }

            if (myFreeHttpWindow.IsResponseRuleEnable)
            {
                if (IsSkipConnectTunnels && oSession.RequestMethod == "CONNECT") return;
                var matchItems =
                    FiddlerSessionHelper.FindMatchTamperRule(oSession, myFreeHttpWindow.FiddlerResponseChangeList,
                        false);
                if (matchItems != null && matchItems.Count > 0)
                {
                    oSession.bBufferResponse =
                        true; //  if any response rule may match the Session, we should set bBufferResponse true (When streaming is enabled for a response, each block of data read from the server is immediately passed to the client application. )
                    foreach (var matchItem in matchItems)
                    {
                        var nowFiddlerResponseChange = (FiddlerResponseChange)matchItem;
                        var tempListViewItem = myFreeHttpWindow.FindListViewItemFromRule(matchItem);
                        if (nowFiddlerResponseChange.IsIsDirectRespons)
                        {
                            FreeHttpWindow.MarkMatchRule(tempListViewItem);
                            MarkSession(oSession);
                            ShowMes(
                                $"macth the [reponse rule {tempListViewItem.SubItems[0].Text}] with {oSession.fullUrl}");
                            FiddlerSessionTamper.ReplaceSessionResponse(oSession, nowFiddlerResponseChange, ShowError,
                                ShowMes);
                            //oSession.state = SessionStates.Done;
                            if (IsOnlyMatchFistTamperRule) break;
                        }
                    }
                }
            }
        }

        public void AutoTamperResponseAfter(Session oSession)
        {
            if (!isOnLoad) return;
            if (myFreeHttpWindow.IsResponseRuleEnable)
            {
                if (IsSkipUiHide && oSession["ui-hide"] != null) return;
                if (IsSkipConnectTunnels && oSession.RequestMethod == "CONNECT") return;
                var matchItems =
                    FiddlerSessionHelper.FindMatchTamperRule(oSession, myFreeHttpWindow.FiddlerResponseChangeList,
                        false);
                if (matchItems != null && matchItems.Count > 0)
                    foreach (var matchItem in matchItems)
                    {
                        var nowFiddlerResponseChange = (FiddlerResponseChange)matchItem;
                        var tempListViewItem = myFreeHttpWindow.FindListViewItemFromRule(matchItem);
                        if (!(nowFiddlerResponseChange.IsRawReplace && nowFiddlerResponseChange.IsIsDirectRespons))
                        {
                            FreeHttpWindow.MarkMatchRule(tempListViewItem);
                            MarkSession(oSession);
                            ShowMes(string.Format("macth the [reponse rule {0}] with {1}",
                                tempListViewItem.SubItems[0].Text, oSession.fullUrl));
                            FiddlerSessionTamper.ModificSessionResponse(oSession, nowFiddlerResponseChange, ShowError,
                                ShowMes);
                        }

                        if (nowFiddlerResponseChange.ResponseLatency > 0)
                        {
                            ShowMes(string.Format("[reponse rule {0}] is modified , now delay {1} ms",
                                tempListViewItem.SubItems[0].Text, nowFiddlerResponseChange.ResponseLatency));
                            Thread.Sleep(nowFiddlerResponseChange.ResponseLatency);
                        }

                        if (IsOnlyMatchFistTamperRule) break;
                    }
            }
        }

        public void AutoTamperResponseBefore(Session oSession)
        {
            //throw new NotImplementedException();
        }

        public void OnBeforeReturningError(Session oSession)
        {
            AutoTamperResponseAfter(oSession);
        }


        public void Dispose()
        {
            tabPage.Dispose();
            myFreeHttpWindow.Dispose();
        }

        private void ShowMes(string mes)
        {
            ShowMes(mes, false);
        }

        private void ShowMes(string mes, bool isReport = false)
        {
            if (!isOnLoad) return;
            if (isReport) _ = RemoteLogService.ReportLogAsync(mes, RemoteLogService.RemoteLogOperation.SessionTamp);
            if (myFreeHttpWindow.InvokeRequired)
                //BeginInvoke,Invoke will execute in the contol ui thread, but Invoke will with the end in the ui thread
                //myFreeHttpWindow.Invoke(new Action(()=>{System.Threading.Thread.Sleep(10000);}) );
                myFreeHttpWindow.BeginInvoke(new Action<string>(myFreeHttpWindow.PutInfo), mes);
            else
                myFreeHttpWindow.PutInfo(mes);
        }

        private void ShowError(string mes)
        {
            ShowError(mes, true);
        }

        private void ShowError(string mes, bool isReport = true)
        {
            if (!isOnLoad) return;
            AddFiddlerObjectLog(mes);
            if (isReport)
                _ = RemoteLogService.ReportLogAsync(mes, RemoteLogService.RemoteLogOperation.SessionTamp,
                    RemoteLogService.RemoteLogType.Error);
            if (myFreeHttpWindow.InvokeRequired)
                myFreeHttpWindow.BeginInvoke(new Action<string>(myFreeHttpWindow.PutError), mes);
            else
                myFreeHttpWindow.PutError(mes);
        }

        private void AddFiddlerObjectLog(string mes)
        {
            FiddlerObject.log($"【FiddlerFreeHttp】:{mes}");
        }

        private void SetStatusText(string mes)
        {
            FiddlerObject.StatusText = mes;
        }

        private void MarkSession(Session oSession)
        {
            oSession["ui-backcolor"] = "Khaki";
            oSession["ui-bold"] = "true";
            oSession["ui-color"] = "Indigo";
            oSession.RefreshUI();
        }


        private void TabsViews_ParentChanged(object sender, EventArgs e)
        {
            myFreeHttpWindow.FreeHttpWindowParentChanged(sender);
        }

        private void tabsViews_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (isCheckedUpdata == false && FiddlerApplication.UI.tabsViews.SelectedTab == tabPage)
            {
                isCheckedUpdata = true;
                //upgradeService.StartCheckUpgrade();
                //upgradeService.StartCheckUpgradeThread();
            }

            //operation report
            if (FiddlerApplication.UI.tabsViews.SelectedTab == tabPage)
            {
                isInFreeHttpTab = true;
                operationReportService.InOperation(DateTime.Now);
                myFreeHttpWindow.FreeHttpWindowSelectedChanged(true);
            }
            else if (isInFreeHttpTab)
            {
                isInFreeHttpTab = false;
                operationReportService.OutOperation(DateTime.Now, myFreeHttpWindow.RequestRuleListView.Items.Count,
                    myFreeHttpWindow.ResponseRuleListView.Items.Count);
                myFreeHttpWindow.FreeHttpWindowSelectedChanged(false);
            }
        }

        private void myFreeHttpWindow_Enter(object sender, EventArgs e)
        {
            //when myFreeHttpWindow is enter do somethings
        }


        private void UI_Deactivate(object sender, EventArgs e)
        {
            myFreeHttpWindow.CloseEditRtb();
        }


        private void upgradeService_GetUpgradeMes(object sender, UpgradeService.UpgradeServiceEventArgs e)
        {
            Action<string, string> ShowDialogResultBox = (message, title) =>
            {
                if (string.IsNullOrEmpty(e.UpgradeInfo.url))
                {
                    MessageBox.Show(message, title);
                }
                else
                {
                    if (MessageBox.Show(message, title,
                            e.UpgradeInfo.isForceEnter ? MessageBoxButtons.OK : MessageBoxButtons.OKCancel,
                            MessageBoxIcon.Information) == DialogResult.OK)
                    {
                        ShowMes(string.Format("enter message details [flag:{0}]", e.UpgradeInfo.messageFlag ?? "any"),
                            true);
                        if (string.IsNullOrEmpty(e.UpgradeInfo.url))
                        {
                            MessageBox.Show("UpgradeInfo.url is error");
                            return;
                        }

                        try
                        {
                            Process.Start(e.UpgradeInfo.url);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show(string.Format("UpgradeMes is error \r\n{0}", ex.Message));
                            ShowError(string.Format("process start fail [{0}] [{1}] ", e.UpgradeInfo.url, ex.Message));
                        }
                    }
                    else
                    {
                        ShowMes(
                            string.Format("cancel enter message details [flag:{0}]",
                                e.UpgradeInfo.messageFlag ?? "any"), true);
                    }
                }
            };

            if (e.IsSuccess)
            {
                if (!string.IsNullOrEmpty(e.UpgradeInfo.uuid) &&
                    string.IsNullOrEmpty(myFreeHttpWindow.ModificSettingInfo.UserToken))
                    myFreeHttpWindow.ModificSettingInfo.UserToken = e.UpgradeInfo.uuid;

                if (e.UpgradeInfo.isNeedUpdata && !e.UpgradeInfo.isSilentUpgrade)
                {
                    //ShowDialogResultBox(string.IsNullOrEmpty(e.UpgradeInfo.message)?"Find new version for [ FreeHttp Plug-in ] \r\nDo you want goto upgrade page to udpade your FreeHttp" : e.UpgradeInfo.message , "find new version");
                    myFreeHttpWindow.Invoke(new Action(() =>
                    {
                        ShowDialogResultBox(
                            string.IsNullOrEmpty(e.UpgradeInfo.message)
                                ? "Find new version for [ FreeHttp Plug-in ] \r\nDo you want goto upgrade page to udpade your FreeHttp"
                                : e.UpgradeInfo.message, "find new version");
                    }));
                    return;
                }

                if (e.UpgradeInfo.isNeedUpdata && e.UpgradeInfo.isSilentUpgrade)
                {
                    //Silent Upgrade
                }
                else if (e.UpgradeInfo.isShowMessage && !string.IsNullOrEmpty(e.UpgradeInfo.message))
                {
                    //show meaasge
                    if (string.IsNullOrEmpty(e.UpgradeInfo.messageFlag))
                    {
                        myFreeHttpWindow.Invoke(new Action(() =>
                        {
                            ShowDialogResultBox(e.UpgradeInfo.message, "new message");
                        }));
                    }
                    else
                    {
                        if (!myFreeHttpWindow.ModificSettingInfo.ReadedMessageFlags.Contains(e.UpgradeInfo.messageFlag))
                        {
                            myFreeHttpWindow.Invoke(new Action(() =>
                            {
                                ShowDialogResultBox(e.UpgradeInfo.message, "new message");
                            }));
                            myFreeHttpWindow.ModificSettingInfo.ReadedMessageFlags.Add(e.UpgradeInfo.messageFlag);
                        }
                    }
                }
                //not any thing
            }
            else
            {
                isCheckedUpdata = false;
            }
        }

        private void myFreeHttpWindow_OnGetSessionRawData(object sender, FreeHttpWindow.GetSessionRawDataEventArgs e)
        {
            var tempSession = FiddlerObject.UI.GetFirstSelectedSession();
            if (tempSession == null)
            {
                FiddlerObject.UI.ShowAlert(new frmAlert("STOP", "please select a session", "OK"));
                FreeHttpWindow.MarkWarnControl(FiddlerApplication.UI.lvSessions);
                return;
            }

            switch (e.SessionAction)
            {
                case FreeHttpWindow.GetSessionAction.ShowShowResponse:
                    var tempStr = FiddlerSessionTamper.GetSessionRawData(tempSession, true);
                    ShowMes(tempStr == null ? "error session" : string.Format("Get Raw Data\r\n{0}", tempStr));
                    myFreeHttpWindow.ShowOwnerWindow(tempSession.fullUrl, tempStr);
                    break;
                case FreeHttpWindow.GetSessionAction.SetCookies:
                    myFreeHttpWindow.SetClientAddCookies(tempSession.RequestHeaders["Cookie"]);
                    break;
                case FreeHttpWindow.GetSessionAction.DeleteCookies:
                    myFreeHttpWindow.SetClientDelCookies(tempSession.RequestHeaders["Cookie"]);
                    break;
                default:
                    FiddlerObject.UI.ShowAlert(new frmAlert("STOP", "Not supported this SessionAction", "OK"));
                    break;
            }
        }

        private void MyFreeHttpWindow_OnGetSessionEventArgs(object sender, FreeHttpWindow.GetSessionEventArgs e)
        {
            var tempSession = FiddlerObject.UI.GetFirstSelectedSession();
            if (tempSession == null)
                e.IsGetSuccess = false;
            else
                e.IsGetSuccess = FiddlerSessionTamper.GetSessionData(tempSession, e);
        }

        private void myFreeHttpWindow_OnGetSessionSeekHead(object sender, FreeHttpWindow.GetSessionSeekHeadEventArgs e)
        {
            var tempSession = FiddlerObject.UI.GetFirstSelectedSession();
            if (tempSession == null)
            {
                FreeHttpWindow.MarkWarnControl(FiddlerApplication.UI.lvSessions);
            }
            else
            {
                if (e != null && e.ResquestHead.Key != null)
                {
                    //HTTPHeaderItem nowHTTPHeaderItem = tempSession.RequestHeaders.First(hTTPHeaderItem => hTTPHeaderItem.Name == e.ResquestHead.Key);
                    var nowHTTPHeaderItem =
                        tempSession.RequestHeaders.FirstOrDefault(hTTPHeaderItem =>
                            hTTPHeaderItem.Name == e.ResquestHead.Key);

                    if (nowHTTPHeaderItem != null)
                    {
                        e.ResquestHead =
                            new KeyValuePair<string, string>(nowHTTPHeaderItem.Name, nowHTTPHeaderItem.Value);
                        e.SeekUri = tempSession.fullUrl;
                    }
                }

                if (e != null && e.ResponseHead.Key != null)
                {
                    var nowHTTPHeaderItem =
                        tempSession.ResponseHeaders.FirstOrDefault(hTTPHeaderItem =>
                            hTTPHeaderItem.Name == e.ResponseHead.Key);
                    if (nowHTTPHeaderItem != null)
                    {
                        e.ResponseHead =
                            new KeyValuePair<string, string>(nowHTTPHeaderItem.Name, nowHTTPHeaderItem.Value);
                        e.SeekUri = tempSession.fullUrl;
                    }
                }
            }
        }

        private void MyFreeHttpWindow_OnShowInIndependentWindow(object sender, bool e)
        {
            if (e)
            {
                myFreeHttpWindow.FreeHttpWindowParentChanged(sender);
                var newForm = new Form();
                newForm.Icon = MyResource.freehttpico;
                newForm.Text = "FreeHttp";
                newForm.Size = tabPage.Size;
                newForm.FormClosing += (yourSender, yourE) =>
                {
                    myFreeHttpWindow.FreeHttpWindowParentChanged(sender);
                    MyControlHelper.SetControlFreeze(tabPage);
                    myFreeHttpWindow.independentWindowToolStripMenuItem.Text = "independent window";
                    tabPage.Controls.Clear();
                    tabPage.Controls.Add(myFreeHttpWindow);
                    MyControlHelper.SetControlUnfreeze(tabPage);
                };
                MyControlHelper.SetControlFreeze(newForm);
                var lb_info = new Label();
                lb_info.Text = "closing...";
                lb_info.ForeColor = Color.Blue;
                lb_info.Location = new Point((newForm.Width - lb_info.Width) / 2,
                    (newForm.Height - lb_info.Height) / 2);
                lb_info.Anchor = AnchorStyles.None;
                newForm.Controls.Add(lb_info);
                newForm.Controls.Add(myFreeHttpWindow);
                lb_info.SendToBack();
                MyControlHelper.SetControlUnfreeze(newForm);
                newForm.Show();

                var llb_info = new LinkLabel();
                llb_info.Text = "FreeHttp is in independent mode";
                llb_info.ForeColor = Color.Blue;
                llb_info.AutoSize = true;
                llb_info.Location = new Point((tabPage.Width - llb_info.Width) / 2,
                    (tabPage.Height - llb_info.Height) / 2);
                llb_info.Anchor = AnchorStyles.None;
                llb_info.LinkClicked += (yourSender, yourE) => { newForm.Activate(); };
                tabPage.Controls.Add(llb_info);

                var llb_infoRecover = new LinkLabel();
                llb_infoRecover.Text = "recover to addin mode";
                llb_infoRecover.ForeColor = Color.Blue;
                llb_infoRecover.AutoSize = true;
                llb_infoRecover.Location = new Point((tabPage.Width - llb_infoRecover.Width) / 2,
                    (tabPage.Height - llb_infoRecover.Height) / 2 + 20);
                llb_infoRecover.Anchor = AnchorStyles.None;
                llb_infoRecover.LinkClicked += (yourSender, yourE) =>
                {
                    myFreeHttpWindow.independentWindowToolStripMenuItem_Click(null, null);
                };
                tabPage.Controls.Add(llb_infoRecover);
            }
            else
            {
                (myFreeHttpWindow.Parent as Form)?.Close();
            }
        }

        private void myFreeHttpWindow_OnUpdataFromSession(object sender, EventArgs e)
        {
            var tempSession = FiddlerObject.UI.GetFirstSelectedSession();
            if (tempSession != null)
            {
                ShowMes(string.Format("Get http session in {0}", tempSession.fullUrl));
                ((FreeHttpWindow)sender).SetModificSession(tempSession);
            }
            else
            {
                FiddlerObject.UI.ShowAlert(new frmAlert("STOP", "please select a session", "OK"));
                //((FreeHttpWindow)sender).MarkWarnControl(Fiddler.FiddlerApplication.UI.Controls[0]);
                FreeHttpWindow.MarkWarnControl(FiddlerApplication.UI.lvSessions);
            }
        }

        private void FiddlerApplication_OnWebSocketMessage(object sender, WebSocketMessageEventArgs e)
        {
            //((Bitmap)((Fiddler.Session)sender).ViewItem.ImageList.Images[34]).Save(@"D:\A1.ico", System.Drawing.Imaging.ImageFormat.Icon);
            var oSession = (Session)sender;
            var webSocketMessage = e.oWSM;
            if (!isOnLoad) return;
            if (webSocketMessage == null)
            {
                AddFiddlerObjectLog("get null WebSocketMessage");
                return;
            }

            if (webSocketMessage.FrameType == WebSocketFrameTypes.Close ||
                webSocketMessage.FrameType == WebSocketFrameTypes.Ping ||
                webSocketMessage.FrameType == WebSocketFrameTypes.Pong ||
                webSocketMessage.FrameType == WebSocketFrameTypes.Reservedx3 ||
                webSocketMessage.FrameType == WebSocketFrameTypes.Reservedx4 ||
                webSocketMessage.FrameType == WebSocketFrameTypes.Reservedx5 ||
                webSocketMessage.FrameType == WebSocketFrameTypes.Reservedx6 ||
                webSocketMessage.FrameType == WebSocketFrameTypes.Reservedx7 ||
                webSocketMessage.FrameType == WebSocketFrameTypes.ReservedxB ||
                webSocketMessage.FrameType == WebSocketFrameTypes.ReservedxC ||
                webSocketMessage.FrameType == WebSocketFrameTypes.ReservedxD ||
                webSocketMessage.FrameType == WebSocketFrameTypes.ReservedxE ||
                webSocketMessage.FrameType == WebSocketFrameTypes.ReservedxF)
                return;
            if ((myFreeHttpWindow.IsRequestRuleEnable && webSocketMessage.IsOutbound) ||
                (myFreeHttpWindow.IsResponseRuleEnable && !webSocketMessage.IsOutbound))
            {
                if (IsSkipUiHide && oSession["ui-hide"] != null) return;
                if (IsSkipConnectTunnels && oSession.RequestMethod == "CONNECT") return;
                var isRequest = webSocketMessage.IsOutbound;
                List<IFiddlerHttpTamper> matchItems = null;
                if (isRequest)
                    matchItems = FiddlerSessionHelper.FindMatchTamperRule(oSession,
                        myFreeHttpWindow.FiddlerRequestChangeList, isRequest, webSocketMessage);
                else
                    //oSession.WriteResponseToStream(new MemoryStream(new Byte[] { 0x81,0x81,0x01,0x41 }), false);
                    //WebSocket ws = oSession.__oTunnel as WebSocket;
                    //ws.listMessages.Add(webSocketMessage);
                    matchItems = FiddlerSessionHelper.FindMatchTamperRule(oSession,
                        myFreeHttpWindow.FiddlerResponseChangeList, isRequest, webSocketMessage);
                if (matchItems != null && matchItems.Count > 0)
                    foreach (var matchItem in matchItems)
                    {
                        var tempListViewItem = myFreeHttpWindow.FindListViewItemFromRule(matchItem);
                        FreeHttpWindow.MarkMatchRule(tempListViewItem);
                        MarkSession(oSession);
                        ShowMes(string.Format("macth the [requst rule {0}] with {1}", tempListViewItem.SubItems[0].Text,
                            oSession.fullUrl));
                        FiddlerSessionTamper.ModificWebSocketMessage(oSession, webSocketMessage, matchItem, isRequest,
                            ShowError, ShowMes);
                        if (!isRequest)
                        {
                            var nowFiddlerResponseChange = (FiddlerResponseChange)matchItem;
                            if (nowFiddlerResponseChange.ResponseLatency > 0)
                            {
                                ShowMes(string.Format("[reponse rule {0}] is modified , now delay {1} ms",
                                    tempListViewItem.SubItems[0].Text, nowFiddlerResponseChange.ResponseLatency));
                                Thread.Sleep(nowFiddlerResponseChange.ResponseLatency);
                            }

                            if (IsOnlyMatchFistTamperRule) break;
                        }
                    }
            }
        }
    }
}