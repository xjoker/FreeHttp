﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using FreeHttp.AutoTest.ParameterizationPick;

namespace FreeHttp.FreeHttpControl.MyControl
{
    public partial class AddParameterPickBox : UserControl
    {
        public AddParameterPickBox()
        {
            InitializeComponent();
            cb_ParameterType.SelectedIndex = 0;
            cb_pickRange.SelectedIndex = 2;
        }

        public AddParameterPickBox(ParameterPick yourParameterPick)
        {
            InitializeComponent();
            tb_ParameterName.Text = yourParameterPick.ParameterName;
            tb_ParameterExpression.Text = yourParameterPick.PickTypeExpression;
            cb_ParameterType.Text = yourParameterPick.PickType.ToString();
            cb_ParameterTypeAddition.Text = yourParameterPick.PickTypeAdditional;
            cb_pickRange.Text = yourParameterPick.PickRange.ToString();
        }

        public event EventHandler<AddParameterEventArgs> OnAddParameterClick;

        private void cb_ParameterType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var tempParameterPickType = (ParameterPickType)Enum.Parse(typeof(ParameterPickType), cb_ParameterType.Text);
            if (ParameterPickTypeEngine.dictionaryParameterPickFunc[tempParameterPickType].Editable)
                cb_ParameterTypeAddition.DropDownStyle = ComboBoxStyle.DropDown;
            else
                cb_ParameterTypeAddition.DropDownStyle = ComboBoxStyle.DropDownList;
            cb_ParameterTypeAddition.DataSource = ParameterPickTypeEngine
                .dictionaryParameterPickFunc[tempParameterPickType].PickTypeAdditionalList;
            cb_ParameterTypeAddition.DisplayMember = "Key"; //可以把DisplayMember与ValueMember放到DataSource设置的前面
            cb_ParameterTypeAddition.ValueMember = "Value";
            cb_ParameterTypeAddition.SelectedIndex = 0;
            cb_ParameterTypeAddition_SelectedIndexChanged(null, null);
        }

        //设置 cb_ParameterTypeAddition.DataSource  会触发cb_ParameterTypeAddition_SelectedIndexChanged ，如果没有提前设置ValueMember，SelectedValue可能不是预期类型
        private void cb_ParameterTypeAddition_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cb_ParameterTypeAddition.SelectedValue.GetType() ==
                typeof(KeyValuePair<string, string>)) // as  只能用于引用类型比较 
                return;
            tb_ParameterExpression.WatermarkText = (string)cb_ParameterTypeAddition.SelectedValue;
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

        private void AddParameterPickBox_Validating(object sender, CancelEventArgs e)
        {
            //string errorMsg;
            //if (!ValidEmailAddress(textBox1.Text, out errorMsg))
            //{
            //    // Cancel the event and select the text to be corrected by the user.
            //    e.Cancel = true;
            //    textBox1.Select(0, textBox1.Text.Length);

            //    // Set the ErrorProvider error with the text to display. 
            //    this.errorProvider1.SetError(textBox1, errorMsg);
            //}
            //this.errorProvider_addParameter.Clear();
            try
            {
                Tag = GetParameterPickInfo();
            }
            catch (Exception)
            {
                Tag = null;
                e.Cancel = true;
            }
        }

        private void AddParameterPickBox_Validated(object sender, EventArgs e)
        {
            errorProvider_addParameter.Clear();
        }

        private void pb_add_Click(object sender, EventArgs e)
        {
            if (OnAddParameterClick != null) OnAddParameterClick(this, new AddParameterEventArgs(true));
        }

        private void pb_remove_Click(object sender, EventArgs e)
        {
            OnValidated(e);
            if (OnAddParameterClick != null) OnAddParameterClick(this, new AddParameterEventArgs(false));
        }

        public void GetFocus()
        {
            tb_ParameterName.Focus();
            tb_ParameterName.Select();
        }

        public ParameterPick GetParameterPickInfo()
        {
            Action<Control, string> MyThrowException = (myControl, errorMes) =>
            {
                FreeHttpWindow.MarkWarnControl(myControl);
                errorProvider_addParameter.SetIconAlignment(myControl, ErrorIconAlignment.MiddleRight);
                errorProvider_addParameter.SetIconPadding(myControl, -20);
                errorProvider_addParameter.SetError(myControl, errorMes);
                myControl.Select();
                myControl.Focus();
                throw new Exception(errorMes);
            };

            errorProvider_addParameter.Clear();
            var returnParameterPick = new ParameterPick();
            ParameterPickType tempParameterPickType;
            if (string.IsNullOrEmpty(tb_ParameterName.Text))
                MyThrowException(tb_ParameterName, "your ParameterName is empty");
            returnParameterPick.ParameterName = tb_ParameterName.Text;
            returnParameterPick.PickRange =
                (ParameterPickRange)Enum.Parse(typeof(ParameterPickRange), cb_pickRange.Text);
            if (!Enum.TryParse(cb_ParameterType.Text, out tempParameterPickType))
                MyThrowException(cb_ParameterType, "ParameterPickType Error");
            returnParameterPick.PickType = tempParameterPickType;
            returnParameterPick.PickTypeAdditional = cb_ParameterTypeAddition.Text;
            returnParameterPick.PickTypeExpression = tb_ParameterExpression.Text;
            var tempError = ParameterPickHelper.CheckParameterPickExpression(returnParameterPick);
            if (tempError != null)
            {
                if (tempError.Contains("PickTypeAdditional"))
                    MyThrowException(cb_ParameterTypeAddition, tempError);
                else
                    MyThrowException(tb_ParameterExpression, tempError);
            }

            return returnParameterPick;
        }

        public class AddParameterEventArgs : EventArgs
        {
            public AddParameterEventArgs(bool isAdd)
            {
                IsAdd = isAdd;
            }

            public bool IsAdd { get; set; }
        }
    }
}