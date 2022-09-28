namespace FreeHttp.FreeHttpControl
{
    partial class EditListView
    {
        /// <summary> 
        /// 必需的设计器变量。
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// 清理所有正在使用的资源。
        /// </summary>
        /// <param name="disposing">如果应释放托管资源，为 true；否则为 false。</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region 组件设计器生成的代码

        /// <summary> 
        /// 设计器支持所需的方法 - 不要
        /// 使用代码编辑器修改此方法的内容。
        /// </summary>
        private void InitializeComponent()
        {
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(EditListView));
            this.ListDataView = new System.Windows.Forms.ListView();
            this.columnHeader_data = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.pictureBox_add = new System.Windows.Forms.PictureBox();
            this.pictureBox_remove = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_add)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_remove)).BeginInit();
            this.SuspendLayout();
            // 
            // lv_dataList
            // 
            this.ListDataView.BackColor = System.Drawing.SystemColors.InactiveBorder;
            this.ListDataView.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.ListDataView.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.columnHeader_data});
            this.ListDataView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.ListDataView.FullRowSelect = true;
            this.ListDataView.LabelEdit = true;
            this.ListDataView.Location = new System.Drawing.Point(0, 0);
            this.ListDataView.Name = "ListDataView";
            this.ListDataView.Size = new System.Drawing.Size(379, 134);
            this.ListDataView.TabIndex = 49;
            this.ListDataView.UseCompatibleStateImageBehavior = false;
            this.ListDataView.View = System.Windows.Forms.View.Details;
            this.ListDataView.DoubleClick += new System.EventHandler(this.lv_dataList_DoubleClick);
            // 
            // columnHeader_data
            // 
            this.columnHeader_data.Text = "column name";
            this.columnHeader_data.Width = 375;
            // 
            // pictureBox_add
            // 
            this.pictureBox_add.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox_add.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox_add.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox_add.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_add.Image")));
            this.pictureBox_add.Location = new System.Drawing.Point(336, 1);
            this.pictureBox_add.Name = "pictureBox_add";
            this.pictureBox_add.Size = new System.Drawing.Size(20, 20);
            this.pictureBox_add.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_add.TabIndex = 51;
            this.pictureBox_add.TabStop = false;
            this.pictureBox_add.Click += new System.EventHandler(this.pictureBox_add_Click);
            this.pictureBox_add.MouseLeave += new System.EventHandler(this.pictureBox_MouseLeave);
            this.pictureBox_add.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseMove);
            // 
            // pictureBox_remove
            // 
            this.pictureBox_remove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pictureBox_remove.BackColor = System.Drawing.Color.Transparent;
            this.pictureBox_remove.Cursor = System.Windows.Forms.Cursors.Hand;
            this.pictureBox_remove.Image = ((System.Drawing.Image)(resources.GetObject("pictureBox_remove.Image")));
            this.pictureBox_remove.Location = new System.Drawing.Point(356, 1);
            this.pictureBox_remove.Name = "pictureBox_remove";
            this.pictureBox_remove.Size = new System.Drawing.Size(20, 20);
            this.pictureBox_remove.SizeMode = System.Windows.Forms.PictureBoxSizeMode.Zoom;
            this.pictureBox_remove.TabIndex = 50;
            this.pictureBox_remove.TabStop = false;
            this.pictureBox_remove.Click += new System.EventHandler(this.pictureBox_remove_Click);
            this.pictureBox_remove.MouseLeave += new System.EventHandler(this.pictureBox_MouseLeave);
            this.pictureBox_remove.MouseMove += new System.Windows.Forms.MouseEventHandler(this.pictureBox_MouseMove);
            // 
            // EditListView
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.pictureBox_add);
            this.Controls.Add(this.pictureBox_remove);
            this.Controls.Add(this.ListDataView);
            this.Name = "EditListView";
            this.Size = new System.Drawing.Size(379, 134);
            this.Load += new System.EventHandler(this.EditListView_Load);
            this.Resize += new System.EventHandler(this.EditListView_Resize);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_add)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox_remove)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pictureBox_add;
        private System.Windows.Forms.PictureBox pictureBox_remove;
        private System.Windows.Forms.ColumnHeader columnHeader_data;
    }
}
