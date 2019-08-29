using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Data;
using Sys.Data;
using Sys.Stdio;

namespace sqlcon.Windows
{
    class TableEditor : Window
    {
        private Grid grid = new Grid();
        private DataGrid dataGrid;
        private UniqueTable udt;

        public TableEditor(UniqueTable udt)
        {
            this.udt = udt;
            if (udt.TableName != null)
            {
                this.Title = $"{udt.TableName} - sqlcon";
            }
            else
                this.Title = "View only - sqlcon";

            this.Width = 800;
            this.Height = 600;

            this.Content = grid;

            dataGrid = new DataGrid
            {
                AlternationCount = 2,
                AlternatingRowBackground = Themes.TableEditor.AlternatingRowBackground,
                Foreground = Themes.TableEditor.Foreground,
                RowBackground = Themes.TableEditor.RowBackground,
            };

            if (udt.TableName != null)
                dataGrid.FrozenColumnCount = 1;
            else
                dataGrid.IsReadOnly = true;

            grid.Children.Add(dataGrid);
            //dataGrid.DataContext = dt.DefaultView;
            dataGrid.ItemsSource = udt.Table.DefaultView;


            dataGrid.AutoGeneratingColumn += DataGrid_AutoGeneratingColumn;
            dataGrid.CellEditEnding += DataGrid_CellEditEnding;
            udt.Table.RowChanged += Table_RowChanged;
            udt.Table.ColumnChanged += Table_ColumnChanged;
        }

        private void DataGrid_CellEditEnding(object sender, DataGridCellEditEndingEventArgs e)
        {
            var col = e.Column;
            var row = e.Row;

        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            var style = new Style(typeof(System.Windows.Controls.Primitives.DataGridColumnHeader));
            if (e.Column.Header.ToString() == UniqueTable.ROWID)
            {
                // e.Cancel = true;   // For not to include 
                e.Column.IsReadOnly = true;

                style.Setters.Add(new Setter(ToolTipService.ToolTipProperty, "column rowid is read only"));
            }

            style.Setters.Add(new Setter
            {
                Property = ForegroundProperty,
                Value = Brushes.Black
            });

            e.Column.HeaderStyle = style;
        }

        private void RunWithoutTrigger(Action action)
        {
            udt.Table.RowChanged -= Table_RowChanged;
            udt.Table.ColumnChanged -= Table_ColumnChanged;

            action();

            udt.Table.RowChanged += Table_RowChanged;
            udt.Table.ColumnChanged += Table_ColumnChanged;
        }

        private void Table_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            DataColumn column = e.Column;
            DataRow row = e.Row;

            TableName tname = udt.TableName;
            if (tname.Provider.IsReadOnly)
                return;

            try
            {
                if (row.RowState != DataRowState.Detached)
                    udt.UpdateCell(row, column, e.ProposedValue);
            }
            catch (Exception ex)
            {
                cerr.WriteLine(ex.Message);
            }
        }

        private void Table_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            TableName tname = udt.TableName;
            if (tname == null)
                return;

            if (tname.Provider.IsReadOnly)
                return;

            try
            {
                switch (e.Row.RowState)
                {
                    case DataRowState.Added:
                        RunWithoutTrigger(() => udt.InsertRow(e.Row));
                        break;

                    case DataRowState.Deleted:
                        break;

                    case DataRowState.Modified:
                        break;
                }
            }
            catch (Exception ex)
            {
                cerr.WriteLine(ex.Message);
            }

        }
    }
}
