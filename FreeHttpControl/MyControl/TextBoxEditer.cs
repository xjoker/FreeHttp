using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FreeHttp.FreeHttpControl
{
    public partial class TextBoxEditer : UserControl
    {
        public delegate void CloseEditBoxEventHandler(object sender, CloseEditBoxEventArgs e);

        private TextBox editTextBox;

        private bool
            isShowEditRichTextBox; //not set this vaule (you should call IsShowEditRichTextBox to set this vaule)

        private readonly ComponentResourceManager myResources;

        private readonly RichTextBox rtb_editTextBox = new RichTextBox();

        public TextBoxEditer()
        {
            InitializeComponent();
            myResources = new ComponentResourceManager(typeof(TextBoxEditer));
            rtb_editTextBox.Leave += rtb_editTextBox_Leave;
            rtb_editTextBox.DetectUrls = false;
            rtb_editTextBox.BackColor = Color.AliceBlue;
            IsShowEditRichTextBox = false;
        }

        public TextBoxEditer(ContainerControl yourContainerControl) : this()
        {
            MainContainerControl = yourContainerControl;
        }

        /// <summary>
        ///     get a value that is RichTextBox showing
        /// </summary>
        public bool IsShowEditRichTextBox
        {
            get => isShowEditRichTextBox;
            private set
            {
                if (isShowEditRichTextBox == value) return;
                isShowEditRichTextBox = value;
                pb_editTextBox.Image = isShowEditRichTextBox
                    ? (Image)myResources.GetObject("zoomsmall")
                    : (Image)myResources.GetObject("zoombig");
            }
        }


        [DescriptionAttribute("the TextBox that you want to binding")]
        /// <summary>
        /// get or set EditTextBox (the TextBox that you want to binding)
        /// </summary>
        public TextBox EditTextBox
        {
            get => editTextBox;
            set
            {
                editTextBox = value;
                if (editTextBox != null)
                {
                    BackColor = editTextBox.BackColor;
                    editTextBox.Resize += editTextBox_Resize;
                    editTextBox.Move += editTextBox_Move;
                }
            }
        }

        [DescriptionAttribute("the main window that RichTextBox will add it")]
        /// <summary>
        /// get or set MainContainerControl (the main window that RichTextBox will add it)
        /// </summary>
        public ContainerControl MainContainerControl { get; set; }

        public event CloseEditBoxEventHandler OnCloseEditBox;

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

        private void pb_editTextBox_Click(object sender, EventArgs e)
        {
            if (IsShowEditRichTextBox)
                CloseRichTextBox();
            else
                ShowRichTextBox();
        }

        private void rtb_editTextBox_Leave(object sender, EventArgs e)
        {
            CloseRichTextBox();
        }

        private void editTextBox_Resize(object sender, EventArgs e)
        {
            if (rtb_editTextBox != null && IsShowEditRichTextBox) rtb_editTextBox.Width = ((TextBox)sender).Width;
        }

        private void editTextBox_Move(object sender, EventArgs e)
        {
            if (rtb_editTextBox != null && IsShowEditRichTextBox)
            {
                var myClientLocation =
                    MainContainerControl.PointToClient(EditTextBox.Parent.PointToScreen(EditTextBox.Location));
                rtb_editTextBox.Location = new Point(myClientLocation.X, myClientLocation.Y + EditTextBox.Height);
            }
        }

        /// <summary>
        ///     Close EditRichTextBox  (when the windows Deactivate you should call it )
        /// </summary>
        public void CloseRichTextBox()
        {
            if (!IsShowEditRichTextBox || MainContainerControl == null || EditTextBox == null) return;
            if (MainContainerControl.Contains(rtb_editTextBox))
            {
                EditTextBox.Clear();
                EditTextBox.AppendText(rtb_editTextBox.Text);
                MainContainerControl.Controls.Remove(rtb_editTextBox);
                IsShowEditRichTextBox = false;
                if (OnCloseEditBox != null) OnCloseEditBox(this, new CloseEditBoxEventArgs(rtb_editTextBox.Text));
            }
        }

        /// <summary>
        ///     Show EditRichTextBox
        /// </summary>
        public void ShowRichTextBox()
        {
            if (MainContainerControl == null || EditTextBox == null) return;

            if (!IsShowEditRichTextBox)
            {
                var myClientLocation =
                    MainContainerControl.PointToClient(EditTextBox.Parent.PointToScreen(EditTextBox.Location));
                rtb_editTextBox.Location = new Point(myClientLocation.X, myClientLocation.Y + EditTextBox.Height);
                rtb_editTextBox.Width = EditTextBox.Width;
                rtb_editTextBox.Height = 200;
                rtb_editTextBox.Clear();
                MainContainerControl.Controls.Add(rtb_editTextBox);
                IsShowEditRichTextBox = true;
                rtb_editTextBox.BringToFront();
                rtb_editTextBox.AppendText(EditTextBox.Text);
                rtb_editTextBox.Focus();
            }
        }

        public class CloseEditBoxEventArgs : EventArgs
        {
            public CloseEditBoxEventArgs(string editData)
            {
                EditData = editData;
            }

            public string EditData { get; set; }
        }
    }
}