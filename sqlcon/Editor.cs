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
    class Editor : Window
    {
        private Grid grid = new Grid();
        private DataGrid dataGrid;
        private UniqueTable udt;

        public Editor(Configuration cfg, UniqueTable udt)
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

            var evenRowColor = GetColor(cfg, "gui.table.editor.AlternatingRowBackground", Colors.DimGray);
            var fkColor = GetColor(cfg, "gui.table.editor.Foreground", Colors.LightGray);
            var bkColor = GetColor(cfg, "gui.table.editor.RowBackground", Colors.Black);

            dataGrid = new DataGrid
            {
                AlternationCount = 2,
                AlternatingRowBackground = new SolidColorBrush(evenRowColor),
                Foreground = new SolidColorBrush(fkColor),
                RowBackground = new SolidColorBrush(bkColor)
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

        private Color GetColor(Configuration cfg, string key, Color defaultColor)
        {
            string colorString = cfg.GetValue<string>(key);

            if (colorString != null)
            {
                ColorConverter converter = new ColorConverter();

                if (converter.CanConvertFrom(typeof(string)))
                {
                    try
                    {
                        Color color = (Color)converter.ConvertFrom(null, null, colorString);
                        return color;
                    }
                    catch (Exception)
                    {
                        stdio.ErrorFormat("color setting {0} = {1} not supported", key, colorString);
                    }
                }
            }

            return defaultColor;
        }


        private SolidColorBrush ToBrush(string name, Color defaultColor)
        {
            Color color = ToColor(name, defaultColor);
            return new SolidColorBrush(color);
        }

        private static Color ToColor(string name, Color defaultColor)
        {
            ColorConverter converter = new ColorConverter();

            if (converter.CanConvertFrom(typeof(string)))
            {
                Color color = (Color)converter.ConvertFrom(null, null, name);
                return color;
            }
            else
                return defaultColor;
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
                stdio.ErrorFormat(ex.Message);
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
                stdio.ErrorFormat(ex.Message);
            }

        }
    }
}
