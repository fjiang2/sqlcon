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
using System.Windows.Markup;
using System.Windows.Documents;
using System.Windows.Controls.Primitives;
using System.IO;
using System.Data;
using System.Data.SqlClient;

using Sys;
using Sys.Data;
using Sys.Data.IO;
using System.ComponentModel;
namespace sqlcon
{
    class ScriptResultPane : Grid
    {
        public TabControl TabControl { get; } = new TabControl();
        public RichTextBox TextBox { get; } = new RichTextBox
        {
            FontFamily = new FontFamily("Consolas"),
            FontSize = 12,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        public ScriptResultPane()
        {
            InitializeComponent();
        }


        private void InitializeComponent()
        {
            Grid grid = this;
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5) });
            grid.RowDefinitions.Add(new RowDefinition());


            //TextBox.Foreground = cfg.GetSolidBrush(ConfigKey._GUI_SQL_EDITOR_FOREGROUND, Colors.Black);
            //TextBox.Background = cfg.GetSolidBrush(ConfigKey._GUI_SQL_EDITOR_BACKGROUND, Colors.White);

            //Paragraph space
            Style style = new Style { TargetType = typeof(Paragraph) };
            style.Setters.Add(new Setter { Property = Block.MarginProperty, Value = new Thickness(0) });
            TextBox.Resources.Add(typeof(Paragraph), style);

            GridSplitter hSplitter = new GridSplitter { Height = 5, HorizontalAlignment = HorizontalAlignment.Stretch };
            //tabControl.Foreground = cfg.GetSolidBrush(ConfigKey._GUI_SQL_EDITOR_FOREGROUND, Colors.Black);
            //tabControl.Background = cfg.GetSolidBrush(ConfigKey._GUI_SQL_EDITOR_BACKGROUND, Colors.White);

            TextBox.SetValue(Grid.RowProperty, 0);
            hSplitter.SetValue(Grid.RowProperty, 1);
            TabControl.SetValue(Grid.RowProperty, 2);
            grid.Children.Add(TextBox);
            grid.Children.Add(hSplitter);
            grid.Children.Add(TabControl);
        }
    }
}
