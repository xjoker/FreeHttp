﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Fiddler;
using FreeHttp.AutoTest.ParameterizationContent;
using FreeHttp.AutoTest.ParameterizationPick;
using FreeHttp.AutoTest.RunTimeStaticData;
using FreeHttp.FiddlerHelper;
using FreeHttp.HttpHelper;
using FreeHttp.MyHelper;
using FreeHttp.Resources;
using FreeHttp.WebService;
using static FreeHttp.WebService.RemoteRuleService;

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

namespace FreeHttp.FreeHttpControl
{
    public partial class FreeHttpWindow : UserControl
    {
        public delegate void GetSessionRawDataEventHandler(object sender, GetSessionRawDataEventArgs e);

        public enum GetSessionAction
        {
            ShowShowResponse,
            SetCookies,
            DeleteCookies
        }

        /// <summary>
        ///     Http modific mode
        /// </summary>
        public enum RuleEditMode
        {
            NewRuleMode,
            EditRequsetRule,
            EditResponseRule
        }

        //fiddlerModificHttpRuleCollection不保持最新数据集合，仅保持最后一次InitializeConfigInfo的rule数据关系
        private FiddlerModificHttpRuleCollection fiddlerModificHttpRuleCollection;
        private bool isLoadFreeHttpWindowUserControl;
        private bool isSetResponseLatencyEable;
        private readonly List<RuleInfoWindow> nowRuleInfoWindowList = new List<RuleInfoWindow>();

        private readonly PictureBox ShowRuleInfo_pb = new PictureBox
            { Cursor = Cursors.Hand, SizeMode = PictureBoxSizeMode.StretchImage };

        public FreeHttpWindow()
        {
            InitializeComponent();
            MyInitializeComponent();
            //this.DoubleBuffered = true;
            SetStyle(
                ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint,
                true);
            UpdateStyles();
        }

        /// <summary>
        ///     FreeHttpWindow
        /// </summary>
        /// <param name="yourRuleCollection">the history rule</param>
        public FreeHttpWindow(FiddlerModificHttpRuleCollection yourRuleCollection,
            FiddlerModificSettingInfo yourModifcSettingInfo, ActuatorStaticDataCollection yourStaticDataCollection,
            FiddlerRuleGroup yourFiddlerRuleGroup)
            : this()
        {
            InitializeConfigInfo(yourRuleCollection, yourModifcSettingInfo, yourStaticDataCollection,
                yourFiddlerRuleGroup);
            if (!rawResponseEdit.SetContextMenuStrip(contextMenuStrip_AddFile))
                MessageBox.Show("RawResponseEdit SetContextMenuStrip fail");
            FreeHttpWindow_Load(null, null);
        }

        //public 

        /// <summary>
        ///     get or set ModificSettingInfo
        /// </summary>
        public FiddlerModificSettingInfo ModificSettingInfo { get; set; }

        /// <summary>
        ///     get or set ModificSettingInfo
        /// </summary>
        public ActuatorStaticDataCollection StaticDataCollection { get; set; }

        /// <summary>
        ///     get FiddlerModificHttpRuleCollection
        /// </summary>
        public FiddlerModificHttpRuleCollection ModificHttpRuleCollection =>
            new FiddlerModificHttpRuleCollection(FiddlerRequestChangeList, FiddlerResponseChangeList);

        /// <summary>
        ///     get or set ModificRuleGroup(如果使用=重置组信息请重新建立其与ListView的关系)
        /// </summary>
        public FiddlerRuleGroup ModificRuleGroup { get; set; }

        /// <summary>
        ///     get or set IsSetResponseLatencyEable
        /// </summary>
        public bool IsSetResponseLatencyEable
        {
            get => isSetResponseLatencyEable;
            private set
            {
                isSetResponseLatencyEable = value;
                ChangeSetResponseLatencyMode(value ? 0 : -1);
            }
        }

        /// <summary>
        ///     Is Request Rule Enable
        /// </summary>
        public bool IsRequestRuleEnable
        {
            get => ModificSettingInfo.IsEnableRequestRule;
            private set => ModificSettingInfo.IsEnableRequestRule = value;
        }

        /// <summary>
        ///     Is Response Rule Enable
        /// </summary>
        public bool IsResponseRuleEnable
        {
            get => ModificSettingInfo.IsEnableResponseRule;
            private set => ModificSettingInfo.IsEnableResponseRule = value;
        }

        /// <summary>
        ///     Get the RequestRule ListView (not add or del item in your code , if you want change the item just use exist
        ///     function)
        /// </summary>
        public ListView RequestRuleListView => lv_requestRuleList;

        /// <summary>
        ///     Get the ResponseRule ListView (not add or del item in your code , if you want change the item just use exist
        ///     function)
        /// </summary>
        public ListView ResponseRuleListView => lv_responseRuleList;


        /// <summary>
        ///     Get latest FiddlerRequestChange list
        /// </summary>
        public List<FiddlerRequestChange> FiddlerRequestChangeList { get; private set; }

        /// <summary>
        ///     Get latest FiddlerResponseChange list
        /// </summary>
        public List<FiddlerResponseChange> FiddlerResponseChangeList { get; private set; }


        /// <summary>
        ///     Get edit ListViewItem (if it is not in edit mode return null)
        /// </summary>
        public ListViewItem EditListViewItem { get; private set; }

        /// <summary>
        ///     Get now edit mode
        /// </summary>
        public RuleEditMode NowEditMode { get; private set; } = RuleEditMode.NewRuleMode;

        /// <summary>
        ///     Get now protocol mode
        /// </summary>
        public TamperProtocalType NowProtocalMode { get; private set; } = TamperProtocalType.Http;

        private void MyInitializeComponent()
        {
            ShowRuleInfo_pb.Image = MyResource.show;
            ShowRuleInfo_pb.MouseLeave += pictureBox_MouseLeave;
            ShowRuleInfo_pb.MouseMove += pictureBox_MouseMove;
            ShowRuleInfo_pb.Click += ShowRuleInfo_pb_Click;
        }

        /// <summary>
        ///     updata reference relationship（ActuatorStaticDataCollection 与 HttpRule 这这里需要重建引用关系）
        /// </summary>
        /// <param name="yourRuleCollection"></param>
        /// <param name="yourModifcSettingInfo"></param>
        /// <param name="yourStaticDataCollection"></param>
        /// <param name="yourFiddlerRuleGroup"></param>
        private void InitializeConfigInfo(FiddlerModificHttpRuleCollection yourRuleCollection,
            FiddlerModificSettingInfo yourModifcSettingInfo, ActuatorStaticDataCollection yourStaticDataCollection,
            FiddlerRuleGroup yourFiddlerRuleGroup)
        {
            fiddlerModificHttpRuleCollection = yourRuleCollection;
            ModificSettingInfo = yourModifcSettingInfo;
            if (ModificSettingInfo != null) ModificSettingInfo.IsSyncTamperRule = true;
            if (ModificSettingInfo != null && ModificSettingInfo.UserToken != null)
                UserComputerInfo.UserToken = ModificSettingInfo.UserToken;
            StaticDataCollection = yourStaticDataCollection;
            ModificRuleGroup = yourFiddlerRuleGroup;
            if (fiddlerModificHttpRuleCollection != null && StaticDataCollection != null)
            {
                foreach (var fr in fiddlerModificHttpRuleCollection.ResponseRuleList)
                {
                    fr.ActuatorStaticDataController =
                        new FiddlerActuatorStaticDataCollectionController(StaticDataCollection);
                    if (fr.IsRawReplace)
                        if (fr.HttpRawResponse.ParameterizationContent == null)
                            fr.HttpRawResponse.ParameterizationContent =
                                new CaseParameterizationContent(fr.HttpRawResponse.OriginSting);
                    fr.SetHasParameter(fr.IsHasParameter, StaticDataCollection);
                }

                foreach (var fr in fiddlerModificHttpRuleCollection.RequestRuleList)
                {
                    if (fr.IsRawReplace)
                        if (fr.HttpRawRequest.ParameterizationContent == null)
                            fr.HttpRawRequest.ParameterizationContent =
                                new CaseParameterizationContent(fr.HttpRawRequest.OriginSting);
                    fr.SetHasParameter(fr.IsHasParameter, StaticDataCollection);
                }
            }
        }


        /// <summary>
        ///     On get the http session button click
        /// </summary>
        public event EventHandler OnUpdataFromSession;

        /// <summary>
        ///     On get the raw http data link click   (EventHandler<GetSessionRawDataEventArgs>)
        /// </summary>
        public event GetSessionRawDataEventHandler OnGetSessionRawData;

        /// <summary>
        ///     find your seek head vaule in session (only use in synchronization)
        /// </summary>
        public event EventHandler<GetSessionSeekHeadEventArgs> OnGetSessionSeekHead;

        /// <summary>
        ///     get select session info show in
        /// </summary>
        public event EventHandler<GetSessionEventArgs> OnGetSessionEventArgs;

