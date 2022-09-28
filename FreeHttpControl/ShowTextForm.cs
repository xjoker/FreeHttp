using System;
using System.Windows.Forms;

namespace FreeHttp.FreeHttpControl
{
    public partial class ShowTextForm : Form
    {
        public ShowTextForm()
        {
            InitializeComponent();
        }

        public ShowTextForm(string name, string textInfo) : this()
        {
            Text = string.IsNullOrEmpty(name) ? "" : name;
            if (textInfo != null) rtb_textInfo.AppendText(textInfo);
        }

        private void ShowTextForm_Load(object sender, EventArgs e)
        {
        }
    }
}