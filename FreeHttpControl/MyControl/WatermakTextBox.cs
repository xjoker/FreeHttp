using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace FreeHttp.FreeHttpControl
{
    internal class WatermakTextBox : TextBox
    {
        private const uint ECM_FIRST = 0x1500;
        private const uint EM_SETCUEBANNER = ECM_FIRST + 1;
        private string watermarkText;

        [Category("扩展属性")]
        [Description("显示的水印提示信息")]
        public string WatermarkText
        {
            get => watermarkText;
            set
            {
                watermarkText = value;
                SetWatermark(watermarkText);
            }
        }


        [DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = false)]
        public static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, uint wParam,
            [MarshalAs(UnmanagedType.LPWStr)] string lParam);

        private void SetWatermark(string watermarkText)
        {
            SendMessage(Handle, EM_SETCUEBANNER, 0, watermarkText);
        }
    }
}