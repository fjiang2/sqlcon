using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Data;

namespace sqlcon
{
    public class Editor : Window
    {
        Grid grid = new Grid();
        DataGrid dataGrid;
        public Editor(DataTable dt)
        {
            this.Title = "Table Editor";
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

            grid.Children.Add(dataGrid);
            //dataGrid.DataContext = dt.DefaultView;
            dataGrid.ItemsSource = dt.DefaultView;

        }

    }
}
