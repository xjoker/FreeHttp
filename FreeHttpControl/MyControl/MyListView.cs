using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Windows.Forms;

namespace FreeHttp.FreeHttpControl
{
    /// <summary>
    ///     item 可拖放排序的ListView
    /// </summary>
    //public class MyListView : ListView
    public class MyListView : ListViewExtended
    {
        private const int WM_LBUTTONDBLCLK = 0x0203; //左键双击
        private int moveItemIndex = -1; //当前正在被移动的项
        private int scrollDecelerateFlag = 25; //自动滚动缓速标识

        /// <summary>
        ///     this ListView disable double click to check the checkbox
        ///     enable DoubleBuffer
        ///     implement items drag in detail mode
        /// </summary>
        public MyListView()
        {
            InitializeComponent();
            SetStyle(
                ControlStyles.DoubleBuffer | ControlStyles.OptimizedDoubleBuffer | ControlStyles.AllPaintingInWmPaint,
                true);
            UpdateStyles();
            GroupSelectedSataus = new GroupSelectedItemsSataus(this);
        }

        public GroupSelectedItemsSataus GroupSelectedSataus { get; }

        /// <summary>
        ///     Drag Start (开始拖放/拖入)
        /// </summary>
        public event EventHandler<ItemDragEventArgs> OnItemDragSortStart;

        /// <summary>
        ///     Drag End （完成拖放/脱出）
        /// </summary>
        public event EventHandler<DragEventArgs> OnItemDragSortEnd;

        protected override void WndProc(ref Message m)
        {
            if (m.Msg == WM_LBUTTONDBLCLK)
            {
                var p = PointToClient(new Point(Cursor.Position.X, Cursor.Position.Y));
                var lvi = GetItemAt(p.X, p.Y);
                if (lvi != null)
                    lvi.Selected = true;
                OnDoubleClick(new EventArgs());
            }
            else
            {
                base.WndProc(ref m);
            }
        }

        private void InitializeComponent()
        {
            SuspendLayout();
            // 
            // MyListView
            // 
            ItemDrag += MyListView_ItemDrag;
            DragDrop += MyListView_DragDrop;
            DragEnter += MyListView_DragEnter;
            DragOver += MyListView_DragOver;
            DragLeave += MyListView_DragLeave;
            ResumeLayout(false);
        }

        /// <summary>
        ///     is your item above your move items (just like AppearsAfterItem)   [if you want enable drag just set ListView
        ///     AllowDrop is true]
        /// </summary>
        /// <param name="nowIndex">you now item index</param>
        /// <returns>is above item</returns>
        private bool AppearAboveItem(int nowIndex)
        {
            if (nowIndex < moveItemIndex) return true;
            return false;
        }

        /// <summary>
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyListView_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (SelectedItems != null && SelectedItems.Count > 0)
            {
                GroupSelectedSataus.GetSnapshoot();
                OnItemDragSortStart?.Invoke(sender, e);
                GroupSelectedSataus.ReCoverSnapshoot();
                moveItemIndex = SelectedItems[0].Index;
                DoDragDrop(SelectedItems, DragDropEffects.Move);
            }
        }

        /// <summary>
        ///     drag complete
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyListView_DragDrop(object sender, DragEventArgs e)
        {
            var targetIndex = InsertionMark.Index;
            if (targetIndex == -1) return;
            var draggedItems = (SelectedListViewItemCollection)e.Data.GetData(typeof(SelectedListViewItemCollection));
            if (draggedItems == null || draggedItems.Count == 0 || draggedItems[0].ListView != this)
            {
                InsertionMark.Index = -1;
                return;
            }

            foreach (ListViewItem draggedItem in draggedItems)
            {
                Items.Remove(draggedItem);
                Items.Insert(targetIndex, draggedItem);
                if (AppearAboveItem(targetIndex)) targetIndex++;
            }

            OnItemDragSortEnd?.Invoke(sender, e);
        }