        /// <summary>
        ///     when the freehttp want show independent
        /// </summary>
        public event EventHandler<bool> OnShowInIndependentWindow;

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void FreeHttpWindow_Load(object sender, EventArgs e)
        {
            if (isLoadFreeHttpWindowUserControl) return;
            try
            {
                LoadFiddlerModificHttpRuleCollection(fiddlerModificHttpRuleCollection);
            }
            catch (Exception ex)
            {
                var errorMes = string.Format("{0}\r\n{1}", ex.Message,
                    ex.InnerException == null ? "" : ex.InnerException.Message);
                _ = RemoteLogService.ReportLogAsync(errorMes, RemoteLogService.RemoteLogOperation.WindowLoad,
                    RemoteLogService.RemoteLogType.Error);
                MessageBox.Show(errorMes, "load user rule fail");
                if (File.Exists("RuleData.xml")) File.Copy("RuleData.xml", "RuleData.lastErrorFile", true);
            }

            if (FiddlerRequestChangeList == null) FiddlerRequestChangeList = new List<FiddlerRequestChange>();
            if (FiddlerResponseChangeList == null) FiddlerResponseChangeList = new List<FiddlerResponseChange>();
            if (StaticDataCollection == null) StaticDataCollection = new ActuatorStaticDataCollection(true);
            if (ModificRuleGroup == null)
            {
                ModificRuleGroup = new FiddlerRuleGroup(lv_requestRuleList, lv_responseRuleList);
            }
            else
            {
                ModificRuleGroup.SetRuleGroupListView(lv_requestRuleList, lv_responseRuleList);
                ModificRuleGroup.RecoverGroup();
            }

            if (ModificSettingInfo == null) ModificSettingInfo = new FiddlerModificSettingInfo(true, false, true, true);
            if (ModificSettingInfo.IsEnableRequestRule) pb_requestRuleSwitch.Image = MyResource.switch_on;
            if (ModificSettingInfo.IsEnableResponseRule) pb_responseRuleSwitch.Image = MyResource.switch_on;

            MyGlobalHelper.OnGetGlobalMessage += (obj, arg) => { PutWarn(arg.Message); };

            tbe_RequestBodyModific.Visible = false;
            tbe_ResponseBodyModific.Visible = false;
            tbe_urlFilter.Visible = false;
            lv_requestRuleList.SetGroupState(ListViewGroupState.Collapsible);
            lv_responseRuleList.SetGroupState(ListViewGroupState.Collapsible);
            lv_responseRuleList.OnItemDragSortEnd += Lv_ruleList_OnItemDragSort;
            lv_requestRuleList.OnItemDragSortEnd += Lv_ruleList_OnItemDragSort;
            lv_requestRuleList.OnItemDragSortStart += Lv_ruleList_OnItemDragSortStart;
            lv_responseRuleList.OnItemDragSortStart += Lv_ruleList_OnItemDragSortStart;
            tbe_RequestBodyModific.OnCloseEditBox += tbe_BodyModific_OnCloseEditBox;
            tbe_ResponseBodyModific.OnCloseEditBox += tbe_BodyModific_OnCloseEditBox;
            tbe_urlFilter.OnCloseEditBox += tbe_BodyModific_OnCloseEditBox;

            cb_macthMode.SelectedIndex = 0;
            tabControl_Modific.SelectedTab = tabPage_requestModific;
            IsSetResponseLatencyEable = false;

            //rtb_MesInfo.AllowDrop = true;
            //rtb_MesInfo.DragEnter += rtb_MesInfo_DragEnter;
            //rtb_MesInfo.DragDrop += rtb_MesInfo_DragDrop;

            splitContainer_httpEditInfo.AllowDrop = true;
            splitContainer_httpEditInfo.DragEnter += rtb_MesInfo_DragEnter;
            splitContainer_httpEditInfo.DragDrop += rtb_MesInfo_DragDrop;

            panel_modific_Contorl.AllowDrop = true;
            panel_modific_Contorl.DragEnter += rtb_MesInfo_DragEnter;
            panel_modific_Contorl.DragDrop += rtb_MesInfo_DragDrop;

            Action dropAction = () => { pb_parameterSwitch.SwitchState = true; };
            MyControlHelper.SetRichTextBoxDropString(rtb_requsetReplace_body, dropAction);
            MyControlHelper.SetRichTextBoxDropString(rtb_requestRaw, dropAction);
            MyControlHelper.SetRichTextBoxDropString(rtb_requestModific_body, dropAction);
            MyControlHelper.SetRichTextBoxDropString(rtb_respenseModific_body, dropAction);

            MyControlHelper.SetRichTextBoxDropString(tb_requestModific_body, dropAction);
            MyControlHelper.SetRichTextBoxDropString(tb_requestModific_uriModificKey, dropAction);
            MyControlHelper.SetRichTextBoxDropString(tb_requestModific_uriModificValue, dropAction);
            MyControlHelper.SetRichTextBoxDropString(tb_responseModific_body, dropAction);

            isLoadFreeHttpWindowUserControl = true;
        }

        internal void FreeHttpWindowSelectedChanged(bool isInFreeHttpWindowSelected)
        {
            if (Parent is Form) return;
            if (nowRuleInfoWindowList == null || nowRuleInfoWindowList.Count == 0) return;
            for (var i = nowRuleInfoWindowList.Count - 1; i >= 0; i--)
            {
                if (nowRuleInfoWindowList[i].IsDisposed)
                {
                    nowRuleInfoWindowList.RemoveAt(i);
                    continue;
                }

                nowRuleInfoWindowList[i].Visible = isInFreeHttpWindowSelected;
            }
        }

        internal void FreeHttpWindowParentChanged(object sender)
        {
            if (nowRuleInfoWindowList == null || nowRuleInfoWindowList.Count == 0) return;
            for (var i = nowRuleInfoWindowList.Count - 1; i >= 0; i--)
                if (!nowRuleInfoWindowList[i].IsDisposed)
                    nowRuleInfoWindowList[i].Close();
            nowRuleInfoWindowList.Clear();
        }

        #region RequestReplace

        private void pb_requestReplace_changeMode_Click(object sender, EventArgs e)
        {
            if (panel_requestReplace_startLine.Visible)
            {
                panel_requestReplace_startLine.Visible = splitContainer_requestReplace.Visible = false;
                pb_requestReplace_changeMode.Visible = true;
                tabPage_requestReplace.Controls.Add(pb_requestReplace_changeMode);
                toolTip_forMainWindow.SetToolTip(pb_requestReplace_changeMode,
                    "change request replace mode to format mode");
                pb_requestReplace_changeMode.BringToFront();
            }
            else
            {
                panel_requestReplace_startLine.Visible = splitContainer_requestReplace.Visible = true;
                panel_requestReplace_startLine.Controls.Add(pb_requestReplace_changeMode);
                toolTip_forMainWindow.SetToolTip(pb_requestReplace_changeMode,
                    "change request replace mode to raw mode");
            }
        }

        #endregion

        public class GetSessionRawDataEventArgs : EventArgs
        {
            public GetSessionRawDataEventArgs(GetSessionAction sessionAction)
            {
                SessionAction = sessionAction;
            }

            public GetSessionAction SessionAction { get; set; }
        }

        public class GetSessionEventArgs
        {
            public GetSessionEventArgs(bool isGetEntity)
            {
                IsGetEntity = isGetEntity;
            }

            public bool IsGetSuccess { get; set; } = false;
            public string Uri { get; set; }
            public List<KeyValuePair<string, string>> RequestHeads { get; set; }
            public string RequestEntity { get; set; }
            public List<KeyValuePair<string, string>> ResponseHeads { get; set; }
            public string ResponseEntity { get; set; }
            public bool IsGetEntity { get; }
        }

        public class GetSessionSeekHeadEventArgs : EventArgs
        {
            public GetSessionSeekHeadEventArgs(KeyValuePair<string, string> resquestHead,
                KeyValuePair<string, string> responseHead)
            {
                ResquestHead = resquestHead;
                ResponseHead = responseHead;
            }

            public string SeekUri { get; set; }
            public KeyValuePair<string, string> ResquestHead { get; set; }
            public KeyValuePair<string, string> ResponseHead { get; set; }
        }

        #region Public Event

        private void tabControl_Modific_Selecting(object sender, TabControlCancelEventArgs e)
        {
            if (e == null) return; // e为了null 即为应用调用，不用检查tab状态
            if (!((((TabControl)sender).TabPages.Count == 2 &&
                   ((TabControl)sender).TabPages[0] == tabPage_requestModific &&
                   ((TabControl)sender).TabPages[1] == tabPage_responseModific) ||
                  ((TabControl)sender).TabPages.Count == 4)) return;
            Action cancelChange = () =>
            {
                MarkControl(pb_ruleCancel, Color.Plum, 2);
                MarkControl(pb_ruleComfrim, Color.Plum, 2);
                e.Cancel = true;
            };

            if (NowEditMode == RuleEditMode.EditRequsetRule)
            {
                if ((((TabControl)sender).SelectedTab == tabPage_responseModific ||
                     ((TabControl)sender).SelectedTab == tabPage_responseReplace) &&
                    NowProtocalMode == TamperProtocalType.Http)
                {
                    MessageBox.Show(
                        "the select requst rule is in editing (that pink rule in rule list) \r\nyou should save or cancel this editing before edit response",
                        "STOP");
                    cancelChange();
                }

                if (((TabControl)sender).SelectedTab != tabPage_requestModific &&
                    NowProtocalMode == TamperProtocalType.WebSocket)
                {
                    MessageBox.Show(
                        "the select websocket requst rule is in editing (that pink rule in rule list) \r\nyou should save or cancel this editing before edit response",
                        "STOP");
                    cancelChange();
                }
            }
            else if (NowEditMode == RuleEditMode.EditResponseRule)
            {
                if ((((TabControl)sender).SelectedTab == tabPage_requestModific ||
                     ((TabControl)sender).SelectedTab == tabPage_requestReplace) &&
                    NowProtocalMode == TamperProtocalType.Http)
                {
                    MessageBox.Show(
                        "the select response rule is in editing (that pink rule in rule list)\r\nyou should save or cancel this editing before edit requst",
                        "STOP");
                    cancelChange();
                }

                if (((TabControl)sender).SelectedTab != tabPage_responseModific &&
                    NowProtocalMode == TamperProtocalType.WebSocket)
                {
                    MessageBox.Show(
                        "the select websocket response rule is in editing (that pink rule in rule list) \r\nyou should save or cancel this editing before edit response",
                        "STOP");
                    cancelChange();
                }
            }
            else
            {
                if ((((TabControl)sender).SelectedTab == tabPage_requestReplace ||
                     ((TabControl)sender).SelectedTab == tabPage_responseReplace) &&
                    NowProtocalMode == TamperProtocalType.WebSocket)
                {
                    MessageBox.Show("websocket tamper rule not need use replace mode", "STOP");
                    e.Cancel = true;
                }

                if (((TabControl)sender).SelectedTab == tabPage_requestModific ||
                    ((TabControl)sender).SelectedTab == tabPage_requestReplace)
                    IsSetResponseLatencyEable = false;
                else
                    IsSetResponseLatencyEable = true;
            }
        }

