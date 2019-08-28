using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.Windows.Markup;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Data;

namespace sqlcon.Windows
{
    class ScriptResultPane : Grid
    {
        private TextBlock lblRowCount = new TextBlock { Width = 200, HorizontalAlignment = HorizontalAlignment.Right };
        private ScriptResultControl Tabs { get; }
        public TabControl TabControl { get; } = new TabControl();
        public RichTextBox TextBox { get; } = new RichTextBox
        {
            FontFamily = new FontFamily("Consolas"),
            FontSize = 12,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        public bool IsDirty { get; set; }

        public ScriptResultPane(ScriptResultControl parent)
        {
            this.Tabs = parent;
            InitializeComponent();

            TabControl.SelectionChanged += TabControl_SelectionChanged;
            TextBox.SelectionChanged += TextBox_SelectionChanged;
            TextBox.TextChanged += TextBox_TextChanged;
            TextBox.Focus();
        }


        private void InitializeComponent()
        {
            Grid grid = this;
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5) });
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5) });


            //TextBox.Foreground = cfg.GetSolidBrush(ConfigKey._GUI_SQL_EDITOR_FOREGROUND, Colors.Black);
            //TextBox.Background = cfg.GetSolidBrush(ConfigKey._GUI_SQL_EDITOR_BACKGROUND, Colors.White);

            //Paragraph space
            Style style = new Style { TargetType = typeof(Paragraph) };
            style.Setters.Add(new Setter { Property = Block.MarginProperty, Value = new Thickness(0) });
            TextBox.Resources.Add(typeof(Paragraph), style);

            GridSplitter hSplitter = new GridSplitter { Height = 5, HorizontalAlignment = HorizontalAlignment.Stretch };
            //tabControl.Foreground = cfg.GetSolidBrush(ConfigKey._GUI_SQL_EDITOR_FOREGROUND, Colors.Black);
            //tabControl.Background = cfg.GetSolidBrush(ConfigKey._GUI_SQL_EDITOR_BACKGROUND, Colors.White);

            StatusBar statusBar = new StatusBar { Height = 20 };
            statusBar.Items.Add(new StatusBarItem { Content = lblRowCount, HorizontalAlignment = HorizontalAlignment.Right });

            TextBox.SetValue(Grid.RowProperty, 0);
            hSplitter.SetValue(Grid.RowProperty, 1);
            TabControl.SetValue(Grid.RowProperty, 2);
            statusBar.SetValue(Grid.RowProperty, 3);

            grid.Children.Add(TextBox);
            grid.Children.Add(hSplitter);
            grid.Children.Add(TabControl);
            grid.Children.Add(statusBar);

        }

        public void ShowText(string text)
        {
            TextBox.Document.Blocks.Clear();
            TextBox.Document.Blocks.Add(new Paragraph(new Run(text)));
            IsDirty = false;
        }

        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            int row = TextBox.LineNumber();
            int col = TextBox.ColumnNumber();
            Tabs.Editor.ShowCursorPosition(row, col);
        }

        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            IsDirty = true;
        }


        //display #of rows
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem tab = (TabControl.SelectedItem as TabItem);

            if (tab == null) return;

            DataGrid grid = tab.Content as DataGrid;
            if (grid == null)
            {
                lblRowCount.Text = "";
                return;
            }

            var view = grid.ItemsSource as DataView;
            if (view == null) return;

            lblRowCount.Text = $"{view.Table.Rows.Count} row(s)";
        }

    }
}
