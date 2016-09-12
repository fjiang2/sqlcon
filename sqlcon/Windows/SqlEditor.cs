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
using System.Windows.Documents;
using System.Data;
using Sys.Data;
using Sys.IO;

namespace sqlcon
{
    class SqlEditor : Window
    {
        private RichTextBox textBox;
        private Button btnExecute;
        private Button btnOpen;
        private Button btnSave;

        private Configuration cfg;
        private FileLink link;
        private ConnectionProvider provider;
        public SqlEditor(Configuration cfg, ConnectionProvider provider, FileLink link)
        {
            InitializeComponent(cfg);

            this.cfg = cfg;
            this.provider = provider;
            this.link = link;
            string text = string.Empty;

            if (link != null)
            {
                this.Title = $"Sql Script Editor: {link}";
                text = link.ReadAllText();
            }
            else
            {
                this.Title = "Sql Script Editor";
            }

            textBox.Document.Blocks.Add(new Paragraph(new Run(text)));
        }

        private void InitializeComponent(Configuration cfg)
        {
            this.Width = 800;
            this.Height = 600;

            DockPanel dockPanel = new DockPanel();
            this.Content = dockPanel;

            ToolBarTray tray = new ToolBarTray();
            dockPanel.Children.Add(tray);
            tray.SetValue(DockPanel.DockProperty, Dock.Top);

            ToolBar toolBar;
            tray.ToolBars.Add(toolBar = new ToolBar());
            toolBar.Items.Add(btnOpen = new Button { Command = ApplicationCommands.Open, Content = "Open", Width = 40, Margin = new Thickness(5) });
            toolBar.Items.Add(btnSave = new Button { Command = ApplicationCommands.Save, Content = "Save", Width = 40, Margin = new Thickness(5) });
            toolBar.Items.Add(btnExecute = new Button { Content = "Execute", Width = 50, Margin = new Thickness(5) });


            TabControl tabControl = new TabControl { Height = 100 };
            dockPanel.Children.Add(tabControl);
            tabControl.SetValue(DockPanel.DockProperty, Dock.Bottom);
            tabControl.Items.Add(new TabItem { Header = "Results" });
            tabControl.Items.Add(new TabItem { Header = "Messages" });


            Grid grid = new Grid();
            dockPanel.Children.Add(grid);
            var fkColor = cfg.GetColor("gui.sql.editor.Foreground", Colors.LightGray);
            var bkColor = cfg.GetColor("gui.sql.editor.Background", Colors.Black);

            textBox = new RichTextBox
            {
                Foreground = new SolidColorBrush(fkColor),
                Background = new SolidColorBrush(bkColor)
            };

            grid.Children.Add(textBox);
        }

        


        private string GetAllText()
        {
            TextRange textRange = new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd);
            return textRange.Text;
        }

        private void Execute()
        {
            var cmd = new SqlCmd(provider, GetAllText());

            cmd.ExecuteNonQuery();
            var ds = cmd.FillDataSet();

        }

        private DataGrid Display(DataTable table)
        {
            var evenRowColor = cfg.GetColor("gui.table.editor.AlternatingRowBackground", Colors.DimGray);
            var fkColor = cfg.GetColor("gui.table.editor.Foreground", Colors.LightGray);
            var bkColor = cfg.GetColor("gui.table.editor.RowBackground", Colors.Black);

            var dataGrid = new DataGrid
            {
                AlternationCount = 2,
                AlternatingRowBackground = new SolidColorBrush(evenRowColor),
                Foreground = new SolidColorBrush(fkColor),
                RowBackground = new SolidColorBrush(bkColor)
            };

            dataGrid.IsReadOnly = true;
            dataGrid.ItemsSource = table.DefaultView;

            return dataGrid;
        }
    }
}
