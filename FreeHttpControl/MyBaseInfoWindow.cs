using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;


/*******************************************************************************
* Copyright (c) 2015 lijie
* All rights reserved.
* 
* 文件名称: 
* 内容摘要: mycllq@hotmail.com
* 
* 历史记录:
* 日	  期:   201505016           创建人: 李杰 15158155511
* 描    述: 创建
*******************************************************************************/


namespace FreeHttp.FreeHttpControl
{
    public class MyBaseInfoWindow : Form
    {
        private bool isMoveForm;
        private bool isShowHideBox = true;
        private Label lb_info;
        private Point myFormStartPos = new Point(0, 0);
        private readonly Timer myUpdataTime = new Timer();

        private string myWindowName = "unknow";

        private PictureBox pictureBox_close;
        private PictureBox pictureBox_hide;
        private Point tempCrtPos = new Point(0, 0);

        public MyBaseInfoWindow()
        {
            InitializeComponent();
        }

        /// <summary>
        ///     获取或设置自定义窗体名称
        /// </summary>
        [DescriptionAttribute("窗体名称")]
        public string WindowName
        {
            get => myWindowName;
            set => myWindowName = Text = lb_info.Text = value;
        }

        /// <summary>
        ///     获取或设置是否显示最最小化按钮
        /// </summary>
        [Description("是否显示最最小化按钮")]
        public bool IsShowHideBox
        {
            get => isShowHideBox;
            set => isShowHideBox = pictureBox_hide.Visible = value;
        }

        /// <summary>
        ///     获取或设置定时刷新窗体的时间
        /// </summary>
        [Description("获取或设置定时刷新窗体的时间")]
        public int IntervalTime { get; set; } = 0;

        private void InitializeComponent()
        {
            lb_info = new Label();
            pictureBox_hide = new PictureBox();
            pictureBox_close = new PictureBox();
            ((ISupportInitialize)pictureBox_hide).BeginInit();
            ((ISupportInitialize)pictureBox_close).BeginInit();
            SuspendLayout();
            // 
            // lb_info
            // 
            lb_info.AutoSize = true;
            lb_info.Font = new Font("宋体", 9.75F, FontStyle.Bold, GraphicsUnit.Point, 134);
            lb_info.ForeColor = Color.SaddleBrown;
            lb_info.Location = new Point(3, 4);
            lb_info.Name = "lb_info";
            lb_info.Size = new Size(111, 13);
            lb_info.TabIndex = 12;
            lb_info.Text = "CaseParameter";
            // 
            // pictureBox_hide
            // 
            pictureBox_hide.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pictureBox_hide.Cursor = Cursors.Hand;
            pictureBox_hide.Image = Properties.Resources.minimize;
            pictureBox_hide.Location = new Point(344, 3);
            pictureBox_hide.Name = "pictureBox_hide";
            pictureBox_hide.Size = new Size(23, 23);
            pictureBox_hide.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox_hide.TabIndex = 13;
            pictureBox_hide.TabStop = false;
            pictureBox_hide.Click += pictureBox_hide_Click;
            pictureBox_hide.MouseLeave += pictureBox_MouseLeave;
            pictureBox_hide.MouseMove += pictureBox_MouseMove;
            // 
            // pictureBox_close
            // 
            pictureBox_close.Anchor = AnchorStyles.Top | AnchorStyles.Right;
            pictureBox_close.Cursor = Cursors.Hand;
            pictureBox_close.Image = Properties.Resources.close;
            pictureBox_close.Location = new Point(373, 3);
            pictureBox_close.Name = "pictureBox_close";
            pictureBox_close.Size = new Size(23, 23);
            pictureBox_close.SizeMode = PictureBoxSizeMode.Zoom;
            pictureBox_close.TabIndex = 10;
            pictureBox_close.TabStop = false;
            pictureBox_close.Click += pictureBox_close_Click;
            pictureBox_close.MouseLeave += pictureBox_MouseLeave;
            pictureBox_close.MouseMove += pictureBox_MouseMove;
            // 
            // MyBaseInfoWindow
            // 
            BackColor = SystemColors.GradientActiveCaption;
            ClientSize = new Size(400, 250);
            Controls.Add(pictureBox_hide);
            Controls.Add(lb_info);
            Controls.Add(pictureBox_close);
            FormBorderStyle = FormBorderStyle.None;
            Name = "MyBaseInfoWindow";
            FormClosing += MyBaseInfoWindow_FormClosing;
            Load += myCaseParameter_Load;
            MouseDown += MyBaseInfoWindow_MouseDown;
            MouseMove += MyBaseInfoWindow_MouseMove;
            MouseUp += MyBaseInfoWindow_MouseUp;
            ((ISupportInitialize)pictureBox_hide).EndInit();
            ((ISupportInitialize)pictureBox_close).EndInit();
            ResumeLayout(false);
            PerformLayout();
        }

        public void myCaseParameter_Load(object sender, EventArgs e)
        {
            pictureBox_hide.Visible = isShowHideBox;
            TopMost = false;
            if (IntervalTime > 0)
            {
                myUpdataTime.Interval = IntervalTime;
                myUpdataTime.Enabled = true;
                myUpdataTime.Tick += myUpdataTime_Tick;
                myUpdataTime.Start();
            }

            lb_info.Text = myWindowName;
            Text = myWindowName;
            MaximizeBox = false;

            pictureBox_hide.Location = new Point(Width - 56, 4);
            pictureBox_close.Location = new Point(Width - 27, 4);
            Resize += MyChildWindow_Resize;
        }

        private void MyChildWindow_Resize(object sender, EventArgs e)
        {
            pictureBox_hide.Location = new Point(Width - 56, 4);
            pictureBox_close.Location = new Point(Width - 27, 4);
        }

        public void myUpdataTime_Tick(object sender, EventArgs e)
        {
            VirtualUpdataTime_Tick();
        }

        public virtual void VirtualUpdataTime_Tick()
        {
        }

        private void pictureBox_close_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void pictureBox_hide_Click(object sender, EventArgs e)
        {
            WindowState = FormWindowState.Minimized;
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

        private void MyBaseInfoWindow_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                isMoveForm = true;
                myFormStartPos = new Point(-e.X, -e.Y); //相对当前控件的鼠标位置
                tempCrtPos = PointToScreen(new Point(-Location.X, -Location.Y)); //控件相对与容器转换为相对于屏幕
            }
        }

        private void MyBaseInfoWindow_MouseUp(object sender, MouseEventArgs e)
        {
            isMoveForm = false;
        }

        private void MyBaseInfoWindow_MouseMove(object sender, MouseEventArgs e)
        {
            if (isMoveForm)
            {
                var nowMousePos = MousePosition; //鼠标光标相对屏幕的位置
                nowMousePos.Offset(myFormStartPos);
                //this.Location = nowMousePos;//相对于父窗体，（如果没有父窗体则可以这样用）
                Location = new Point(nowMousePos.X - tempCrtPos.X, nowMousePos.Y - tempCrtPos.Y);
            }
        }

        private void MyBaseInfoWindow_FormClosing(object sender, FormClosingEventArgs e)
        {
            myUpdataTime.Tick -= myUpdataTime_Tick;
        }
    }
}