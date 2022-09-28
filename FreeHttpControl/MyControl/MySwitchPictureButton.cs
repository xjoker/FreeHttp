using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FreeHttp.FreeHttpControl
{
    internal class MySwitchPictureButton : PictureBox
    {
        private Image switchOnImage;

        private bool switchState = true;

        public MySwitchPictureButton()
        {
            Cursor = Cursors.Hand;
            SizeMode = PictureBoxSizeMode.StretchImage;
            //if(IsAutoChangeSwitchState)
            //{
            //    this.Click += (sender, e) => { SwitchState = !SwitchState; };
            //}
        }


        [DescriptionAttribute("Is auto change switchState when click")]
        public bool IsAutoChangeSwitchState { get; set; } = false;

        /// <summary>
        ///     备用状态显示的图片
        /// </summary>
        [DescriptionAttribute("Image when switchState is false")]
        public Image SwitchOffImage { get; set; }

        /// <summary>
        ///     主要状态显示的图片
        /// </summary>
        [DescriptionAttribute("Image when switchState is true")]
        public Image SwitchOnImage
        {
            get => switchOnImage;
            set => switchOnImage = switchOnImage = value;
        }

        public bool SwitchState
        {
            get => switchState;
            set
            {
                switchState = value;
                if (switchState)
                {
                    if (SwitchOnImage != null) Image = SwitchOnImage;
                }
                else
                {
                    if (SwitchOffImage != null) Image = SwitchOffImage;
                }
            }
        }

        protected override void OnClick(EventArgs e)
        {
            if (IsAutoChangeSwitchState) SwitchState = !SwitchState;
            base.OnClick(e);
            //else
            //{

            //}
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