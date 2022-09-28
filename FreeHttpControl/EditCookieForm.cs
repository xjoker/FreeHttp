﻿using System;
using System.Windows.Forms;

namespace FreeHttp.FreeHttpControl
{
    public partial class EditCookieForm : Form
    {
        private readonly ListView editListView;

        public EditCookieForm(ListView yourEditListView)
        {
            InitializeComponent();
            editListView = yourEditListView;
            tb_name.Text = "name";
            rtb_value.Text = "vaule";
            tb_attribute.Text = "Path=/";
        }

        public EditCookieForm(ListView yourEditListView, string name, string vaule, string attribute)
            : this(yourEditListView)
        {
            if (name != null) tb_name.Text = name;
            if (vaule != null) rtb_value.Text = vaule;
            if (attribute != null) tb_attribute.Text = attribute;
        }

        private void EditCookieForm_Load(object sender, EventArgs e)
        {
            UpdataSetText();
            //this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            MaximumSize = Size;
            MinimumSize = Size;
        }


        private void tb_attribute_TextChanged(object sender, EventArgs e)
        {
            UpdataSetText();
        }

        private void bt_ok_Click(object sender, EventArgs e)
        {
            if (tb_attribute.Text.Contains("Domain=www.yourhost.com"))
            {
                MessageBox.Show(
                    "please change Domain=www.yourhost.com to your web host\r\nwww.yourhost.com is just a example",
                    "edit Domain");
                editListView.Tag = null;
                return;
            }

            editListView.Items.Add(rtb_setValue.Text);
            Close();
        }


        private void UpdataSetText()
        {
            if (tb_attribute.Text != "")
                rtb_setValue.Text = string.Format("Set-Cookie: {0}={1}; {2}", tb_name.Text, rtb_value.Text,
                    tb_attribute.Text);
            else
                rtb_setValue.Text = string.Format("Set-Cookie: {0}={1}", tb_name.Text, rtb_value.Text);
        }
    }
}