using System;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;
using Timer = System.Windows.Forms.Timer;

namespace FreeHttp.FreeHttpControl
{
    public class MarkControlService : IDisposable
    {
        private readonly Timer myTimer = new Timer();
        private readonly Dictionary<Control, RemindControlInfo> remindControlDc;
        private readonly Dictionary<ListViewItem, RemindControlInfo> remindItemDc;

        public MarkControlService(int clickTime)
        {
            remindItemDc = new Dictionary<ListViewItem, RemindControlInfo>();
            remindControlDc = new Dictionary<Control, RemindControlInfo>();
            myTimer.Interval = clickTime;
            myTimer.Tick += myTimer_Tick;
            myTimer.Start();
        }

        public void Dispose()
        {
            myTimer.Dispose();
        }

        private void myTimer_Tick(object sender, EventArgs e)
        {
            if (remindItemDc.Count > 0)
            {
                //MyControlHelper.SetControlFreeze(lv_requestRuleList);
                var tempRemoveItem = new List<ListViewItem>();
                var tempHighlightList = new List<ListViewItem>();
                tempHighlightList.AddRange(remindItemDc.Keys);
                foreach (var tempHighlightItem in tempHighlightList)
                {
                    if (tempHighlightItem == null)
                    {
                        tempRemoveItem.Add(tempHighlightItem);
                        continue;
                    }

                    remindItemDc[tempHighlightItem].RemindTime--;
                    if (remindItemDc[tempHighlightItem].RemindTime == 0) tempRemoveItem.Add(tempHighlightItem);
                }
                //MyControlHelper.SetControlUnfreeze(lv_requestRuleList);

                Monitor.Enter(remindItemDc);
                foreach (var tempItem in tempRemoveItem)
                {
                    tempItem.BackColor = remindItemDc[tempItem].OriginColor;
                    remindItemDc.Remove(tempItem);
                }

                Monitor.Exit(remindItemDc);
            }

            if (remindControlDc.Count > 0)
            {
                var tempRemoveControl = new List<Control>();
                var tempRemindList = new List<Control>();
                tempRemindList.AddRange(remindControlDc.Keys);
                foreach (var tempRemindControl in tempRemindList)
                {
                    remindControlDc[tempRemindControl].RemindTime--;
                    if (remindControlDc[tempRemindControl].RemindTime == 0) tempRemoveControl.Add(tempRemindControl);
                }

                Monitor.Enter(remindControlDc);
                foreach (var tempItem in tempRemoveControl)
                {
                    tempItem.BackColor = remindControlDc[tempItem].OriginColor;
                    remindControlDc.Remove(tempItem);
                }

                Monitor.Exit(remindControlDc);
            }
        }

        public void MarkControl(Control yourControl, Color yourColor, int yourShowTick)
        {
            try
            {
                if (yourControl != null)
                {
                    Monitor.Enter(remindControlDc);
                    if (remindControlDc.ContainsKey(yourControl))
                        remindControlDc[yourControl] =
                            new RemindControlInfo(yourShowTick, remindControlDc[yourControl].OriginColor);
                    else
                        remindControlDc.Add(yourControl, new RemindControlInfo(yourShowTick, yourControl.BackColor));
                    Monitor.Exit(remindControlDc);
                    yourControl.BackColor = yourColor;
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void MarkControl(ListViewItem yourItem, Color yourColor, int yourShowTick)
        {
            try
            {
                if (yourItem != null)
                {
                    Monitor.Enter(remindItemDc);
                    if (remindItemDc.ContainsKey(yourItem))
                        remindItemDc[yourItem] =
                            new RemindControlInfo(yourShowTick, remindItemDc[yourItem].OriginColor);
                    else
                        remindItemDc.Add(yourItem, new RemindControlInfo(yourShowTick, yourItem.BackColor));
                    Monitor.Exit(remindItemDc);
                    yourItem.BackColor = yourColor;
                }
            }
            catch (Exception ex)
            {
            }
        }

        public void SetColor(Control yourControl, Color yourColor)
        {
            if (yourControl != null)
            {
                if (remindControlDc.ContainsKey(yourControl)) remindControlDc.Remove(yourControl);
                yourControl.BackColor = yourColor;
            }
        }

        public void SetColor(ListViewItem yourItem, Color yourColor)
        {
            if (yourItem != null)
            {
                if (remindItemDc.ContainsKey(yourItem)) remindItemDc.Remove(yourItem);
                yourItem.BackColor = yourColor;
            }
        }

        /// <summary>
        ///     the information for the mark Control
        /// </summary>
        private class RemindControlInfo
        {
            public RemindControlInfo(int yourRemindTime, Color yourOriginColor)
            {
                RemindTime = yourRemindTime;
                OriginColor = yourOriginColor;
            }

            public int RemindTime { get; set; }
            public Color OriginColor { get; }
        }
    }
}