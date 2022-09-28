//#define NET4_5UP

using System;
using System.Diagnostics;
using System.Windows.Forms;
using FreeHttp.WebService;

namespace FreeHttp.FreeHttpControl
{
    public partial class UserFeedbackWindow : Form
    {
        private readonly FreeHttpWindow mainWindow;

        public UserFeedbackWindow(FreeHttpWindow freeHttpWindow)
        {
            InitializeComponent();
            mainWindow = freeHttpWindow;
        }

        private void Llb_gotoGitHub_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            Process.Start("https://github.com/lulianqi/FreeHttp/issues");
        }

        private void Bt_ok_Click(object sender, EventArgs e)
        {
            if (rtb_feedbackContent.Text == "")
            {
                MessageBox.Show("Please enter content", "Stop", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

#if NET4_5UP
            var submitFeedback = FeedbackService.SubmitFeedbackAsync(UserComputerInfo.UserToken,
                UserComputerInfo.GetComputerMac(), UserComputerInfo.GetMachineName(), watermakTextBox_contactInfo.Text,
                rtb_feedbackContent.Text);
            submitFeedback.ContinueWith(task =>
            {
                if (mainWindow == null) return;
                if (!(task.Result == 200 || task.Result == 201))
                    mainWindow.PutError($"submit feedback fial with {task.Result}");
                else
                    mainWindow.PutInfo("submit feedback succeed");
            });
#endif

#if NET4
            WebService.FeedbackService.SubmitFeedbackTask(WebService.UserComputerInfo.GetComputerMac(), watermakTextBox_contactInfo.Text, rtb_feedbackContent.Text,new Action<int>((code) => { if (mainWindow == null) return; if (!(code == 200 || code ==201)) { mainWindow.PutError(string.Format("submit feedback fial with {0}", code)); } else { mainWindow.PutInfo("submit feedback succeed"); } }));
#endif
            Close();
        }
    }
}