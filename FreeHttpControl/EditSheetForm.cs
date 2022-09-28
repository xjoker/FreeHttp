﻿using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Windows.Forms;
using FreeHttp.AutoTest;
using FreeHttp.AutoTest.MyCommonHelper.FileHelper;

namespace FreeHttp.FreeHttpControl
{
    public partial class EditSheetForm : Form
    {
        private readonly List<List<string>> listViewSource;
        private Point selectCell;
        private DataTable sourceDataTable;

        public EditSheetForm()
        {
            InitializeComponent();
        }

        public EditSheetForm(string name, List<List<string>> dataSource, Point yourSelectCell)
            : this()
        {
            Text = string.IsNullOrEmpty(name) ? "" : name;
            if (dataSource != null)
            {
                listViewSource = dataSource;
                selectCell = yourSelectCell;
            }
        }

        public event EventHandler<SaveSheetDataEventArgs> SaveSheetData;

        private void EditSheetForm_Load(object sender, EventArgs e)
        {
            dataGridView.DataSource = FillData(listViewSource);
            for (var i = 0; i < dataGridView.Columns.Count; i++)
                dataGridView.Columns[i].SortMode = DataGridViewColumnSortMode.NotSortable;
            if (selectCell.X < dataGridView.ColumnCount && selectCell.Y < dataGridView.RowCount)
                //dataGridView.Rows[0].Cells[0].Selected = false;
                //dataGridView.Rows[0].Cells[0].Selected = true;
                dataGridView.CurrentCell = dataGridView.Rows[selectCell.Y].Cells[selectCell.X];
        }

        private void dataGridView_RowsAdded(object sender, DataGridViewRowsAddedEventArgs e)
        {
            for (var i = e.RowIndex; i < dataGridView.Rows.Count; i++)
            {
                dataGridView.Rows[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                dataGridView.Rows[i].HeaderCell.Value = i.ToString();
            }
        }

        private void dataGridView_RowsRemoved(object sender, DataGridViewRowsRemovedEventArgs e)
        {
            for (var i = e.RowIndex; i < dataGridView.Rows.Count; i++)
            {
                dataGridView.Rows[i].HeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleRight;
                dataGridView.Rows[i].HeaderCell.Value = i.ToString();
            }
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

        private void pb_saveSheet_Click(object sender, EventArgs e)
        {
            if (SaveSheetData == null) return;
            var tempViewSource = GetData();
            this.SaveSheetData(this,
                new SaveSheetDataEventArgs(tempViewSource,
                    dataGridView.CurrentCell == null
                        ? null
                        : new Point?(new Point(dataGridView.CurrentCell.ColumnIndex,
                            dataGridView.CurrentCell.RowIndex))));
        }

        private void EditSheetForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            pb_saveSheet_Click(this, null);
        }

        private void pb_export_Click(object sender, EventArgs e)
        {
            //saveFileDialog
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                var tempViewSource = GetData();
                try
                {
                    CsvFileHelper.SaveCsvFile(saveFileDialog.FileName, tempViewSource);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Stop", MessageBoxButtons.OK, MessageBoxIcon.Stop);
                }
            }
        }

        private DataTable FillData(List<List<string>> sourceData)
        {
            sourceDataTable = new DataTable();
            var columnCount = 0;
            if (sourceData != null)
            {
                sourceDataTable.BeginLoadData();
                foreach (var tempRowItem in sourceData)
                    if (tempRowItem != null && tempRowItem.Count > 0)
                    {
                        if (tempRowItem.Count > columnCount)
                        {
                            for (var i = columnCount; i < tempRowItem.Count; i++)
                                sourceDataTable.Columns.Add(i.ToString(), typeof(string));
                            columnCount = tempRowItem.Count;
                        }

                        sourceDataTable.Rows.Add(tempRowItem.ToArray());
                    }
                    else
                    {
                        sourceDataTable.Rows.Add(sourceDataTable.NewRow());
                    }

                sourceDataTable.EndLoadData();
            }

            return sourceDataTable;
        }

        private List<List<string>> GetData()
        {
            var outData = new List<List<string>>();
            if (sourceDataTable != null)
            {
                if (dataGridView.IsCurrentCellDirty)
                {
                    dataGridView.CommitEdit(DataGridViewDataErrorContexts.Commit);
                    dataGridView.BindingContext[dataGridView.DataSource].EndCurrentEdit();
                }

                dataGridView.EndEdit();
                foreach (DataRow tempRow in sourceDataTable.Rows) outData.Add(tempRow.ItemArray);
            }

            return outData;
        }

        private bool IsDataSourceEqual(List<List<string>> yourDataSource)
        {
            if (listViewSource == yourDataSource) return true;
            if (listViewSource == null || yourDataSource == null) return false;
            if (listViewSource.Count != yourDataSource.Count) return false;
            for (var i = 0; i < listViewSource.Count; i++)
            {
                if (listViewSource[i].Count != yourDataSource[i].Count) return false;
                for (var j = 0; j < listViewSource[i].Count; j++)
                    if (listViewSource[i][j] != yourDataSource[i][j])
                        return false;
            }

            return true;
        }

        public class SaveSheetDataEventArgs : EventArgs
        {
            public SaveSheetDataEventArgs(List<List<string>> sheetData, Point? selectCell)
            {
                SheetData = sheetData;
                SelectCell = selectCell;
            }

            public List<List<string>> SheetData { get; set; }
            public Point? SelectCell { get; set; }
        }
    }
}