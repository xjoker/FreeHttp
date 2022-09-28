using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FreeHttp.FreeHttpControl
{
    internal class MyEnabledPictureButton : PictureBox
    {
        private Image enabledImage;

        public MyEnabledPictureButton()
        {
            //this.MouseMove += pictureBox_MouseMove;
            //this.MouseLeave += pictureBox_MouseLeave;
            Cursor = Cursors.Hand;
            SizeMode = PictureBoxSizeMode.StretchImage;
        }

        /// <summary>
        ///     不可用时显示的图片
        /// </summary>
        [DescriptionAttribute("")]
        public Image DisEnabledImage { get; set; }

        /// <summary>
        ///     可用时显示的图片
        /// </summary>
        [DescriptionAttribute("")]
        public Image EnabledImage
        {
            get => enabledImage;
            set => enabledImage = Image = value;
        }

        public new bool Enabled
        {
            get => base.Enabled;
            set
            {
                base.Enabled = value;
                if (base.Enabled)
                {
                    if (EnabledImage != null) Image = EnabledImage;
                    Cursor = Cursors.Hand;
                }
                else
                {
                    if (DisEnabledImage != null) Image = DisEnabledImage;
                    Cursor = Cursors.No;
                }
            }
        }

        protected override void OnMouseMove(MouseEventArgs e)
        {
            pictureBox_MouseMove(this, e);
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            pictureBox_MouseLeave(this, e);
        }

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
    }
}