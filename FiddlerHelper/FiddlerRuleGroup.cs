﻿using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Windows.Forms;
using FreeHttp.FreeHttpControl;
using FreeHttp.WebService;

namespace FreeHttp.FiddlerHelper
{
    [Serializable]
    [DataContract]
    public class FiddlerRuleGroup
    {
        private bool _isRequestRuleGroupInTemporaryStatus;
        private bool _isResponseRuleGroupInTemporaryStatus;
        private ListView RequestRuleListView;
        private ListView ResponseRuleListView;

        public FiddlerRuleGroup(ListView rqLv, ListView rpLv)
        {
            RequestRuleListView = rqLv;
            ResponseRuleListView = rpLv;
            RequestGroupDictionary = new Dictionary<string, List<string>>();
            ResponseGroupDictionary = new Dictionary<string, List<string>>();
        }

        [DataMember] public Dictionary<string, List<string>> RequestGroupDictionary { get; set; }

        [DataMember] public Dictionary<string, List<string>> ResponseGroupDictionary { get; set; }

        public bool IsEmpty => (RequestGroupDictionary == null || RequestGroupDictionary.Count == 0) &&
                               (ResponseGroupDictionary == null || ResponseGroupDictionary.Count == 0);

        public void SetRuleGroupListView(ListView rqLv, ListView rpLv)
        {
            RequestRuleListView = rqLv;
            ResponseRuleListView = rpLv;
        }

        /// <summary>
        ///     重排listView顺序（因为在存在分组的情况下拖拽排序会不生效，所以需要强制刷新，任何地方都不要自行调用该方法修改ListView）
        /// </summary>
        /// <param name="listView"></param>
        private void ReflushListViewItem(ListView listView)
        {
            var listViewItems = new List<ListViewItem>();
            foreach (ListViewItem itm in listView.Items) listViewItems.Add(itm);
            listView.Items.Clear();
            foreach (var itm in listViewItems) listView.Items.Add(itm);
        }

        /// <summary>
        ///     更新GroupDictionary
        /// </summary>
        /// <param name="listView">选择更新指定ListView（默认null表示全部更新）</param>
        public void ReflushGroupDc(ListView listView = null)
        {
            if (RequestRuleListView == listView || listView == null)
            {
                RequestGroupDictionary.Clear();
                if (RequestRuleListView.Groups != null && RequestRuleListView.Groups.Count > 0)
                    foreach (ListViewGroup listViewGroup in RequestRuleListView.Groups)
                    {
                        var ruleUidList = new List<string>();
                        foreach (ListViewItem listViewItem in listViewGroup.Items)
                            ruleUidList.Add(((IFiddlerHttpTamper)listViewItem.Tag).RuleUid);

                        if (RequestGroupDictionary.ContainsKey(listViewGroup.Header))
                            _ = RemoteLogService.ReportLogAsync(
                                $"find same key :{listViewGroup.Header} in[ReflushGroupDc]",
                                RemoteLogService.RemoteLogOperation.EditRule, RemoteLogService.RemoteLogType.Error);
                        else
                            RequestGroupDictionary.Add(listViewGroup.Header, ruleUidList);
                    }
            }

            if (ResponseRuleListView == listView || listView == null)
            {
                ResponseGroupDictionary.Clear();
                if (ResponseRuleListView.Groups != null && ResponseRuleListView.Groups.Count > 0)
                    foreach (ListViewGroup listViewGroup in ResponseRuleListView.Groups)
                    {
                        var ruleUidList = new List<string>();
                        foreach (ListViewItem listViewItem in listViewGroup.Items)
                            ruleUidList.Add(((IFiddlerHttpTamper)listViewItem.Tag).RuleUid);

                        if (ResponseGroupDictionary.ContainsKey(listViewGroup.Header))
                            _ = RemoteLogService.ReportLogAsync(
                                $"find same key :{listViewGroup.Header} in[ReflushGroupDc]",
                                RemoteLogService.RemoteLogOperation.EditRule, RemoteLogService.RemoteLogType.Error);
                        else
                            ResponseGroupDictionary.Add(listViewGroup.Header, ruleUidList);
                    }
            }
        }

        /// <summary>
        ///     对item进行重新排列，在有group且顺序发送变化时需要重排
        /// </summary>
        /// <param name="listView"></param>
        public void ReArrangeGroup(ListView listView = null)
        {
            if (RequestRuleListView == listView || listView == null)
            {
                MyControlHelper.SetControlFreeze(RequestRuleListView);
                RemoveGroupTemporary(RequestRuleListView);
                RecoverTemporaryGroup(RequestRuleListView);
                MyControlHelper.SetControlUnfreeze(RequestRuleListView);
            }

            if (ResponseRuleListView == listView || listView == null)
            {
                MyControlHelper.SetControlFreeze(ResponseRuleListView);
                RemoveGroupTemporary(ResponseRuleListView);
                RecoverTemporaryGroup(ResponseRuleListView);
                MyControlHelper.SetControlUnfreeze(ResponseRuleListView);
            }
        }