        private void MyListView_DragEnter(object sender, DragEventArgs e)
        {
            GroupSelectedSataus.GetSnapshoot();
            OnItemDragSortStart?.Invoke(sender, null);
            GroupSelectedSataus.ReCoverSnapshoot();
            var draggedItems = (SelectedListViewItemCollection)e.Data.GetData(typeof(SelectedListViewItemCollection));
            e.Effect = draggedItems == null || draggedItems.Count == 0 || draggedItems[0].ListView != this
                ? DragDropEffects.None
                : e.AllowedEffect;
        }

        private void MyListView_DragLeave(object sender, EventArgs e)
        {
            InsertionMark.Index = -1;
            OnItemDragSortEnd?.Invoke(sender, null);
        }

        /// <summary>
        ///     drag over the contor boundary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MyListView_DragOver(object sender, DragEventArgs e)
        {
            Debug.WriteLine(
                $"--------------------------{DateTime.Now.Millisecond}[{e.X},{e.Y}]MyListView_DragOver--------------------");
            var targetPoint = PointToClient(new Point(e.X, e.Y));
            Debug.WriteLine(
                $"--------------------------[{targetPoint.X},{targetPoint.Y},{Height}]--------------------");

            var targetIndex = InsertionMark.NearestIndex(targetPoint);
            Debug.WriteLine($"--------------------------[{targetIndex}]--------------------");

            //System.Diagnostics.Debug.WriteLine(targetIndex.ToString() + this.InsertionMark.AppearsAfterItem.ToString());
            if (targetIndex > -1) InsertionMark.Color = Color.PowderBlue;
            //Rectangle itemBounds = myListView.GetItemRect(targetIndex);
            //myListView.InsertionMark.AppearsAfterItem = (targetPoint.X > itemBounds.Left + (itemBounds.Width / 2));
            InsertionMark.AppearsAfterItem = !AppearAboveItem(targetIndex);
            InsertionMark.Index = targetIndex;


            //自动滚动
            if (targetIndex == -1) targetIndex = moveItemIndex;
            if (targetPoint.Y < 30 && targetIndex > 0)
            {
                if (scrollDecelerateFlag % 5 == 0)
                    EnsureVisible(targetIndex - 1);
                if (scrollDecelerateFlag > 0)
                    scrollDecelerateFlag--;
            }
            else if (targetPoint.Y > Height - 15 && targetIndex < Items.Count - 1)
            {
                if (scrollDecelerateFlag % 5 == 0)
                    EnsureVisible(targetIndex + 1);
                if (scrollDecelerateFlag > 0)
                    scrollDecelerateFlag--;
            }
            else
            {
                scrollDecelerateFlag = 25;
            }
        }

        /// <summary>
        ///     维持group模式下的选中状态（在ListView动态编辑group过程中，会让选择状态异常）
        /// </summary>
        public class GroupSelectedItemsSataus
        {
            public GroupSelectedItemsSataus(ListView listView)
            {
                NowListView = listView;
                NowSelectedItems = new List<ListViewItem>();
            }

            private ListView NowListView { get; }
            private List<ListViewItem> NowSelectedItems { get; }

            public void GetSnapshoot()
            {
                NowSelectedItems.Clear();
                foreach (ListViewItem listViewItem in NowListView.SelectedItems) NowSelectedItems.Add(listViewItem);
            }

            public void ReCoverSnapshoot()
            {
                if (NowSelectedItems.Count > 0)
                {
                    foreach (ListViewItem listViewItem in NowListView.SelectedItems)
                        if (NowSelectedItems.Contains(listViewItem))
                            NowSelectedItems.Remove(listViewItem);
                        else
                            listViewItem.Selected = false;
                    foreach (var listViewItem in NowSelectedItems) listViewItem.Selected = true;
                }

                NowSelectedItems.Clear();
            }
        }
    }
}