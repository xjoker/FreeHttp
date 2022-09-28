using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace FreeHttp.FreeHttpControl
{
    public partial class EditListView : UserControl
    {
        public EditListView()
        {
            InitializeComponent();
            columnHeader_data.Text = ColumnHeaderName;
            SplitStr = SplitStr == null ? ": " : SplitStr;
        }

        /// <summary>
        ///     编辑或添加时 key value 的默认分割
        /// </summary>
        [DescriptionAttribute("编辑或添加时 key value 的默认分割")]
        public string SplitStr { get; set; }

        /// <summary>
        ///     是否以key value方式显示
        /// </summary>
        [DescriptionAttribute("是否以key value方式显示")]
        public bool IsKeyValue { get; set; }

        /// <summary>
        ///     List Item 的值是否保持唯一性
        /// </summary>
        [DescriptionAttribute("编辑或添加时List Item 的值是否保持唯一性")]
        public bool IsItemUnique { get; set; }

        /// <summary>
        ///     可用于显示的列名
        /// </summary>
        [DescriptionAttribute("可用于显示的列名")]
        public string ColumnHeaderName { get; set; }

        public ListView ListDataView { get; private set; }

        private void EditListView_Load(object sender, EventArgs e)
        {
            columnHeader_data.Text = ColumnHeaderName;
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

        private void EditListView_Resize(object sender, EventArgs e)
        {
            columnHeader_data.Width = ListDataView.Width;
        }

        private void pictureBox_add_Click(object sender, EventArgs e)
        {
            if (IsKeyValue)
            {
                var f = new EditKeyVaule(ListDataView, true, IsItemUnique, SplitStr);
                f.ShowDialog();
            }
            else
            {
                var f = new RemoveHead(ListDataView, true, IsItemUnique);
                f.ShowDialog();
            }
        }

        private void pictureBox_remove_Click(object sender, EventArgs e)
        {
            if (ListDataView.SelectedItems.Count > 0)
            {
                var tempRemoveIndex = ListDataView.SelectedItems.Count - 1;
                for (var i = tempRemoveIndex; i >= 0; i--) ListDataView.Items.Remove(ListDataView.SelectedItems[i]);
            }
            else if (ListDataView.Items.Count > 0)
            {
                if (MessageBox.Show("if you want remove all data", "remove all", MessageBoxButtons.OKCancel,
                        MessageBoxIcon.Question) == DialogResult.OK) ListDataView.Items.Clear();
            }
            else
            {
                MessageBox.Show("nothing to clear");
            }
        }

        private void lv_dataList_DoubleClick(object sender, EventArgs e)
        {
            if (ListDataView.SelectedItems.Count > 0)
            {
                if (IsKeyValue)
                {
                    var f = new EditKeyVaule(ListDataView, false, IsItemUnique, SplitStr);
                    f.ShowDialog();
                }
                else
                {
                    var f = new RemoveHead(ListDataView, IsItemUnique, false);
                    f.ShowDialog();
                }
            }
        }
    }
}