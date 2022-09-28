using System;
using System.Drawing;
using System.Timers;
using System.Windows.Forms;
using Timer = System.Timers.Timer;

namespace FreeHttp.FreeHttpControl.ControlHelper
{
    public class LoadWindowService
    {
        private readonly Timer asyncTimer = new Timer();
        private bool isInload;
        private readonly LoadBitmap loadBitmap = new LoadBitmap(new Size(100, 100));
        private Form loadForm;
        private int loadTime;
        private readonly PictureBox pictureBox = new PictureBox();
        private readonly System.Windows.Forms.Timer timer = new System.Windows.Forms.Timer();

        public LoadWindowService()
        {
            pictureBox.SizeMode = PictureBoxSizeMode.Zoom;
            timer.Interval = 300;
            timer.Tick += Timer_Tick;
            asyncTimer.Elapsed += Timer_Elapsed;
        }

        private void Timer_Elapsed(object sender, ElapsedEventArgs e)
        {
            Timer_Tick(null, null);
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            if (loadForm == null || loadForm.Created == false)
            {
                StopLoad();
                return;
            }

            pictureBox.Image = loadBitmap.DrawCircle(loadTime);
            loadTime++;
        }

        public void StartLoad(Form form, bool isAsync = false)
        {
            if (isInload) return;
            loadForm = form;
            loadForm.Controls.Add(pictureBox);
            loadForm.FormClosed += (o, e) => { StopLoad(); };
            pictureBox.Dock = DockStyle.Fill;
            pictureBox.BringToFront();
            loadBitmap.SetSize(pictureBox.Width > pictureBox.Height ? pictureBox.Height : pictureBox.Width);
            isInload = true;
            loadTime = 0;
            if (isAsync)
                asyncTimer.Start();
            else
                timer.Start();
        }

        public void StopLoad()
        {
            if (!isInload) return;
            loadForm?.Controls.Remove(pictureBox);
            loadForm = null;
            isInload = false;
            timer.Stop();
            asyncTimer.Stop();
        }
    }
}