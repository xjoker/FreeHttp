using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Security;
using System.Windows.Forms;

namespace FreeHttp.FreeHttpControl
{
    public class MyControlHelper
    {
        private const int WM_SETREDRAW = 0xB;

        /// <summary>
        ///     停止控件刷新
        /// </summary>
        /// <param name="yourCtr">your Control</param>
        public static void SetControlFreeze(Control yourCtr)
        {
            UnsafeNativeMethods.SendMessage(yourCtr.Handle, WM_SETREDRAW, 0, IntPtr.Zero);
        }

        /// <summary>
        ///     恢复控件刷新
        /// </summary>
        /// <param name="yourCtr">your Control</param>
        public static void SetControlUnfreeze(Control yourCtr)
        {
            UnsafeNativeMethods.SendMessage(yourCtr.Handle, WM_SETREDRAW, 1, IntPtr.Zero);
            yourCtr.Refresh();
        }

        /// <summary>
        ///     为TextBoxBase 控件添加拖放数据的功能
        /// </summary>
        /// <param name="yourCtr">需要启用拖放数据的控件</param>
        /// <param name="action">拖放完成后的辅助事件</param>
        public static void SetRichTextBoxDropString(TextBoxBase yourCtr, Action action = null)
        {
            if (yourCtr == null) return;
            if (yourCtr is RichTextBox)
                ((RichTextBox)yourCtr).AllowDrop = true;
            else if (yourCtr is TextBox)
                ((TextBox)yourCtr).AllowDrop = true;
            else
                yourCtr.AllowDrop = true;
            yourCtr.DragDrop += (sender, e) =>
            {
                var tempTextBoxBase = sender as TextBoxBase;
                var tempText = (string)e.Data.GetData(typeof(string));
                if (tempText == null || tempTextBoxBase == null) return;
                var selectionStart = tempTextBoxBase.SelectionStart;
                tempTextBoxBase.Text = tempTextBoxBase.Text.Insert(selectionStart, tempText);
                tempTextBoxBase.Select(selectionStart, tempText.Length);
                action?.Invoke();
            };
            yourCtr.DragEnter += (sender, e) =>
            {
                if (e.Data.GetData(typeof(string)) == null)
                    e.Effect = DragDropEffects.None;
                else
                    e.Effect = DragDropEffects.Move;
            };
        }


        public static void SetRichTextBoxDropString(RichTextBox yourRtb)
        {
            if (yourRtb == null) return;
            yourRtb.AllowDrop = true;
            yourRtb.DragDrop += Rtb_DragDrop;
            yourRtb.DragEnter += Rtb_DragEnter;
        }

        private static void Rtb_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetData(typeof(string)) == null) e.Effect = DragDropEffects.None;
        }

        private static void Rtb_DragDrop(object sender, DragEventArgs e)
        {
            var tempRichTextBox = sender as RichTextBox;
            var tempText = (string)e.Data.GetData(typeof(string));
            if (tempText == null || tempRichTextBox == null) return;
            var selectionStart = tempRichTextBox.SelectionStart;
            tempRichTextBox.Text = tempRichTextBox.Text.Insert(selectionStart, tempText);
            tempRichTextBox.Select(selectionStart, tempText.Length);
        }
    }

    [SuppressUnmanagedCodeSecurity]
    internal static class UnsafeNativeMethods
    {
        [DllImport("user32")]
        public static extern int SendMessage(IntPtr hwnd, int wMsg, int wParam, IntPtr lParam);
    }

    public static class MyExtensionMethods
    {
        public static int GetLatency(this LinkLabel llb)
        {
            //delay:200ms
            var tempText = llb.Text;
            var latency = 0;
            if (tempText.StartsWith("delay:") && tempText.EndsWith("ms"))
            {
                tempText = tempText.Substring(6, tempText.Length - 8);
                if (!int.TryParse(tempText, out latency)) latency = 0;
            }

            return latency;
        }

        public static void SetLatency(this LinkLabel llb, int latency)
        {
            if (latency > 0)
                llb.Text = string.Format("delay:{0}ms", latency);
            else
                llb.Text = "";
        }

        /// <summary>
        ///     添加带颜色内容
        /// </summary>
        /// <param name="rtb">目标richtextbox</param>
        /// <param name="strInput">输入内容</param>
        /// <param name="fontColor">颜色</param>
        /// <param name="isNewLine">是否换行</param>
        public static void AddRtbStr(this RichTextBox rtb, string strInput, Color fontColor, bool isNewLine,
            Font font = null)
        {
            lock (rtb)
            {
                var p1 = rtb.TextLength;
                //rtb.SelectionColor = fontColor;
                if (isNewLine)
                    rtb.AppendText(strInput + "\n"); //保留每行的所有颜色。 //  rtb.Text += strInput + "/n";  //添加时，仅当前行有颜色。    
                else
                    rtb.AppendText(strInput);
                var p2 = strInput.Length;
                rtb.Select(p1, p2);
                rtb.SelectionColor = fontColor;
                //rtb.SelectionFont = new Font(FontFamily.GenericMonospace, 14);
                if (font != null) rtb.SelectionFont = font;
                rtb.Select(rtb.TextLength, 0);
                rtb.SelectionColor = rtb.ForeColor;
                if (font != null) rtb.SelectionFont = rtb.Font;
            }
        }
    }
}