        private void addFileToolStripMenuItem_Click(object sender, EventArgs e)
        {
            object sourceControl = ((ContextMenuStrip)((ToolStripMenuItem)sender).Owner).SourceControl;
            RichTextBox tempRtb = null;
            if (sourceControl == rtb_requsetReplace_body)
            {
                tempRtb = rtb_requsetReplace_body;
            }
            else if (sourceControl == rtb_requestRaw)
            {
                tempRtb = rtb_requestRaw;
            }
            else
            {
                tempRtb = sourceControl as RichTextBox;
                if (tempRtb == null)
                {
                    //throw new Exception("not adapt this event");
                    MessageBox.Show("get file fail , please try again or add manually");
                    return;
                }
            }

            if (openFileDialog_addFIle.ShowDialog() == DialogResult.OK)
            {
                var tempPath = openFileDialog_addFIle.FileName;
                var tempIndex = tempRtb.Text.IndexOf("<<replace file path>>");
                if (tempIndex >= 0) tempRtb.Text = tempRtb.Text.Remove(tempIndex);

                if (!tempRtb.Text.EndsWith("\n\n") && tempRtb != rtb_requsetReplace_body)
                {
                    if (tempRtb.Text.EndsWith("\n"))
                        tempRtb.AppendText("\n");
                    else
                        tempRtb.AppendText("\n\n");
                }

                tempRtb.AppendText(string.Format("<<replace file path>>{0}", tempPath));
            }
        }

        private void contextMenuStrip_addParameter_Opening(object sender, CancelEventArgs e)
        {
            ((ToolStripDropDown)sender).Tag = ((ToolStripDropDown)sender).OwnerItem;
        }

        private void contextMenuStrip_AddFile_Opening(object sender, CancelEventArgs e)
        {
            ((ContextMenuStrip)sender).Tag = ((ContextMenuStrip)sender).SourceControl;
            if (((ContextMenuStrip)sender).SourceControl == rtb_requestModific_body ||
                ((ContextMenuStrip)sender).SourceControl == rtb_respenseModific_body)
            {
                addFileToolStripMenuItem.Enabled = false;
                antoContentLengthToolStripMenuItem.Enabled = false;
                antoContentLengthToolStripMenuItem.Checked = true;
            }
            else
            {
                addFileToolStripMenuItem.Enabled = true;
                antoContentLengthToolStripMenuItem.Enabled = true;
                //antoContentLengthToolStripMenuItem.Checked = true;
            }
        }

        private void addParameterDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            string additionStr = null;
            string addParameterStr = null;
            if (sender == currentValueToolStripMenuItem)
                additionStr = "(=)";
            else if (sender == nextValueToolStripMenuItem)
                additionStr = "(+)";
            else if (sender == previousValueToolStripMenuItem)
                additionStr = "(+2)";
            else
                return;
            if (((ToolStripItem)sender).Owner.Tag == null)
            {
                MessageBox.Show("add parameter fail ,please add manually");
                return;
            }

            var tempTag = ((ToolStripItem)sender).Owner.Tag;
            addParameterStr = string.Format("*#{0}{1}*#", ((ToolStripItem)tempTag).Text, additionStr);
            //there is a bug in dot net 4.5 when call SourceControl (https://github.com/Microsoft/dotnet/issues/434 )
            //RichTextBox tempRichTextBox = ((ContextMenuStrip)((((ToolStripItem)(tempTag)).OwnerItem).OwnerItem.Owner)).SourceControl as RichTextBox;
            var tempRichTextBox =
                ((ContextMenuStrip)((ToolStripItem)tempTag).OwnerItem.OwnerItem.Owner).Tag as RichTextBox;
            if (tempRichTextBox == null)
            {
                MessageBox.Show("add parameter fail ,please add manually");
                return;
            }

            var selectionStart = tempRichTextBox.SelectionStart;
            tempRichTextBox.Text = tempRichTextBox.Text.Insert(selectionStart, addParameterStr);
            tempRichTextBox.Select(selectionStart, addParameterStr.Length);
            pb_parameterSwitch.SwitchState = true;
        }


        private void addParameterDataToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            keyValueToolStripMenuItem.DropDownItems.Clear();
            parameterToolStripMenuItem.DropDownItems.Clear();
            dataSouceToolStripMenuItem.DropDownItems.Clear();
            if (StaticDataCollection == null) return;
            if (StaticDataCollection.RunActuatorStaticDataKeyList != null &&
                StaticDataCollection.RunActuatorStaticDataKeyList.Count > 0)
            {
                foreach (var tempItem in StaticDataCollection.RunActuatorStaticDataKeyList)
                {
                    //keyValueToolStripMenuItem.DropDownItems.Add(tempItem.Key);
                    var tempTmi = new ToolStripMenuItem(tempItem.Key);
                    tempTmi.DropDown = contextMenuStrip_addParameter;
                    keyValueToolStripMenuItem.DropDownItems.Add(tempTmi);
                }

                keyValueToolStripMenuItem.Enabled = true;
            }
            else
            {
                keyValueToolStripMenuItem.Enabled = false;
            }

            if (StaticDataCollection.RunActuatorStaticDataParameterList != null &&
                StaticDataCollection.RunActuatorStaticDataParameterList.Count > 0)
            {
                foreach (var tempItem in StaticDataCollection.RunActuatorStaticDataParameterList)
                {
                    //parameterToolStripMenuItem.DropDownItems.Add(tempItem.Key);
                    var tempTmi = new ToolStripMenuItem(tempItem.Key);
                    tempTmi.DropDown = contextMenuStrip_addParameter;
                    parameterToolStripMenuItem.DropDownItems.Add(tempTmi);
                }

                parameterToolStripMenuItem.Enabled = true;
            }
            else
            {
                parameterToolStripMenuItem.Enabled = false;
            }

