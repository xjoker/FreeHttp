using System;
using System.Drawing;
using System.Windows.Forms;
using FreeHttp.HttpHelper;

namespace FreeHttp.FreeHttpControl
{
    public partial class RawRequestEdit : UserControl
    {
        public RawRequestEdit()
        {
            InitializeComponent();
        }

        public event EventHandler OnRawRequestEditClose;

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

        public void SetText(string mes)
        {
            rtb_request.Text = mes;
        }

        public HttpRequest GetHttpRequest()
        {
            return HttpRequest.GetHttpRequest(rtb_request.Text.Replace("\n", "\r\n"));
        }

        private void pictureBox_changeMode_Click(object sender, EventArgs e)
        {
            Visible = false;
            if (OnRawRequestEditClose != null) OnRawRequestEditClose(this, null);
        }
    }
}