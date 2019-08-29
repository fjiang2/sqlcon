using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Input;
using System.Windows.Controls.Primitives;
using System.Data;

namespace sqlcon.Windows
{
    static class WpfUtils
    {
        public static Button NewImageButton(ICommand command, string text, string toolTip, string image)
        {
            StackPanel stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
            stackPanel.Children.Add(NewImage(image));
            stackPanel.Children.Add(new TextBlock { Text = text });

            return new Button
            {
                Command = command,
                Content = stackPanel,
                Margin = new Thickness(5),
                ToolTip = toolTip
            };
        }

        public static BitmapImage NewBitmapImage(string image)
        {
            //make sure: build action on image to Resource
            string pack = "pack://application:,,,/sqlcon;component/Windows/images";
            return new BitmapImage(new Uri($"{pack}/{image}", UriKind.Absolute));
        }

        public static Image NewImage(string image)
        {
            return new Image { Source = NewBitmapImage(image) };
        }

        public static StackPanel NewImageLabel(string text, string image)
        {
            StackPanel stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
            Image img = NewImage(image);
            img.Width = 12;
            img.Height = 12;
            stackPanel.Children.Add(img);
            stackPanel.Children.Add(new TextBlock { Text = text, Padding = new Thickness(2,0,2,0) });

            return stackPanel;
        }

        public static StackPanel NewLabelImage(string text, string image)
        {
            StackPanel stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
            Image img = NewImage(image);
            img.Width = 12;
            img.Height = 12;
            stackPanel.Children.Add(new TextBlock { Text = text, Padding = new Thickness(2, 0, 2, 0) });
            stackPanel.Children.Add(img);

            return stackPanel;
        }

        public static DataGrid CreateDataGrid(this DataTable table, Configuration cfg)
        {
            var fkColor = cfg.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_TABLE_FOREGROUND, Colors.White);
            var bkColor = cfg.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_TABLE_BACKGROUND, Colors.Black);
            var evenRowColor = cfg.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_TABLE_ALTERNATINGROWBACKGROUND, Colors.DimGray);
            var oddRowColor = cfg.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_TABLE_ROWBACKGROUND, Colors.Black);

            var dataGrid = new DataGrid
            {
                Foreground = fkColor,
                AlternationCount = 2,
                AlternatingRowBackground = evenRowColor,
                RowBackground = oddRowColor
            };

            var style = new Style(typeof(DataGridColumnHeader));
            style.Setters.Add(new Setter { Property = Control.ForegroundProperty, Value = fkColor });
            style.Setters.Add(new Setter { Property = Control.BackgroundProperty, Value = bkColor });
            style.Setters.Add(new Setter { Property = Control.PaddingProperty, Value = new Thickness(2, 0, 2, 0) });
            dataGrid.ColumnHeaderStyle = style;

            style = new Style(typeof(DataGridRowHeader));
            style.Setters.Add(new Setter { Property = Control.ForegroundProperty, Value = fkColor });
            style.Setters.Add(new Setter { Property = Control.BackgroundProperty, Value = bkColor });
            style.Setters.Add(new Setter { Property = Control.PaddingProperty, Value = new Thickness(2, 0, 2, 0) });
            dataGrid.RowHeaderStyle = style;

            dataGrid.RowHeaderWidth = 40;
            dataGrid.IsReadOnly = true;

            dataGrid.ItemsSource = table.DefaultView;
            //dataGrid.Loaded += DataGrid_Loaded;
            dataGrid.LoadingRow += DataGrid_LoadingRow;
            return dataGrid;
        }

        private static void DataGrid_LoadingRow(object sender, DataGridRowEventArgs e)
        {
            // add line number on the grid
            e.Row.Header = (e.Row.GetIndex() + 1).ToString();
        }

        private static void DataGrid_Loaded(object sender, RoutedEventArgs e)
        {
            DataGrid dataGrid = (DataGrid)sender;
            DataGridTextColumn column = new DataGridTextColumn
            {
                Header = "",
            };

            dataGrid.Columns.Insert(0, column);
        }
    }
}