            if (StaticDataCollection.RunActuatorStaticDataSouceList != null &&
                StaticDataCollection.RunActuatorStaticDataSouceList.Count > 0)
            {
                foreach (var tempItem in StaticDataCollection.RunActuatorStaticDataSouceList)
                {
                    //dataSouceToolStripMenuItem.DropDownItems.Add(tempItem.Key);
                    var tempTmi = new ToolStripMenuItem(tempItem.Key);
                    tempTmi.DropDown = contextMenuStrip_addParameter;
                    dataSouceToolStripMenuItem.DropDownItems.Add(tempTmi);
                }

                dataSouceToolStripMenuItem.Enabled = true;
            }
            else
            {
                dataSouceToolStripMenuItem.Enabled = false;
            }
        }

        private void antoContentLengthToolStripMenuItem_Click(object sender, EventArgs e)
        {
            antoContentLengthToolStripMenuItem.Checked = !antoContentLengthToolStripMenuItem.Checked;
        }

        private void pictureBox_editRuleMode_Click(object sender, EventArgs e)
        {
            pb_ruleComfrim_Click(sender, e);
        }

        private void pictureBox_editHttpFilter_Click(object sender, EventArgs e)
        {
            pictureBox_editHttpFilter.Tag = GetHttpFilter();

            var f = new HttpFilterWindow(pictureBox_editHttpFilter.Tag, NowProtocalMode);
            f.ShowDialog();

            if (((FiddlerHttpFilter)pictureBox_editHttpFilter.Tag).HeadMatch != null ||
                ((FiddlerHttpFilter)pictureBox_editHttpFilter.Tag).BodyMatch != null)
                pictureBox_editHttpFilter.Image = MyResource.filter_on;
            else
                pictureBox_editHttpFilter.Image = MyResource.filter_off;
            SetUriMatch(((FiddlerHttpFilter)pictureBox_editHttpFilter.Tag).UriMatch);
        }

        //pictureBox change for all
        public void pictureBox_MouseMove(object sender, MouseEventArgs e)
        {
            ((PictureBox)sender).BackColor = Color.Honeydew;
        }

        //pictureBox change for all
        public void pictureBox_MouseLeave(object sender, EventArgs e)
        {
            ((PictureBox)sender).BackColor = Color.Transparent;
        }


        private void tb_Modific_body_Enter(object sender, EventArgs e)
        {
            TextBoxEditer tbe;
            if (sender == tb_requestModific_body)
                tbe = tbe_RequestBodyModific;
            else if (sender == tb_responseModific_body)
                tbe = tbe_ResponseBodyModific;
            else if (sender == tb_urlFilter)
                tbe = tbe_urlFilter;
            else
                throw new Exception("nonsupport sender in tb_Modific_body_Enter");
            tbe.Visible = true;
        }

        private void tb_Modific_body_Leave(object sender, EventArgs e)
        {
            TextBoxEditer tbe = null;
            if (sender == tb_requestModific_body)
                tbe = tbe_RequestBodyModific;
            else if (sender == tb_responseModific_body)
                tbe = tbe_ResponseBodyModific;
            else if (sender == tb_urlFilter)
                tbe = tbe_urlFilter;
            else
                throw new Exception("nonsupport sender in tb_Modific_body_Enter");

            if (!tbe.IsShowEditRichTextBox) tbe.Visible = false;
        }

        private void tbe_BodyModific_OnCloseEditBox(object sender, TextBoxEditer.CloseEditBoxEventArgs e)
        {
            ((TextBoxEditer)sender).Visible = false;
        }

        private void tabControl_Modific_Resize(object sender, EventArgs e)
        {
            tb_urlFilter.Width = tabControl_Modific.Width - 264;

            //tabPage_requestModific
            requestRemoveHeads.Width = (tabControl_Modific.Width - 22) / 3;
            requestAddHeads.Width = (tabControl_Modific.Width - 22) * 2 / 3;

            tb_requestModific_uriModificValue.Width = tabControl_Modific.Width - 126;
            tb_requestModific_body.Width = tabControl_Modific.Width - 92;

            //tabPage_requestReplace
            tb_requestReplace_uri.Width = tabControl_Modific.Width - 289;

            //tabPage_reponseModific
            responseRemoveHeads.Width = (tabControl_Modific.Width - 22) / 3;
            responseAddHeads.Width = (tabControl_Modific.Width - 22) * 2 / 3;

            tb_responseModific_body.Width = tabControl_Modific.Width - 92;
        }

        private void splitContainer_httpControl_Resize(object sender, EventArgs e)
        {
            //rule list
            //- (lv_requestRuleList.Groups.Count > 0 ? 5 : 0)
            columnHeader_requstRule.Width = lv_requestRuleList.Width - 75;
            columnHeader_responseRule.Width = lv_responseRuleList.Width - 75;
        }

        #endregion

        #region Modific ContorLine

        private void pb_ruleComfrim_Click(object sender, EventArgs e)
        {
            FiddlerRequestChange nowRequestChange = null;
            FiddlerResponseChange nowResponseChange = null;
            IFiddlerHttpTamper fiddlerHttpTamper = null;
            ListView tamperRuleListView = null;

            try
            {
                if (tabControl_Modific.SelectedTab == tabPage_requestModific)
                    nowRequestChange = GetRequestModificInfo();
                else if (tabControl_Modific.SelectedTab == tabPage_requestReplace)
                    nowRequestChange = GetRequestReplaceInfo();
                else if (tabControl_Modific.SelectedTab == tabPage_responseModific)
                    nowResponseChange = GetResponseModificInfo();
                else if (tabControl_Modific.SelectedTab == tabPage_responseReplace)
                    nowResponseChange = GetResponseReplaceInfo();
                else
                    throw new Exception("unknow http tamper tab");
            }
            catch (Exception ex)
            {
                _ = RemoteLogService.ReportLogAsync(ex.ToString(), RemoteLogService.RemoteLogOperation.AddRule,
                    RemoteLogService.RemoteLogType.Error);
                MessageBox.Show(ex.Message, "Stop", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                PutError($"add rule fail :{ex.Message}");
                MarkControl(tabControl_Modific.SelectedTab, Color.Plum, 2);
                nowRequestChange = null;
                nowResponseChange = null;
            }
            finally
            {
                if (nowRequestChange != null)
                {
                    fiddlerHttpTamper = nowRequestChange;
                    tamperRuleListView = lv_requestRuleList;
                }
                else if (nowResponseChange != null)
                {
                    fiddlerHttpTamper = nowResponseChange;
                    tamperRuleListView = lv_responseRuleList;
                }
            }

            if (fiddlerHttpTamper == null) return;

            if (fiddlerHttpTamper.HttpFilter == null || fiddlerHttpTamper.HttpFilter.UriMatch == null)
            {
                MessageBox.Show("you Uri Filter is not legal \r\n check it again", "edit again");
                MarkControl(groupBox_urlFilter, Color.Plum, 2);
                return;
            }

            ListViewItem nowRuleItem = null;
            if (NowEditMode == RuleEditMode.NewRuleMode) //编辑模式不检查重复Filter，如果需要检查去掉if直接执行{}内逻辑
                foreach (ListViewItem tempItem in tamperRuleListView.Items)
                {
                    if (tempItem == EditListViewItem) continue;
                    //if (fiddlerHttpTamper.HttpFilter.UriMatch.Equals(tempItem.Tag))
                    if (fiddlerHttpTamper.HttpFilter.Equals(tempItem.Tag))
                    {
                        MarkRuleItem(tempItem, Color.Plum, 2);
                        DialogResult tempDs;
                        //add mode
                        if (EditListViewItem == null)
                        {
                            tempDs = MessageBox.Show(
                                string.Format(
                                    "find same url filter with [Rule:{0}], do you want create the same uri rule \r\n    [Yes]       new a same url filter rule \r\n    [No]       update the rule \r\n    [Cancel]  give up save",
                                    tempItem.SubItems[0].Text), "find same rule ", MessageBoxButtons.YesNoCancel,
                                MessageBoxIcon.Question);
                            if (tempDs == DialogResult.Yes) continue;

                            if (tempDs == DialogResult.No)
                            {
                                nowRuleItem = tempItem;
                                SyncEnableSateToIFiddlerHttpTamper(nowRuleItem, fiddlerHttpTamper);
                                UpdataRuleToListView(nowRuleItem, fiddlerHttpTamper, true);
                                break;
                            }

                            return;
                        }
                        //edit mode

                        tempDs = MessageBox.Show(
                            string.Format(
                                "find same uri filter with [Rule:{0}], do you want save the rule \r\n    [Yes]       skip the same uri filter rule \r\n    [No]       remove the rule \r\n    [Cancel]  give up save",
                                tempItem.SubItems[0].Text), "find same rule ", MessageBoxButtons.YesNoCancel,
                            MessageBoxIcon.Question);
                        if (tempDs == DialogResult.Yes) continue;

                        if (tempDs == DialogResult.No)
                        {
                            DelRuleFromListView(tamperRuleListView, tempItem);
                            //tamperRuleListView.Items.Remove(tempItem);
                            continue;
                        }

                        return;
                    }
                }

            if (nowRuleItem == null)
            {
                if (EditListViewItem == null)
                {
                    AddRuleToListView(tamperRuleListView, fiddlerHttpTamper, true);
                }
                else
                {
                    SyncEnableSateToIFiddlerHttpTamper(EditListViewItem, fiddlerHttpTamper);
                    UpdataRuleToListView(EditListViewItem, fiddlerHttpTamper, true);
                }
            }

            ChangeNowRuleMode(RuleEditMode.NewRuleMode, NowProtocalMode, null, null);
        }

        private void pb_ruleCancel_Click(object sender, EventArgs e)
        {
            PutWarn("Clear the Modific Info");
            ChangeNowRuleMode(RuleEditMode.NewRuleMode, NowProtocalMode, null, null);
        }

        private void pb_protocolSwitch_Click(object sender, EventArgs e)
        {
            if (NowEditMode != RuleEditMode.NewRuleMode)
                if (DialogResult.Cancel ==
                    MessageBox.Show(
                        "your are in EditMode now \r\nchange protocol mode will discard your change for this rule",
                        "change protocol mode", MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
                    return;
            ChangeNowRuleMode(RuleEditMode.NewRuleMode,
                ((MySwitchPictureButton)sender).SwitchState ? TamperProtocalType.WebSocket : TamperProtocalType.Http,
                null, null);
        }

        private void pb_responseLatency_Click(object sender, EventArgs e)
        {
            if (IsSetResponseLatencyEable)
            {
                var f = new SetVaule("Set Latency",
                    "Enter the exact number of milliseconds by which to delay the response",
                    sender == pb_responseLatency ? "0" : lbl_ResponseLatency.GetLatency().ToString(), checkValue =>
                    {
                        int tempValue;
                        if (checkValue == "") return null;
                        return int.TryParse(checkValue, out tempValue) && tempValue >= 0 ? null : "";
                    });
                f.OnSetValue += f_OnSetValue;
                f.ShowDialog();
            }
            else
            {
                MessageBox.Show("Can not set latency  in reqest modific mode\r\njust change to response modific mode",
                    "Stop", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
        }

        private void pb_pickRule_Click(object sender, EventArgs e)
        {
            EditParameterPickWindow f;
            if (pb_pickRule.Tag is List<ParameterPick>)
                f = new EditParameterPickWindow((List<ParameterPick>)pb_pickRule.Tag, SetHttpParameterPick);
            else
                f = new EditParameterPickWindow(null, SetHttpParameterPick);
            f.StartPosition = FormStartPosition.CenterParent;
            f.ShowDialog();
        }

        private void f_OnSetValue(object sender, SetVaule.SetVauleEventArgs e)
        {
            if (e.SetValue == null || e.SetValue == "0" || e.SetValue == "")
                ChangeSetResponseLatencyMode(0);
            else
                //impossible to exception throw
                ChangeSetResponseLatencyMode(int.Parse(e.SetValue));
        }

        private void disableCacheToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (NowEditMode == RuleEditMode.EditResponseRule)
            {
                //DialogResult dialogResult ;
                if (MessageBox.Show(
                        "your are in Response Edit Mode.\r\ndo you want give up the editing and new a rule to continue this quick rule?",
                        "Continue or not", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    pb_ruleCancel_Click(this, e);
                else
                    return;
            }

            tabControl_Modific.SelectedTab = tabPage_requestModific;
            requestRemoveHeads.ListDataView.Items.Add("Pragma");
            //requestRemoveHeads.ListDataView.Items.Add("Expires");//If there is a Cache-Control header with the max-age or s-maxage directive in the response, the Expires header is ignored.   <Expires: Wed, 21 Oct 2018 15:28:00 GMT>   Expires是一个响应头
            requestRemoveHeads.ListDataView.Items.Add("Cache-Control");
            //requestRemoveHeads.ListDataView.Items.Add("ETag");//ETag 也是HTTP响应头。 与If-None-Match 完成缓存验证 ，与 If-Match 请求头部，检测到"空中碰撞"的编辑冲突。
            //requestRemoveHeads.ListDataView.Items.Add("Last-Modified");//Last-Modified  是一个响应首部，其中包含源头服务器认定的资源做出修改的日期及时间。 它通常被用作一个验证器来判断接收到的或者存储的资源是否彼此一致。由于精确度比  ETag 要低，所以这是一个备用机制。包含有  If-Modified-Since 或 If-Unmodified-Since 首部的条件请求会使用这个字段。
            requestRemoveHeads.ListDataView.Items.Add("If-None-Match");
            requestRemoveHeads.ListDataView.Items.Add("If-Modified-Since");
            requestAddHeads.ListDataView.Items.Add("Pragma: no-cache");
            requestAddHeads.ListDataView.Items.Add("Cache-Control: no-cache");

            if (tb_urlFilter.Text == "")
            {
                var sessionArgs = GetNowHttpSession();
                if (sessionArgs.IsGetSuccess && sessionArgs.Uri != null)
                    SetUriMatch(new FiddlerUriMatch(FiddlerUriMatchMode.Is, sessionArgs.Uri));
            }
        }

        private void addCookieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (NowEditMode == RuleEditMode.EditResponseRule)
            {
                if (MessageBox.Show(
                        "your are in Response Edit Mode.\r\ndo you want give up the editing and new a rule to continue this quick rule?",
                        "Continue or not", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    pb_ruleCancel_Click(this, e);
                else
                    return;
            }

            tabControl_Modific.SelectedTab = tabPage_requestModific;
            var f = new EditKeyVaule(requestAddHeads.ListDataView, "Cookie", ": ");
            f.ShowDialog();
        }

        private void addUserAgentToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (NowEditMode == RuleEditMode.EditResponseRule)
            {
                if (MessageBox.Show(
                        "your are in Response Edit Mode.\r\ndo you want give up the editing and new a rule to continue this quick rule?",
                        "Continue or not", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    pb_ruleCancel_Click(this, e);
                else
                    return;
            }

            tabControl_Modific.SelectedTab = tabPage_requestModific;
            requestRemoveHeads.ListDataView.Items.Add("User-Agent");
            var f = new EditKeyVaule(requestAddHeads.ListDataView, "User-Agent", ": ");
            f.ShowDialog();
        }

        private void ChangeSessionEncodingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var seekHeadArgs = new GetSessionSeekHeadEventArgs(new KeyValuePair<string, string>("Content-Type", null),
                new KeyValuePair<string, string>("Content-Type", null));
            if (OnGetSessionSeekHead != null) OnGetSessionSeekHead(this, seekHeadArgs);
            var changeEncodeInfo = new ChangeEncodeForm.ChangeEncodeInfo
            {
                ContentType_Request = seekHeadArgs.ResquestHead.Value,
                ContentType_Response = seekHeadArgs.ResponseHead.Value, NowEncode = null, EditMode = NowEditMode
            };
            var f = new ChangeEncodeForm(changeEncodeInfo);
            var dialogResult = f.ShowDialog();

            if (string.IsNullOrEmpty(changeEncodeInfo.NowEncode)) return;
            if (changeEncodeInfo.ContentType_Request == null)
            {
                var responseChange = new FiddlerResponseChange();
                if (seekHeadArgs.SeekUri != null)
                    responseChange.HttpFilter =
                        new FiddlerHttpFilter(new FiddlerUriMatch(FiddlerUriMatchMode.Is, seekHeadArgs.SeekUri));
                responseChange.HeadDelList = new List<string> { "Content-Type" };
                responseChange.HeadAddList = new List<string>
                    { string.Format("Content-Type: {0}", changeEncodeInfo.ContentType_Response) };
                responseChange.BodyModific =
                    new ParameterContentModific(string.Format("<recode>{0}", changeEncodeInfo.NowEncode), "");
                SetResponseModificInfo(responseChange);
            }
            else
            {
                var requestChange = new FiddlerRequestChange();
                if (seekHeadArgs.SeekUri != null)
                    requestChange.HttpFilter =
                        new FiddlerHttpFilter(new FiddlerUriMatch(FiddlerUriMatchMode.Is, seekHeadArgs.SeekUri));
                requestChange.HeadDelList = new List<string> { "Content-Type" };
                requestChange.HeadAddList = new List<string>
                    { string.Format("Content-Type: {0}", changeEncodeInfo.ContentType_Request) };
                requestChange.BodyModific =
                    new ParameterContentModific(string.Format("<recode>{0}", changeEncodeInfo.NowEncode), "");
                SetRequestModificInfo(requestChange);
            }
        }

        private void deleteCookieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (NowEditMode == RuleEditMode.EditRequsetRule)
            {
                if (MessageBox.Show(
                        "your are in Request Edit Mode.\r\ndo you want give up the editing and new a rule to continue this quick rule?",
                        "Continue or not", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    pb_ruleCancel_Click(this, e);
                else
                    return;
            }

            tabControl_Modific.SelectedTab = tabPage_responseModific;

            var tempDomain = string.Empty;
            var sessionArgs = GetNowHttpSession();
            if (sessionArgs.IsGetSuccess && sessionArgs.RequestHeads != null)
                try
                {
                    tempDomain = sessionArgs.RequestHeads.First(headerItem => headerItem.Key.Trim().ToLower() == "host")
                        .Value.Trim();
                }
                catch
                {
                    tempDomain = "www.yourhost.com";
                }

            var f = new EditCookieForm(responseAddHeads.ListDataView, null, null,
                string.Format("Max-Age=1;Domain={0};Path=/", tempDomain));
            f.ShowDialog();
            if (tb_urlFilter.Text == "")
                if (sessionArgs.IsGetSuccess && sessionArgs.Uri != null)
                    SetUriMatch(new FiddlerUriMatch(FiddlerUriMatchMode.Is, sessionArgs.Uri));
        }

        private void setClientCookieToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (NowEditMode == RuleEditMode.EditRequsetRule)
            {
                if (MessageBox.Show(
                        "your are in Request Edit Mode.\r\ndo you want give up the editing and new a rule to continue this quick rule?",
                        "Continue or not", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    pb_ruleCancel_Click(this, e);
                else
                    return;
            }

            tabControl_Modific.SelectedTab = tabPage_responseModific;
            var f = new EditCookieForm(responseAddHeads.ListDataView);
            f.ShowDialog();
        }

        private void copySessionCookiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (NowEditMode == RuleEditMode.EditRequsetRule)
            {
                if (MessageBox.Show(
                        "your are in Request Edit Mode.\r\ndo you want give up the editing and new a rule to continue this quick rule?",
                        "Continue or not", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    pb_ruleCancel_Click(this, e);
                else
                    return;
            }

            tabControl_Modific.SelectedTab = tabPage_responseModific;
            if (OnGetSessionRawData != null)
                OnGetSessionRawData(this, new GetSessionRawDataEventArgs(GetSessionAction.SetCookies));
        }

        private void removeSessionCookiesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (NowEditMode == RuleEditMode.EditRequsetRule)
            {
                if (MessageBox.Show(
                        "your are in Request Edit Mode.\r\ndo you want give up the editing and new a rule to continue this quick rule?",
                        "Continue or not", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
                    pb_ruleCancel_Click(this, e);
                else
                    return;
            }

            tabControl_Modific.SelectedTab = tabPage_responseModific;
            if (OnGetSessionRawData != null)
                OnGetSessionRawData(this, new GetSessionRawDataEventArgs(GetSessionAction.DeleteCookies));
        }

        private void showSelectedSessionStreamToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (OnGetSessionRawData != null)
                OnGetSessionRawData(this, new GetSessionRawDataEventArgs(GetSessionAction.ShowShowResponse));
        }

        private void httpTamperSettingToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var f = new SettingWindow(ModificSettingInfo);
            f.ShowDialog();
        }

        private void loadingRemoteRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var f = new GetRemoteRuleWindow(this);
            f.StartPosition = FormStartPosition.CenterParent;
            f.ShowDialog();
            return;

            //下面的都是测试代码，不会被执行到
            //WebService.RemoteRuleService.GetRemoteRuleAsync("6077f8fa617545cb9fbf12b1c874f7ee").ContinueWith((rule) => { LoadFiddlerModificHttpRuleCollection(rule.Result); });
            var ruleTask = Task.Run(() =>
            {
                //return WebService.RemoteRuleService.GetRemoteRuleAsync("6077f8fa617545cb9fbf12b1c874f7ee").GetAwaiter().GetResult();
                var x = GetRemoteRuleAsync("6077f8fa617545cb9fbf12b1c874f7ee");
                Thread.Sleep(100);
                return x.Result;
            });
            var ruleDetails = ruleTask.GetAwaiter().GetResult();
            if (ruleDetails != null)
            {
                InitializeConfigInfo(ruleDetails.ModificHttpRuleCollection, ModificSettingInfo,
                    ruleDetails.StaticDataCollection, null);
                LoadFiddlerModificHttpRuleCollection(fiddlerModificHttpRuleCollection);
            }

            return;

            //FiddlerModificHttpRuleCollection tempModificHttpRuleCollection = WebService.RemoteRuleService.GetRemoteRuleAsync("6077f8fa617545cb9fbf12b1c874f7ee").GetAwaiter().GetResult();
            var getRuleTask = GetRemoteRuleAsync("6077f8fa617545cb9fbf12b1c874f7ee");
            //getRuleTask.Start();
            //getRuleTask.Wait();
            var tempModificHttpRuleCollection = getRuleTask.Result;
            if (tempModificHttpRuleCollection != null)
                LoadFiddlerModificHttpRuleCollection(tempModificHttpRuleCollection.ModificHttpRuleCollection);
        }

        private void parameterDataManageToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (var tempFrom in FiddlerApplication.UI.OwnedForms)
                if (tempFrom is StaticDataManageWindow)
                {
                    tempFrom.Location = new Point((Screen.PrimaryScreen.WorkingArea.Width - tempFrom.Width) / 2,
                        (Screen.PrimaryScreen.WorkingArea.Height - tempFrom.Height) / 2);
                    return;
                }

            StaticDataManageWindow staticDataManageWindow;
            staticDataManageWindow = new StaticDataManageWindow(StaticDataCollection);
            staticDataManageWindow.Owner = FiddlerApplication.UI;
            staticDataManageWindow.StartPosition = FormStartPosition.CenterScreen;
            //f.ShowDialog();
            staticDataManageWindow.Show();
        }

        public void independentWindowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (independentWindowToolStripMenuItem.Text == "independent window")
            {
                OnShowInIndependentWindow(this, true);
                independentWindowToolStripMenuItem.Text = "addin window";
            }
            else
            {
                OnShowInIndependentWindow(this, false);
                independentWindowToolStripMenuItem.Text = "independent window";
            }
        }

        private void FeedbackToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var f = new UserFeedbackWindow(this);
            f.StartPosition = FormStartPosition.CenterParent;
            f.ShowDialog();
        }

        private void CodeInGithubToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/lulianqi/FreeHttp");
        }

        private void DocumentationToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Process.Start("http://doc.lulianqi.com/FreeHttp/Documentation/recent");
        }

        #endregion

        #region Rule control

        private void Lv_ruleList_OnItemDragSort(object sender, DragEventArgs e)
        {
            ModificRuleGroup.RecoverTemporaryGroup((ListView)sender);
            RefreshFiddlerRuleList((ListView)sender);
        }

        private void Lv_ruleList_OnItemDragSortStart(object sender, ItemDragEventArgs e)
        {
            ModificRuleGroup.RemoveGroupTemporary((ListView)sender);
        }

        private void pb_requestRuleSwitch_Click(object sender, EventArgs e)
        {
            if (IsRequestRuleEnable)
            {
                pb_requestRuleSwitch.Image = MyResource.switch_off;
                IsRequestRuleEnable = false;
                PutWarn("Request Temper Rule Forbidden");
            }
            else
            {
                pb_requestRuleSwitch.Image = MyResource.switch_on;
                IsRequestRuleEnable = true;
                PutWarn("Request Temper Rule Enabled");
            }
        }

        private void pb_responseRuleSwitch_Click(object sender, EventArgs e)
        {
            if (IsResponseRuleEnable)
            {
                pb_responseRuleSwitch.Image = MyResource.switch_off;
                IsResponseRuleEnable = false;
                PutWarn("Response Temper Rule Forbidden");
            }
            else
            {
                pb_responseRuleSwitch.Image = MyResource.switch_on;
                IsResponseRuleEnable = true;
                PutWarn("Response Temper Rule Enabled");
            }
        }


        private void lv_ruleList_ItemMouseHover(object sender, ListViewItemMouseHoverEventArgs e)
        {
            var nowListViewItem = e.Item;
            ShowRuleInfo_pb.Tag = nowListViewItem;
            nowListViewItem.ListView.Controls.Add(ShowRuleInfo_pb);
            ShowRuleInfo_pb.Visible = true;
            var r = nowListViewItem.Bounds;
            ShowRuleInfo_pb.Size = new Size((int)((r.Height - 4) * 1.5), r.Height - 4);
            ShowRuleInfo_pb.Location = new Point(nowListViewItem.Position.X + r.Width - ShowRuleInfo_pb.Width - 30,
                nowListViewItem.Position.Y + 2);
        }

        private void lv_ruleList_MouseLeave(object sender, EventArgs e)
        {
            var tempListView = sender as ListView;
            if (tempListView == null) return;
            var tempPosition = MousePosition;
            tempPosition = tempListView.PointToClient(tempPosition);
            if (tempListView.GetItemAt(tempPosition.X, tempPosition.Y) == null) ShowRuleInfo_pb.Visible = false;
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void ShowRuleInfo_pb_Click(object sender, EventArgs e)
        {
            var nowListViewItem = ShowRuleInfo_pb.Tag as ListViewItem;
            var isExistedInfoWindow = false;
            if (nowListViewItem == null) return;
            for (var i = nowRuleInfoWindowList.Count - 1; i >= 0; i--)
            {
                if (nowRuleInfoWindowList[i].IsDisposed)
                {
                    nowRuleInfoWindowList.RemoveAt(i);
                    continue;
                }

                if (!isExistedInfoWindow && nowRuleInfoWindowList[i].InnerListViewItem == nowListViewItem)
                {
                    nowRuleInfoWindowList[i].Activate();
                    isExistedInfoWindow = true;
                }
            }

            if (isExistedInfoWindow) return;

            Point myPosition;
            try
            {
                myPosition = new Point(nowListViewItem.Bounds.X, nowListViewItem.Bounds.Y);
            }
            catch
            {
                MessageBox.Show("your rule is  already collapsed");
                return;
            }

            myPosition = nowListViewItem.ListView.PointToScreen(myPosition);
            myPosition = ParentForm.PointToClient(myPosition);
            myPosition.Offset(40, 10);
            var myListViewCBallon = new RuleInfoWindow(nowListViewItem);
            myListViewCBallon.Owner = ParentForm;
            myListViewCBallon.HasShadow = true;
            myListViewCBallon.setBalloonPosition(ParentForm, myPosition, new Size(0, 0));
            myListViewCBallon.Show();
            myListViewCBallon.UpdateBalloonPosition(myPosition);

            nowRuleInfoWindowList.Add(myListViewCBallon);
            if (nowRuleInfoWindowList.Count > 4)
            {
                nowRuleInfoWindowList[0].Close();
                nowRuleInfoWindowList.RemoveAt(0);
            }
        }

        private void lv_RuleList_DoubleClick(object sender, EventArgs e)
        {
            //Point p = PointToClient(new Point(Cursor.Position.X, Cursor.Position.Y)); 
            //ListViewItem lvi = ((ListView)sender).GetItemAt(p.X, p.Y);
            if (((ListView)sender).SelectedItems == null || ((ListView)sender).SelectedItems.Count == 0) return;
            var nowListViewItem = ((ListView)sender).SelectedItems[0];
            if (nowListViewItem != null)
            {
                //TamperProtocalType tempProtocolMode;
                //if (string.IsNullOrEmpty(((IFiddlerHttpTamper)nowListViewItem.Tag).TamperProtocol))
                //{
                //    tempProtocolMode = TamperProtocalType.Http;
                //}
                //else if (!Enum.TryParse<TamperProtocalType>(((IFiddlerHttpTamper)nowListViewItem.Tag).TamperProtocol, out tempProtocolMode))
                //{
                //    if(DialogResult.OK== MessageBox.Show("find unkonw protocal in your rule \r\ndo you want change the unkonw protocol to Http", "unkonw protocol", MessageBoxButtons.OKCancel, MessageBoxIcon.Question))
                //    {
                //        tempProtocolMode = TamperProtocalType.Http;
                //    }
                //    else
                //    {
                //        return;
                //    }
                //}
                if (sender == lv_requestRuleList)
                {
                    ChangeNowRuleMode(RuleEditMode.EditRequsetRule,
                        ((IFiddlerHttpTamper)nowListViewItem.Tag).TamperProtocol,
                        string.Format("Edit Requst {0}", nowListViewItem.SubItems[0].Text), nowListViewItem);
                    SetRequestModificInfo((FiddlerRequestChange)nowListViewItem.Tag);
                }

                else if (sender == lv_responseRuleList)
                {
                    ChangeNowRuleMode(RuleEditMode.EditResponseRule,
                        ((IFiddlerHttpTamper)nowListViewItem.Tag).TamperProtocol,
                        string.Format("Edit Response {0}", nowListViewItem.SubItems[0].Text), nowListViewItem);
                    SetResponseModificInfo((FiddlerResponseChange)nowListViewItem.Tag);
                }
                else
                {
                    MessageBox.Show("not adaptive to lv_RuleList_DoubleClick");
                }

                nowListViewItem.ListView.SelectedItems.Clear();
            }
        }

        private void lv_RuleList_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            if (e.Item != null) SyncEnableSateToIFiddlerHttpTamper(e.Item, (IFiddlerHttpTamper)e.Item.Tag);
            //((IFiddlerHttpTamper)e.Item.Tag).IsEnable = e.Item.Checked;
        }

        private void pb_addTemperRule_Click(object sender, EventArgs e)
        {
            if (sender == pb_addRequestRule)
            {
                ChangeNowRuleMode(RuleEditMode.NewRuleMode, NowProtocalMode, null, null);
                tabControl_Modific.SelectedTab = tabPage_requestModific;
                MarkTipControl(tabPage_requestModific);
                //MarkControl(pb_getSession, Color.MediumSpringGreen, 1);
            }
            else if (sender == pb_addResponseRule)
            {
                ChangeNowRuleMode(RuleEditMode.NewRuleMode, NowProtocalMode, null, null);
                tabControl_Modific.SelectedTab = tabPage_responseModific;
                MarkTipControl(tabPage_responseModific);
                //MarkControl(pb_getSession, Color.MediumSpringGreen, 1);
            }
        }

        private void pb_removeTemperRule_Click(object sender, EventArgs e)
        {
            ListView nowRuleListView = null;
            if (sender == pb_removeRequestRule)
                nowRuleListView = lv_requestRuleList;
            else if (sender == pb_removeResponseRule)
                nowRuleListView = lv_responseRuleList;
            else
                return;
            if (nowRuleListView.SelectedItems != null && nowRuleListView.SelectedItems.Count > 0)
            {
                foreach (ListViewItem tempItem in nowRuleListView.SelectedItems)
                {
                    if (tempItem == EditListViewItem)
                    {
                        ChangeNowRuleMode(RuleEditMode.NewRuleMode, NowProtocalMode, null, null);
                        PutWarn("you editing rule is removed ,now change to [NewRuleMode]");
                    }

                    DelRuleFromListView(nowRuleListView, tempItem);
                    //nowRuleListView.Items.Remove(tempItem);
                }

                //更新组信息，删除空组
                RemoveEmptyViewGroup(nowRuleListView);
                //删除不用调整rule id 没有重复的风险
                //AdjustRuleListViewIndex(nowRuleListView);
            }
            else
            {
                MessageBox.Show("please select the rules that your want remove");
            }
        }

        #region RuleToolStripMenuItem

        private ListView GetRuleToolStripMenuItemSourceControl(object sender)
        {
            object sourceControl = ((ContextMenuStrip)((ToolStripMenuItem)sender).Owner).SourceControl;
            if (sourceControl == lv_requestRuleList)
                return lv_requestRuleList;
            if (sourceControl == lv_responseRuleList)
                return lv_responseRuleList;
            throw new Exception("nonsupported SelectedRuleToolStripMenuItem_Click");
        }

        private void removeSelectedRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tempRuleLv = GetRuleToolStripMenuItemSourceControl(sender);

            if (tempRuleLv == lv_requestRuleList)
                pb_removeTemperRule_Click(pb_removeRequestRule, null);
            else if (tempRuleLv == lv_responseRuleList)
                pb_removeTemperRule_Click(pb_removeResponseRule, null);
            else
                throw new Exception("nonsupported SelectedRuleToolStripMenuItem_Click");
        }

        private void removeAllRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!(MessageBox.Show("if you want remove all your tamper rule in this list", "reconfirm ",
                    MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)) return;
            var tempRuleLv = GetRuleToolStripMenuItemSourceControl(sender);
            if (EditListViewItem != null && EditListViewItem.ListView == tempRuleLv)
            {
                ChangeNowRuleMode(RuleEditMode.NewRuleMode, NowProtocalMode, null, null);
                PutWarn("you editing rule is removed ,now change to [NewRuleMode]");
            }

            //tempRuleLv.Items.Clear();
            DelRuleFromListView(tempRuleLv, null);
        }

        private void copySelectedRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var nowRuleListView = GetRuleToolStripMenuItemSourceControl(sender);
            if (nowRuleListView.SelectedItems != null && nowRuleListView.SelectedItems.Count > 0)
                foreach (ListViewItem tempItem in nowRuleListView.SelectedItems)
                    try
                    {
                        var tempHttpTamper = ((IFiddlerHttpTamper)tempItem.Tag).Clone() as IFiddlerHttpTamper;
                        tempHttpTamper.RuleUid = null; //深度克隆会有一样的UID，这里需要重置副本UID
                        tempHttpTamper.HttpFilter.Name = string.Format("<copy from> {0}",
                            tempHttpTamper.HttpFilter?.GetShowTitle() ?? "");
                        AddRuleToListView(nowRuleListView, tempHttpTamper, true);
                    }
                    catch (Exception ex)
                    {
                        _ = RemoteLogService.ReportLogAsync(ex.ToString(), RemoteLogService.RemoteLogOperation.AddRule,
                            RemoteLogService.RemoteLogType.Error);
                        MessageBox.Show(string.Format("copy rule file\r\n{0}", tempItem?.SubItems[1].Text, "Stop"));
                    }
            else
                MessageBox.Show("please select the rules that your want copy", "Stop");
        }

        private void enableThisRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tempRuleLv = GetRuleToolStripMenuItemSourceControl(sender);
            if (tempRuleLv.SelectedItems != null)
                foreach (ListViewItem tempItem in tempRuleLv.SelectedItems)
                    tempItem.Checked = true;
        }

        private void enableAllRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tempRuleLv = GetRuleToolStripMenuItemSourceControl(sender);

            foreach (ListViewItem tempItem in tempRuleLv.Items) tempItem.Checked = true;
        }

        private void unableAllRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tempRuleLv = GetRuleToolStripMenuItemSourceControl(sender);

            foreach (ListViewItem tempItem in tempRuleLv.Items) tempItem.Checked = false;
        }

        private void editThisRuleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var tempRuleLv = GetRuleToolStripMenuItemSourceControl(sender);
            lv_RuleList_DoubleClick(tempRuleLv, null);
        }

        #region RuleToolStripMenuItem_Group

        //临时标记当前在操作的ListView
        private ListView _nowTempGroupRuleLv;

        //临时标记当前在操作的ListViewGroup
        private ListViewGroup _nowTempGroupRuleLvg;

        /// <summary>
        ///     更新组列表（移除item为空的group）
        /// </summary>
        /// <param name="yourListView"></param>
        private void RemoveEmptyViewGroup(ListView yourListView)
        {
            if (yourListView.Groups.Count > 0)
                for (var i = yourListView.Groups.Count - 1; i >= 0; i--)
                    if (yourListView.Groups[i].Items.Count == 0)
                    {
                        PutInfo($"group [{yourListView.Groups[i].Header}] will be remove ,because that no item ");
                        yourListView.Groups.RemoveAt(i);
                    }
        }

        /// <summary>
        ///     跟换分组
        /// </summary>
        /// <param name="selectedListViewItemCollection"></param>
        /// <param name="listViewGroup"></param>
        /// <returns></returns>
        private bool MoveRuleItemGroup(ListView.SelectedListViewItemCollection selectedListViewItemCollection,
            ListViewGroup listViewGroup)
        {
            foreach (ListViewItem listViewItem in selectedListViewItemCollection) listViewItem.Group = listViewGroup;
            if (selectedListViewItemCollection.Count > 0)
            {
                RemoveEmptyViewGroup(selectedListViewItemCollection[0].ListView);
                ModificRuleGroup.ReArrangeGroup(selectedListViewItemCollection[0].ListView);
                RefreshFiddlerRuleList(selectedListViewItemCollection[0].ListView);
            }

            return true;
        }

        /// <summary>
        ///     选中的项是否都在默认分组（没有分组信息）
        /// </summary>
        /// <param name="selectedListViewItemCollection"></param>
        /// <returns></returns>
        private bool IsAllDefaultViewGroup(ListView.SelectedListViewItemCollection selectedListViewItemCollection)
        {
            foreach (ListViewItem listViewItem in selectedListViewItemCollection)
                if (listViewItem.Group != null)
                    return false;
            return true;
        }

        /// <summary>
        ///     确定项目是否都来自同一个分组
        /// </summary>
        /// <param name="selectedListViewItemCollection"></param>
        /// <returns>如果不是同一个分组返回null，如果是返回分组</returns>
        private ListViewGroup FindOnlyOneViewGroup(
            ListView.SelectedListViewItemCollection selectedListViewItemCollection)
        {
            ListViewGroup oneListViewGroup = null;
            foreach (ListViewItem listViewItem in selectedListViewItemCollection)
            {
                if (listViewItem.Group == null) return null;
                if (oneListViewGroup == null)
                {
                    oneListViewGroup = listViewItem.Group;
                }
                else
                {
                    if (oneListViewGroup != listViewItem.Group) return null;
                }
            }

            return oneListViewGroup;
        }

        /// <summary>
        ///     展开分组操作
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void groupToolStripMenuItem_DropDownOpened(object sender, EventArgs e)
        {
            var tempRuleLv = GetRuleToolStripMenuItemSourceControl(sender);
            _nowTempGroupRuleLv = tempRuleLv;
            if (tempRuleLv.Groups.Count > 0)
            {
                moveToGroupToolStripMenuItem.Enabled = true;
                renameThisGroupToolStripMenuItem.Enabled = true;
                deleteThisGroupToolStripMenuItem.Enabled = true;
                enableThisGroupToolStripMenuItem.Enabled = true;
                unableThisGroupToolStripMenuItem.Enabled = true;

                moveToGroupToolStripMenuItem.DropDownItems.Clear();
                //添加默认分组
                ToolStripItem tempAddToolStripItem = new ToolStripMenuItem("Default");
                tempAddToolStripItem.Click += (esd, ee) => { MoveRuleItemGroup(tempRuleLv.SelectedItems, null); };
                moveToGroupToolStripMenuItem.DropDownItems.Add(tempAddToolStripItem);
                //检查无效分组
                ListViewGroup onlyOneGroup = null;
                var isSelectedAllDefault = false;
                if (IsAllDefaultViewGroup(tempRuleLv.SelectedItems))
                {
                    isSelectedAllDefault = true;
                    tempAddToolStripItem.Enabled = false;
                }
                else
                {
                    onlyOneGroup = FindOnlyOneViewGroup(tempRuleLv.SelectedItems);
                }

                //添加分组
                foreach (ListViewGroup group in tempRuleLv.Groups)
                {
                    tempAddToolStripItem = new ToolStripMenuItem(group.Header);
                    tempAddToolStripItem.Click += (esd, ee) => { MoveRuleItemGroup(tempRuleLv.SelectedItems, group); };
                    moveToGroupToolStripMenuItem.DropDownItems.Add(tempAddToolStripItem);
                    if (onlyOneGroup == group) tempAddToolStripItem.Enabled = false;
                }

                //确认MenuItem可用项
                if (isSelectedAllDefault || onlyOneGroup == null)
                {
                    renameThisGroupToolStripMenuItem.Enabled = false;
                    deleteThisGroupToolStripMenuItem.Enabled = false;
                    enableThisGroupToolStripMenuItem.Enabled = false;
                    unableThisGroupToolStripMenuItem.Enabled = false;
                }
                else
                {
                    _nowTempGroupRuleLvg = onlyOneGroup;
                }
            }
            else
            {
                moveToGroupToolStripMenuItem.Enabled = false;
                renameThisGroupToolStripMenuItem.Enabled = false;
                deleteThisGroupToolStripMenuItem.Enabled = false;
                enableThisGroupToolStripMenuItem.Enabled = false;
                unableThisGroupToolStripMenuItem.Enabled = false;

                _nowTempGroupRuleLvg = null;
            }
        }


        private void moveToGroupToolStripMenuItem_DropDownOpening(object sender, EventArgs e)
        {
            var tempRuleLv = _nowTempGroupRuleLv;
        }

        /// <summary>
        ///     添加分组
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void addToNewGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newGroupName = "";
            var f = new SetVaule("New rule group", "you will new a rule group ,and now set the group name.", "",
                checkValue =>
                {
                    if (string.IsNullOrEmpty(checkValue) || checkValue.Length > 30)
                        return "error length of group name";
                    if (checkValue == "Default") return "[Default] is a reserve name";
                    if (_nowTempGroupRuleLv?.Groups != null && _nowTempGroupRuleLv.Groups.Count > 0)
                        foreach (ListViewGroup lvg in _nowTempGroupRuleLv.Groups)
                            if (lvg.Header == checkValue)
                                return $"[{checkValue}] have been used";
                    return null;
                });
            f.OnSetValue += (obj, tag) => { newGroupName = tag.SetValue; };
            if (f.ShowDialog() == DialogResult.OK)
            {
                var tempRuleLv = _nowTempGroupRuleLv;
                if (tempRuleLv.SelectedItems != null && tempRuleLv.SelectedItems.Count > 0)
                {
                    var tempListViewGroup = new ListViewGroup(newGroupName);
                    ((MyListView)tempRuleLv).GroupSelectedSataus.GetSnapshoot();
                    tempRuleLv.Groups.Add(tempListViewGroup);
                    ((MyListView)tempRuleLv).GroupSelectedSataus.ReCoverSnapshoot();
                    ((MyListView)tempRuleLv).SetGroupState(ListViewGroupState.Collapsible, tempListViewGroup);
                    foreach (ListViewItem tempItem in tempRuleLv.SelectedItems) tempItem.Group = tempListViewGroup;
                    //((MyListView)tempRuleLv).SetGroupFooter(tempListViewGroup, "Group contains " + tempListViewGroup.Items.Count + " items...");
                    RemoveEmptyViewGroup(tempRuleLv);
                    ModificRuleGroup.ReArrangeGroup(tempRuleLv);
                    RefreshFiddlerRuleList(tempRuleLv);
                    PutInfo($"group [{newGroupName}] add succeed");
                }
                else
                {
                    MessageBox.Show("please select the rules that your want ", "Stop");
                }
            }
            // do nothing
        }

        /// <summary>
        ///     重命名分组
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void renameThisGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var newGroupName = _nowTempGroupRuleLvg.Header;
            var f = new SetVaule("Rename rule group", "you will rename rule group ,and input the new group name.",
                newGroupName, checkValue =>
                {
                    if (string.IsNullOrEmpty(checkValue) || checkValue.Length > 30)
                        return "error length of group name";
                    if (checkValue == "Default") return "[Default] is a reserve name";
                    if (_nowTempGroupRuleLv?.Groups != null && _nowTempGroupRuleLv.Groups.Count > 0)
                        foreach (ListViewGroup lvg in _nowTempGroupRuleLv.Groups)
                            if (lvg.Header == checkValue)
                                return $"[{checkValue}] have been used";
                    return null;
                });
            f.OnSetValue += (obj, tag) => { newGroupName = tag.SetValue; };
            if (f.ShowDialog() == DialogResult.OK)
            {
                _nowTempGroupRuleLvg.Header = newGroupName;
                ModificRuleGroup.ReflushGroupDc(_nowTempGroupRuleLvg.ListView);
                ((MyListView)_nowTempGroupRuleLvg.ListView).SetGroupState(ListViewGroupState.Collapsible,
                    _nowTempGroupRuleLvg);
            }
        }

        /// <summary>
        ///     删除分组
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void deleteThisGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            var dialogResult =
                MessageBox.Show(
                    $"group [{_nowTempGroupRuleLvg.Header}] will been delete ,and his rule item will move to [Default]",
                    "Delete group", MessageBoxButtons.OKCancel, MessageBoxIcon.Question);
            if (dialogResult == DialogResult.OK)
            {
                //foreach(ListViewItem listViewItem in _nowTempGroupRuleLvg.Items)
                //{
                //    listViewItem.Group = null;
                //}
                for (var i = _nowTempGroupRuleLvg.Items.Count - 1; i >= 0; i--)
                    _nowTempGroupRuleLvg.Items[i].Group = null;
                _nowTempGroupRuleLvg.ListView.Groups.Remove(_nowTempGroupRuleLvg);
                ModificRuleGroup.ReArrangeGroup(_nowTempGroupRuleLv);
                RefreshFiddlerRuleList(_nowTempGroupRuleLv);
                PutInfo($"group [{_nowTempGroupRuleLvg.Header}] remove succeed");
            }
        }

        /// <summary>
        ///     启用分组包含的项（勾选）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void enableThisGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem listViewItem in _nowTempGroupRuleLvg.Items) listViewItem.Checked = true;
        }

        /// <summary>
        ///     禁用分组包含的项（取消勾选）
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void unableThisGroupToolStripMenuItem_Click(object sender, EventArgs e)
        {
            foreach (ListViewItem listViewItem in _nowTempGroupRuleLvg.Items) listViewItem.Checked = false;
        }

        #endregion

        #endregion

        #endregion

        #region Url Filter

        private void pb_getSession_Click(object sender, EventArgs e)
        {
            //_= new WebService.RuleReportService().UploadRules<FiddlerRequestChange,FiddlerResponseChange>(fiddlerModificHttpRuleCollection.RequestRuleList, fiddlerModificHttpRuleCollection.ResponseRuleList);
            if (OnUpdataFromSession != null) OnUpdataFromSession(this, null);
        }

        private void cb_macthMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_macthMode.Text == "AllPass")
            {
                tb_urlFilter.Text = "";
                tb_urlFilter.Enabled = false;
            }
            else
            {
                tb_urlFilter.Enabled = true;
            }
        }

        private void tb_urlFilter_DragEnter(object sender, DragEventArgs e)
        {
            var draggedSessions = (Session[])e.Data.GetData(typeof(Session[]));
            e.Effect = draggedSessions == null || draggedSessions.Length < 1 ? DragDropEffects.None : e.AllowedEffect;
        }

        private void tb_urlFilter_DragDrop(object sender, DragEventArgs e)
        {
            var draggedSessions = (Session[])e.Data.GetData(typeof(Session[]));
            if (draggedSessions != null && draggedSessions.Length > 0) tb_urlFilter.Text = draggedSessions[0].fullUrl;
        }

        private void rtb_MesInfo_DragEnter(object sender, DragEventArgs e)
        {
            var draggedSessions = (Session[])e.Data.GetData(typeof(Session[]));
            e.Effect = draggedSessions == null || draggedSessions.Length < 1 ? DragDropEffects.None : e.AllowedEffect;
        }

        private void rtb_MesInfo_DragDrop(object sender, DragEventArgs e)
        {
            var draggedSessions = (Session[])e.Data.GetData(typeof(Session[]));
            if (draggedSessions != null && draggedSessions.Length > 0)
                for (var i = 0; i < draggedSessions.Length; i++)
                {
                    var tempStr = FiddlerSessionTamper.GetSessionRawData(draggedSessions[i], true);
                    PutInfo(tempStr == null ? "error session" : string.Format("Get Raw Data\r\n{0}", tempStr));
                }
        }

        #endregion

        #region RequestModific

        #endregion

        #region ResponseModific

        #endregion

        #region ResponseReplace

        #endregion
    }
}