        /// <summary>
        ///     临时移除分组，需要与RecoverTemporaryGroup配合使用
        /// </summary>
        /// <param name="listView"></param>
        public void RemoveGroupTemporary(ListView listView)
        {
            if (RequestRuleListView == listView)
            {
                if (_isRequestRuleGroupInTemporaryStatus) return;
                if (RequestRuleListView.Groups != null && RequestRuleListView.Groups.Count > 0)
                {
                    ReflushGroupDc(RequestRuleListView);
                    RequestRuleListView.Groups.Clear();
                    foreach (ListViewItem listViewItem in listView.Items) listViewItem.Group = null;
                    _isRequestRuleGroupInTemporaryStatus = true;
                }
            }
            else if (ResponseRuleListView == listView)
            {
                if (_isResponseRuleGroupInTemporaryStatus) return;
                if (ResponseRuleListView.Groups != null && ResponseRuleListView.Groups.Count > 0)
                {
                    ReflushGroupDc(ResponseRuleListView);
                    ResponseRuleListView.Groups.Clear();
                    foreach (ListViewItem listViewItem in listView.Items) listViewItem.Group = null;
                    _isResponseRuleGroupInTemporaryStatus = true;
                }
            }
            else
            {
                _ = RemoteLogService.ReportLogAsync("unknow listView in [RemoveGroupTemporary]",
                    RemoteLogService.RemoteLogOperation.EditRule, RemoteLogService.RemoteLogType.Error);
            }
        }

        /// <summary>
        ///     恢复分组
        /// </summary>
        /// <param name="listView"></param>
        /// <param name="isCheckStatuus">是否检查_isRequestRuleGroupInTemporaryStatus/_isResponseRuleGroupInTemporaryStatus状态，如果不检查即直接恢复</param>
        public void RecoverTemporaryGroup(ListView listView, bool isCheckStatuus = true)
        {
            if (RequestRuleListView == listView)
            {
                if (!_isRequestRuleGroupInTemporaryStatus && isCheckStatuus) return;
                if (!isCheckStatuus) listView.Groups.Clear(); //如果不检查状态需要清除listView自己的Groups，以免照成重复，或空的Groups。
                if (RequestGroupDictionary != null && RequestGroupDictionary.Count > 0)
                {
                    ReflushListViewItem(listView);
                    var tempIdGroupDc = new Dictionary<string, ListViewGroup>();
                    foreach (var kv in RequestGroupDictionary)
                    {
                        var tempListViewGroup = new ListViewGroup(kv.Key);
                        listView.Groups.Add(tempListViewGroup);
                        foreach (var tempUid in kv.Value)
                            try
                            {
                                tempIdGroupDc.Add(tempUid, tempListViewGroup);
                            }
                            catch (Exception ex)
                            {
                                _ = RemoteLogService.ReportLogAsync($"error in [RecoverTemporaryGroup] :{ex}",
                                    RemoteLogService.RemoteLogOperation.EditRule, RemoteLogService.RemoteLogType.Error);
                            }
                    }

                    if (tempIdGroupDc.Count > 0)
                        foreach (ListViewItem listViewItem in listView.Items)
                            if (tempIdGroupDc.ContainsKey(((IFiddlerHttpTamper)listViewItem.Tag).RuleUid))
                                listViewItem.Group = tempIdGroupDc[((IFiddlerHttpTamper)listViewItem.Tag).RuleUid];
                    ((MyListView)listView).SetGroupState(ListViewGroupState.Collapsible);
                    _isRequestRuleGroupInTemporaryStatus = false;
                }
            }
            else if (ResponseRuleListView == listView)
            {
                if (!_isResponseRuleGroupInTemporaryStatus && isCheckStatuus) return;
                if (!isCheckStatuus) listView.Groups.Clear();
                if (ResponseGroupDictionary != null && ResponseGroupDictionary.Count > 0)
                {
                    ReflushListViewItem(listView);
                    var tempIdGroupDc = new Dictionary<string, ListViewGroup>();
                    foreach (var kv in ResponseGroupDictionary)
                    {
                        var tempListViewGroup = new ListViewGroup(kv.Key);
                        listView.Groups.Add(tempListViewGroup);
                        foreach (var tempUid in kv.Value)
                            try
                            {
                                tempIdGroupDc.Add(tempUid, tempListViewGroup);
                            }
                            catch (Exception ex)
                            {
                                _ = RemoteLogService.ReportLogAsync($"error in [RecoverTemporaryGroup] :{ex}",
                                    RemoteLogService.RemoteLogOperation.EditRule, RemoteLogService.RemoteLogType.Error);
                            }
                    }

                    if (tempIdGroupDc.Count > 0)
                        foreach (ListViewItem listViewItem in listView.Items)
                            if (tempIdGroupDc.ContainsKey(((IFiddlerHttpTamper)listViewItem.Tag).RuleUid))
                                listViewItem.Group = tempIdGroupDc[((IFiddlerHttpTamper)listViewItem.Tag).RuleUid];
                    ((MyListView)listView).SetGroupState(ListViewGroupState.Collapsible);
                    _isResponseRuleGroupInTemporaryStatus = false;
                }
            }
            else
            {
                _ = RemoteLogService.ReportLogAsync("unknow listView in [RecoverTemporaryGroup]",
                    RemoteLogService.RemoteLogOperation.EditRule, RemoteLogService.RemoteLogType.Error);
            }
        }

        /// <summary>
        ///     恢复分组信息（首次加载时可以用于还原保存的上一次分组）
        /// </summary>
        public void RecoverGroup()
        {
            RecoverTemporaryGroup(RequestRuleListView, false);
            RecoverTemporaryGroup(ResponseRuleListView, false);
        }
    }
}