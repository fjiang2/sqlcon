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

namespace sqlcon
{
    public class Editor : Window
    {
        private Grid grid = new Grid();
        private DataGrid dataGrid;
        private UniqueTable udt;

        public Editor(UniqueTable udt)
        {
            this.udt = udt;
            if (udt.TableName != null)
            {
                this.Title = $"Table Editor: {udt.TableName}";
            }
            else
                this.Title = "Table Viewer";
            this.Width = 800;
            this.Height = 600;

            this.Content = grid;

            dataGrid = new DataGrid
            {
                AlternationCount = 2,
                AlternatingRowBackground = new SolidColorBrush(Colors.DimGray),
                Foreground = new SolidColorBrush(Colors.LightGray),
                RowBackground = new SolidColorBrush(Colors.Black)
            };

            if (udt.TableName != null)
                dataGrid.FrozenColumnCount = 1;
            else
                dataGrid.IsReadOnly = true;

            grid.Children.Add(dataGrid);
            //dataGrid.DataContext = dt.DefaultView;
            dataGrid.ItemsSource = udt.Table.DefaultView;


            dataGrid.AutoGeneratingColumn += DataGrid_AutoGeneratingColumn;
            udt.Table.RowChanged += Table_RowChanged;
            udt.Table.ColumnChanged += Table_ColumnChanged;
        }

        private void DataGrid_AutoGeneratingColumn(object sender, DataGridAutoGeneratingColumnEventArgs e)
        {
            if (e.Column.Header.ToString() == UniqueTable.ROWID)
            {
                // e.Cancel = true;   // For not to include 
                e.Column.IsReadOnly = true;
            }
        }


        private void Table_ColumnChanged(object sender, DataColumnChangeEventArgs e)
        {
            DataColumn column = e.Column;
            DataRow row = e.Row;
            udt.UpdateChanges(row, column, e.ProposedValue);
        }

        private void Table_RowChanged(object sender, DataRowChangeEventArgs e)
        {
            TableName tname = udt.TableName;
            if (tname == null)
                return;

            switch (e.Row.RowState)
            {
                case DataRowState.Added:
                    break;

                case DataRowState.Deleted:
                    break;

                case DataRowState.Modified:
                    break;
            }
        }
    }
}
