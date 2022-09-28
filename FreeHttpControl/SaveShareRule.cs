﻿using System;
using System.Drawing;
using System.Windows.Forms;
using FreeHttp.FreeHttpControl.ControlHelper;
using FreeHttp.MyHelper;
using FreeHttp.WebService;
using FreeHttp.WebService.DataModel;

namespace FreeHttp.FreeHttpControl
{
    public partial class SaveShareRule : Form
    {
        private readonly LoadWindowService loadWindowService;
        private readonly ShareRuleService shareRuleService;

        public SaveShareRule(ShareRuleService ruleService)
        {
            InitializeComponent();
            shareRuleService = ruleService;
            loadWindowService = new LoadWindowService();
        }

        private void SaveShareRule_Load(object sender, EventArgs e)
        {
            comboBox_yourRule.DataSource = shareRuleService.NowShareRuleSummary.PrivateRuleList;
            comboBox_yourRule.ValueMember = "Token";
            comboBox_yourRule.DisplayMember = "ShowTag";
            comboBox_yourRule.Text = "please select personal ShareRule";
            rb_newRule_CheckedChanged(sender, e);
        }

        private void rb_newRule_CheckedChanged(object sender, EventArgs e)
        {
            if (rb_newRule.Checked)
            {
                wtb_ruleRemark.Enabled = true;
                comboBox_yourRule.Enabled = false;
            }
            else
            {
                wtb_ruleRemark.Enabled = false;
                comboBox_yourRule.Enabled = true;
            }
        }

        private void bt_save_Click(object sender, EventArgs e)
        {
            if (rb_newRule.Checked)
            {
                if (string.IsNullOrEmpty(wtb_ruleRemark.Text))
                {
                    MessageBox.Show("please enter comment name", "Stop");
                    MyGlobalHelper.markControlService.MarkControl(wtb_ruleRemark, Color.Aquamarine, 2);
                    return;
                }

                shareRuleService.SaveShareRulesAsync(wtb_ruleRemark.Text, ck_parameterData.Checked).ContinueWith(rs =>
                {
                    if (rs.Result.Key == null)
                    {
                        Invoke(new Action(() => MessageBox.Show("Save share rule fail ,Please try again later", "Fail",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning)));
                    }
                    else
                    {
                        shareRuleService.NowShareRuleSummary?.PrivateRuleList.Add(new ShareRuleSummary.RuleToken
                            { Token = rs.Result.Key, Remark = rs.Result.Value });
                        Invoke(new Action(() =>
                            MessageBox.Show($"your share rule [{rs.Result.Value ?? "-"}] save succeed", "succeed",
                                MessageBoxButtons.OK, MessageBoxIcon.Information, MessageBoxDefaultButton.Button1,
                                MessageBoxOptions.DefaultDesktopOnly)));
                        if (this != null && !IsDisposed)
                        {
                            //理论上使用.ConfigureAwait(true)尝试将延续任务封送回原始上下文; 可以不用Invoke，而winform里默认就是true的,不过在这里ContinueWith的前一个任务的上下文大概率已经不是主线程了
                            //所以还是直接使用Invoke保险 （或者Control.CheckForIllegalCrossThreadCalls = false; 但是这个并不推荐，只是在隐藏风险）
                            Invoke(new Action(() =>
                            {
                                (Owner as GetRemoteRuleWindow)?.GotoPrvateRule(rs.Result.Key);
                            }));
                            //(this.Owner as GetRemoteRuleWindow)?.GotoPrvateRule(rs.Result.Key);
                            Close();
                        }
                    }

                    loadWindowService.StopLoad();
                    MyGlobalHelper.PutGlobalMessage(this,
                        new MyGlobalHelper.GlobalMessageEventArgs(false,
                            $"save share rule [{rs.Result.Value}] that taken is : {rs.Result.Key}"));
                });
                loadWindowService.StartLoad(this);
            }
            else if (rb_updataRule.Checked)
            {
                if (string.IsNullOrEmpty((string)comboBox_yourRule.SelectedValue))
                {
                    MessageBox.Show("Update share rule fail ,please choose an existed share rule first ", "Stop",
                        MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    MyGlobalHelper.markControlService.MarkControl(comboBox_yourRule, Color.Pink, 2);
                    return;
                }

                shareRuleService
                    .UpdateShareRulesAsync((string)comboBox_yourRule.SelectedValue, ck_parameterData.Checked)
                    .ContinueWith(rs =>
                    {
                        if (!rs.Result)
                        {
                            Invoke(new Action(() => MessageBox.Show("Update share rule fail ,Please try again later",
                                "Fail", MessageBoxButtons.OK, MessageBoxIcon.Warning)));
                        }
                        else
                        {
                            //ContinueWith 里面如果不用Invoke，MessageBox不会盖在顶层（后面的操作UI代码可能不在主线程）
                            Invoke(new Action(() =>
                                MessageBox.Show($"your share rule [{comboBox_yourRule.Text}] update succeed", "succeed",
                                    MessageBoxButtons.OK, MessageBoxIcon.None)));
                            if (this != null && !IsDisposed)
                            {
                                Invoke(new Action(() =>
                                {
                                    (Owner as GetRemoteRuleWindow)?.GotoPrvateRule(
                                        (string)comboBox_yourRule.SelectedValue);
                                }));
                                Close();
                            }
                        }

                        loadWindowService.StopLoad();
                        MyGlobalHelper.PutGlobalMessage(this,
                            new MyGlobalHelper.GlobalMessageEventArgs(false,
                                $"update share rule [{comboBox_yourRule.Text}]"));
                    });
                loadWindowService.StartLoad(this);
            }
        }
    }
}