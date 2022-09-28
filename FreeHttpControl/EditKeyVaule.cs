﻿using System;
using System.Windows.Forms;

namespace FreeHttp.FreeHttpControl
{
    public partial class EditKeyVaule : Form
    {
        private readonly ListView editListView;
        private readonly bool isAdd; //add or edit mode
        private readonly bool isUnique; //is not allow repetition
        private readonly string splitStr; //splitStr ": "

        public EditKeyVaule(ListView yourEditListView, bool yourIsAdd, string yourSplitStr)
        {
            InitializeComponent();
            editListView = yourEditListView;
            isAdd = yourIsAdd;
            splitStr = yourSplitStr == null ? ": " : yourSplitStr;
        }

        public EditKeyVaule(ListView yourEditListView, string yourHeadKey, string yourSplitStr)
            : this(yourEditListView, true, yourSplitStr)
        {
            tb_key.Text = yourHeadKey;
            tb_key.Enabled = false;
        }

        public EditKeyVaule(ListView yourEditListView, bool yourIsAdd, bool yourIsUnique, string yourSplitStr)
            : this(yourEditListView, yourIsAdd, yourSplitStr)
        {
            isUnique = yourIsUnique;
        }

        private void EditKeyVaule_Load(object sender, EventArgs e)
        {
            if (!isAdd)
            {
                var headStr = editListView.SelectedItems[0].Text;
                if (headStr.Contains(splitStr))
                {
                    tb_key.Text = headStr.Remove(headStr.IndexOf(splitStr));
                    rtb_value.Text = headStr.Substring(headStr.IndexOf(splitStr) + splitStr.Length);
                }
            }

            //this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximumSize = Size;
            MinimumSize = Size;
        }

        private void bt_ok_Click(object sender, EventArgs e)
        {
            if (tb_key.Text == "" || rtb_value.Text == "")
            {
                MessageBox.Show("input key and value", "Stop", MessageBoxButtons.OK, MessageBoxIcon.Stop);
            }
            else
            {
                var tempItemStr = string.Format("{0}{1}{2}", tb_key.Text, splitStr, rtb_value.Text);
                if (isUnique)
                    foreach (ListViewItem tempItem in editListView.Items)
                        if (tempItem.Text == tempItemStr)
                        {
                            if (!isAdd && tempItem == editListView.SelectedItems[0]) continue;
                            MessageBox.Show("Find the same data in the list", "Stop", MessageBoxButtons.OK,
                                MessageBoxIcon.Stop);
                            return;
                        }

                if (isAdd)
                    editListView.Items.Add(tempItemStr);
                else
                    editListView.SelectedItems[0].Text = tempItemStr;
                Close();
            }
        }
    }
}