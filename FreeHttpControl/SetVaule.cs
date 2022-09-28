using System;
using System.Windows.Forms;

namespace FreeHttp.FreeHttpControl
{
    public partial class SetVaule : Form
    {
        private readonly Func<string, string> CheckValueFunc;

        public SetVaule()
        {
            InitializeComponent();
        }

        public SetVaule(string formName, string remarkInfo, string nowValue, Func<string, string> checkValueDelegate)
            : this()
        {
            if (formName != null) Text = Name = formName;
            if (remarkInfo != null) lb_info.Text = remarkInfo;
            if (nowValue != null) tb_vaule.Text = nowValue;
            if (checkValueDelegate != null) CheckValueFunc = checkValueDelegate;
        }

        public event EventHandler<SetVauleEventArgs> OnSetValue;

        private void SetVaule_Load(object sender, EventArgs e)
        {
            //this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximumSize = Size;
            MinimumSize = Size;
        }

        private void bt_ok_Click(object sender, EventArgs e)
        {
            if (CheckValueFunc != null)
            {
                var checkMes = CheckValueFunc(tb_vaule.Text);
                if (checkMes != null)
                {
                    MessageBox.Show($"your value is not legal \r\n{checkMes}\r\n edit it again", "Stop",
                        MessageBoxButtons.OK, MessageBoxIcon.Stop);
                    return;
                }
            }

            if (OnSetValue != null) OnSetValue(this, new SetVauleEventArgs(tb_vaule.Text));
            DialogResult = DialogResult.OK;
            Close();
        }

        public class SetVauleEventArgs : EventArgs
        {
            public SetVauleEventArgs(string setValue)
            {
                SetValue = setValue;
            }

            public string SetValue { get; set; }
        }
    }
}