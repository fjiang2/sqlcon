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
using System.IO;
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
            string text = string.Empty;

            if (link != null)
            {
                this.link = link;
                text = link.ReadAllText();
                textBox.Document.Blocks.Add(new Paragraph(new Run(text)));
            }
            else
            {
                this.link = FileLink.CreateLink("untitled.sql", null, null);
            }

            this.Title = $"{this.link} - sqlcon";

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
            var fkColor = cfg.GetColor("gui.sql.editor.Foreground", Colors.Black);
            var bkColor = cfg.GetColor("gui.sql.editor.Background", Colors.White);

            textBox = new RichTextBox
            {
                Foreground = new SolidColorBrush(fkColor),
                Background = new SolidColorBrush(bkColor)
            };

            grid.Children.Add(textBox);

            textBox.Focus();
            btnExecute.Click += (sender, e) => Execute();
            btnSave.Click += (sender, e) => Save();
        }

        private string GetAllText()
        {
            if (string.IsNullOrEmpty(textBox.Selection.Text))
            {
                TextRange textRange = new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd);
                return textRange.Text;
            }
            else
                return textBox.Selection.Text;
        }

        private void Execute()
        {
            string text = GetAllText();

            var cmd = new SqlCmd(provider, text);
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

        public void Save()
        {
            var saveFile = new Microsoft.Win32.SaveFileDialog();
            saveFile.Filter = "Sql Script Files (*.sql)|*.sql|Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            if (saveFile.ShowDialog(this) == true)
            {
                TextRange documentTextRange = new TextRange(textBox.Document.ContentStart, textBox.Document.ContentEnd);

                // If this file exists, it's overwritten.
                using (FileStream fs = File.Create(saveFile.FileName))
                {
                    if (Path.GetExtension(saveFile.FileName).ToLower() == ".rtf")
                    {
                        documentTextRange.Save(fs, DataFormats.Rtf);
                    }
                    else
                    {
                        documentTextRange.Save(fs, DataFormats.Text);
                    }
                }
            }
        }
    }
}
