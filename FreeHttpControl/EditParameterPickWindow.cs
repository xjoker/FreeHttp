using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Forms;
using FreeHttp.AutoTest.ParameterizationPick;
using FreeHttp.FreeHttpControl.MyControl;

namespace FreeHttp.FreeHttpControl
{
    public partial class EditParameterPickWindow : Form
    {
        private readonly int parameterPickBoxHeight = 26;
        private readonly Point startLocation = new Point(8, 28);
        private readonly int startWindowHeight = 87;
        private readonly List<AddParameterPickBox> addParameterPickBoxList = new List<AddParameterPickBox>();

        private readonly Action<List<ParameterPick>> SetParameterPickAction;

        public EditParameterPickWindow()
        {
            InitializeComponent();
        }

        public EditParameterPickWindow(List<ParameterPick> parameterPicklist,
            Action<List<ParameterPick>> setParameterPickAction)
        {
            InitializeComponent();
            if (parameterPicklist != null && parameterPicklist.Count > 0)
                foreach (var parameterPick in parameterPicklist)
                    AddParameterPickBox(parameterPick);
            else
                AddParameterPickBox(null);
            SetParameterPickAction = setParameterPickAction;
        }

        private void EditParameterPickWindow_Load(object sender, EventArgs e)
        {
        }

        private void AddParameterPickBox(ParameterPick yourParameterPick)
        {
            AddParameterPickBox tempAddParameterPickBox;
            if (yourParameterPick == null)
            {
                tempAddParameterPickBox = new AddParameterPickBox();
            }
            else
            {
                tempAddParameterPickBox = new AddParameterPickBox(yourParameterPick);
                tempAddParameterPickBox.Tag = yourParameterPick;
            }

            tempAddParameterPickBox.OnAddParameterClick += AddParameterPickBox_OnAddParameterClick;
            AddAddParameterPickBox(tempAddParameterPickBox);
        }


        [MethodImpl(MethodImplOptions.Synchronized)]
        private void AddAddParameterPickBox(AddParameterPickBox yourAddParameterPickBox)
        {
            if (addParameterPickBoxList.Contains(yourAddParameterPickBox)) return;
            var nowParameterPickBoxCount = addParameterPickBoxList.Count;
            yourAddParameterPickBox.Location = new Point(startLocation.X,
                startLocation.Y + nowParameterPickBoxCount * parameterPickBoxHeight);
            Controls.Add(yourAddParameterPickBox);
            yourAddParameterPickBox.GetFocus();
            Height += parameterPickBoxHeight;
            addParameterPickBoxList.Add(yourAddParameterPickBox);
        }

        [MethodImpl(MethodImplOptions.Synchronized)]
        private void ResizeParameterPickBoxList()
        {
            if (addParameterPickBoxList != null)
            {
                for (var i = 0; i < addParameterPickBoxList.Count; i++)
                    addParameterPickBoxList[i].Location =
                        new Point(startLocation.X, startLocation.Y + parameterPickBoxHeight * i);
                Height = startWindowHeight + parameterPickBoxHeight * addParameterPickBoxList.Count;
            }
        }

        private void AddParameterPickBox_OnAddParameterClick(object sender, AddParameterPickBox.AddParameterEventArgs e)
        {
            if (e.IsAdd)
            {
                try
                {
                    var tempParameterPick = addParameterPickBoxList[addParameterPickBoxList.Count - 1]
                        .GetParameterPickInfo();
                    addParameterPickBoxList[addParameterPickBoxList.Count - 1].Tag = tempParameterPick;
                    AddParameterPickBox(null);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(string.Format("this parameter add infomation is illegal :{0}", ex.Message));
                }
            }
            else
            {
                if (addParameterPickBoxList.Contains(sender) || Controls.Contains((Control)sender))
                {
                    Controls.Remove((Control)sender);
                    addParameterPickBoxList.Remove((AddParameterPickBox)sender);
                    ResizeParameterPickBoxList();
                    if (addParameterPickBoxList.Count == 0) pb_add.Visible = true;
                }
                else
                {
                    MessageBox.Show("Fial to remove this item", "Stop", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
        }

        private void pb_add_Click(object sender, EventArgs e)
        {
            AddParameterPickBox(null);
            pb_add.Visible = false;
        }

        private void bt_ok_Click(object sender, EventArgs e)
        {
            var setParameterPickList = new List<ParameterPick>();
            foreach (var tempItem in addParameterPickBoxList)
                if (tempItem.Tag != null && tempItem.Tag is ParameterPick)
                    setParameterPickList.Add((ParameterPick)tempItem.Tag);
                else
                    try
                    {
                        var tempParameterPick = tempItem.GetParameterPickInfo();
                        tempItem.Tag = tempParameterPick;
                        setParameterPickList.Add(tempParameterPick);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show(string.Format("this parameter add infomation is illegal :{0}", ex.Message));
                        return;
                    }

            if (SetParameterPickAction != null) SetParameterPickAction(setParameterPickList);
            Close();
        }
    }
}