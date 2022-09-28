using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace FreeHttp.FreeHttpControl
{
    public partial class ShowSheetForm : Form
    {
        private readonly List<List<string>> listViewSource;

        public ShowSheetForm()
        {
            InitializeComponent();
        }

        public ShowSheetForm(string name, List<List<string>> dataSource)
            : this()
        {
            Text = string.IsNullOrEmpty(name) ? "" : name;
            if (dataSource != null) listViewSource = dataSource;
        }

        private void ShowSheetForm_Load(object sender, EventArgs e)
        {
            var columnCount = 0;
            if (listViewSource != null)
                foreach (var tempRowItem in listViewSource)
                    if (tempRowItem != null && tempRowItem.Count > 0)
                    {
                        if (tempRowItem.Count > columnCount)
                        {
                            for (var i = columnCount; i < tempRowItem.Count; i++)
                                listView.Columns.Add((i + 1).ToString());
                            columnCount = tempRowItem.Count;
                        }

                        listView.Items.Add(new ListViewItem(tempRowItem.ToArray()));
                    }
                    else
                    {
                        listView.Items.Add(new ListViewItem());
                    }
        }
    }
}