using System;
using System.Windows.Forms;

namespace FreeHttp.FreeHttpControl
{
    public partial class SortRule : Form
    {
        private readonly MyListView _listView;


        public SortRule(MyListView listView)
        {
            _listView = listView;
        }

        private void SortRule_Load(object sender, EventArgs e)
        {
            Controls.Add(_listView);
        }
    }
}