﻿using System;
using System.Windows.Forms;
using Fiddler;
using FreeHttp.FiddlerHelper;
using FreeHttp.MyHelper;
using FreeHttp.WebService.HttpServer;

namespace FreeHttp.FreeHttpControl
{
    public partial class SettingWindow : Form
    {
        private readonly FiddlerModificSettingInfo modifcSettingInfo;

        public SettingWindow()
        {
            InitializeComponent();
        }

        public SettingWindow(FiddlerModificSettingInfo yourModifcSettingInfo) : this()
        {
            modifcSettingInfo = yourModifcSettingInfo;
        }

        private void SettingWindow_Load(object sender, EventArgs e)
        {
            if (modifcSettingInfo != null)
            {
                myEnableSwitch_IsOnlyMatchFistTamperRule.IsEnable = modifcSettingInfo.IsOnlyMatchFirstTamperRule;
                myEnableSwitch_IsSkipHideUi.IsEnable = modifcSettingInfo.IsSkipUiHide;
                myEnableSwitch_IsSkipConnectTunnels.IsEnable = modifcSettingInfo.IsSkipConnectTunnels;
                myEnableSwitch_IsEnableHttpsService.IsEnable = MyGlobalHelper.myHttpListener.IsStart;
                myEnableSwitch_IsSyncTamperRule.IsEnable = modifcSettingInfo.IsSyncTamperRule;
                myEnableSwitch_IsHideSelfSession.IsEnable = modifcSettingInfo.IsHideSelfSession;
                myEnableSwitch_IsOnlyMatchFistTamperRule.OnChangeEnable +=
                    myEnableSwitch_IsOnlyMatchFistTamperRule_OnChangeEable;
                myEnableSwitch_IsSkipHideUi.OnChangeEnable += myEnableSwitch_IsDefaultEnableRule_OnChangeEable;
                myEnableSwitch_IsSkipConnectTunnels.OnChangeEnable += myEnableSwitch_IsConnectTunnels_OnChangeEable;
                myEnableSwitch_IsEnableHttpsService.OnChangeEnable +=
                    myEnableSwitch_IsEnableHttpsService_OnChangeEnable;
                myEnableSwitch_IsSyncTamperRule.OnChangeEnable += MyEnableSwitch_IsSyncTamperRule_OnChangeEnable;
                myEnableSwitch_IsHideSelfSession.OnChangeEnable += MyEnableSwitch_IsHideSelfSession_OnChangeEnable;
            }

            //this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximumSize = Size;
            MinimumSize = Size;
        }


        private void myEnableSwitch_IsConnectTunnels_OnChangeEable(object sender,
            MyEnableSwitch.ChangeEnableEventArgs e)
        {
            modifcSettingInfo.IsSkipConnectTunnels = e.IsEnable;
        }

        private void myEnableSwitch_IsDefaultEnableRule_OnChangeEable(object sender,
            MyEnableSwitch.ChangeEnableEventArgs e)
        {
            modifcSettingInfo.IsSkipUiHide = e.IsEnable;
        }

        private void myEnableSwitch_IsOnlyMatchFistTamperRule_OnChangeEable(object sender,
            MyEnableSwitch.ChangeEnableEventArgs e)
        {
            modifcSettingInfo.IsOnlyMatchFirstTamperRule = e.IsEnable;
        }

        private void MyEnableSwitch_IsSyncTamperRule_OnChangeEnable(object sender,
            MyEnableSwitch.ChangeEnableEventArgs e)
        {
            modifcSettingInfo.IsSyncTamperRule = e.IsEnable;
        }

        private void MyEnableSwitch_IsHideSelfSession_OnChangeEnable(object sender,
            MyEnableSwitch.ChangeEnableEventArgs e)
        {
            modifcSettingInfo.IsHideSelfSession = e.IsEnable;
        }

