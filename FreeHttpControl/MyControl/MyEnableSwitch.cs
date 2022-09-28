using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FreeHttp.FreeHttpControl
{
    public partial class MyEnableSwitch : UserControl
    {
        private bool isEnable;

        private readonly ComponentResourceManager myResources;


        public MyEnableSwitch()
        {
            InitializeComponent();
            myResources = new ComponentResourceManager(typeof(MyEnableSwitch));
        }

        [DescriptionAttribute("the TextBox that you want to binding")]
        /// <summary>
        /// get or set the switch status (set thie value will not call OnChangeEnable)
        /// </summary>
        public bool IsEnable
        {
            get => isEnable;
            set
            {
                isEnable = value;
                pb_switch.Image = isEnable
                    ? (Image)myResources.GetObject("switch_on")
                    : (Image)myResources.GetObject("switch_off");
            }
        }

        public event EventHandler<ChangeEnableEventArgs> OnChangeEnable;

        private void pb_switch_Click(object sender, EventArgs e)
        {
            IsEnable = !IsEnable;
            if (OnChangeEnable != null) OnChangeEnable(this, new ChangeEnableEventArgs(IsEnable));
        }

        public class ChangeEnableEventArgs : EventArgs
        {
            public ChangeEnableEventArgs(bool isEnable)
            {
                IsEnable = isEnable;
            }

            public bool IsEnable { get; set; }
        }
    }
}