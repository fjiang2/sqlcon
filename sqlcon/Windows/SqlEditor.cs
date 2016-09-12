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

        private FileLink link;
        private ConnectionProvider provider;
        public SqlEditor(Configuration cfg, ConnectionProvider provider, FileLink link)
        {
            InitializeComponent(cfg);

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
            tabControl.Items.Add(new TabItem { Header = "Output" });
            tabControl.Items.Add(new TabItem { Header = "Table" });


            Grid grid = new Grid();
            dockPanel.Children.Add(grid);
            var fkColor = GetColor(cfg, "gui.sql.editor.Foreground", Colors.LightGray);
            var bkColor = GetColor(cfg, "gui.sql.editor.Background", Colors.Black);

            textBox = new RichTextBox
            {
                Foreground = new SolidColorBrush(fkColor),
                Background = new SolidColorBrush(bkColor)
            };

            grid.Children.Add(textBox);
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


        private string GetAllText()
        {
            TextRange textRange = new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd);
            return textRange.Text;
        }

        private void Execute()
        {
            new SqlCmd(provider, GetAllText()).ExecuteNonQuery();
        }
    }
}