        private void myEnableSwitch_IsEnableHttpsService_OnChangeEnable(object sender,
            MyEnableSwitch.ChangeEnableEventArgs e)
        {
            if (e.IsEnable)
            {
                if (!MyGlobalHelper.IsAdministrator())
                {
                    MessageBox.Show(
                        "Enable http sevice need administrator rights \r\nPlease use administrator privileges to open the application",
                        "Stop", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                    myEnableSwitch_IsEnableHttpsService.IsEnable = false;
                    return;
                }

                var isAffirm = false;
                var listenerPort = 443;
                if (MySocketHelper.IsPortInTcpListening(listenerPort))
                {
                    listenerPort = 9990;
                    while (MySocketHelper.IsPortInTcpListening(listenerPort))
                    {
                        listenerPort++;
                        if (listenerPort == 65537)
                        {
                            MessageBox.Show("can not find leisure port from 9990 to 65536", "Stop",
                                MessageBoxButtons.OK, MessageBoxIcon.Stop);
                            myEnableSwitch_IsEnableHttpsService.IsEnable = false;
                            return;
                        }
                    }
                }

                var listenerPrefixesStr = string.Format(@"https://*:{0}/", listenerPort);
                var f = new SetVaule("Set listener prefixes",
                    "you can set listener prefixes for your http service,like https://*:443/", listenerPrefixesStr,
                    checkValue => { return checkValue.StartsWith("http") && checkValue.EndsWith("/") ? null : ""; });
                f.OnSetValue += (obj, tag) =>
                {
                    listenerPrefixesStr = tag.SetValue;
                    isAffirm = true;
                };
                f.ShowDialog();
                if (!isAffirm)
                {
                    myEnableSwitch_IsEnableHttpsService.IsEnable = false;
                    return;
                }

                var portIndex = listenerPrefixesStr.LastIndexOf(":");
                if (!(portIndex > 0 && listenerPrefixesStr.EndsWith("/") && int.TryParse(
                        listenerPrefixesStr.Substring(portIndex + 1, listenerPrefixesStr.Length - portIndex - 2),
                        out listenerPort)))
                {
                    MessageBox.Show("your listener prefixes is illegality", "Stop", MessageBoxButtons.OK,
                        MessageBoxIcon.Stop);
                    myEnableSwitch_IsEnableHttpsService.IsEnable = false;
                    return;
                }

                if (CertMaker.GetRootCertificate() == null)
                {
                    MessageBox.Show("can not get Fiddler RootCertificate", "Stop", MessageBoxButtons.OK,
                        MessageBoxIcon.Stop);
                    myEnableSwitch_IsEnableHttpsService.IsEnable = false;
                    return;
                }

                if (MySocketHelper.IsPortInTcpListening(listenerPort))
                {
                    MessageBox.Show(string.Format("this port {0} is in listening with other process", listenerPort),
                        "Stop", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    myEnableSwitch_IsEnableHttpsService.IsEnable = false;
                    return;
                }

                try
                {
                    CertificatesHelper.AddCertificateToX509Store(CertMaker.GetRootCertificate());
                    CertificatesHelper.BindingCertificate(CertMaker.GetRootCertificate(), listenerPort);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("fail to BindingCertificate with {0}", ex.Message), "Stop",
                        MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    myEnableSwitch_IsEnableHttpsService.IsEnable = false;
                    return;
                }

                try
                {
                    MyGlobalHelper.myHttpListener.Start(listenerPrefixesStr);
                    MyGlobalHelper.PutGlobalMessage(this,
                        new MyGlobalHelper.GlobalMessageEventArgs(false,
                            string.Format("add listening for [{0}]", listenerPrefixesStr)));
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("fail to Start HttpListener with {0}", ex.Message), "Stop",
                        MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    MyGlobalHelper.myHttpListener.Stop();
                    myEnableSwitch_IsEnableHttpsService.IsEnable = false;
                }
            }
            else
            {
                MyGlobalHelper.myHttpListener.Stop();
            }
        }
    }
}