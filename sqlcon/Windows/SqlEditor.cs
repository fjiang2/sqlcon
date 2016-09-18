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
using System.Windows.Controls.Primitives;
using System.IO;
using System.Data;
using Sys.Data;
using Sys.IO;

namespace sqlcon.Windows
{
    class SqlEditor : Window
    {
        private RichTextBox textBox;
        TabControl tabControl;
        private TextBlock lblCursorPosition;
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

            //Tool bar
            ToolBarTray tray = new ToolBarTray();
            tray.SetValue(DockPanel.DockProperty, Dock.Top);
            dockPanel.Children.Add(tray);

            ToolBar toolBar;
            tray.ToolBars.Add(toolBar = new ToolBar());
            toolBar.Items.Add(btnOpen = new Button { Command = ApplicationCommands.Open, Content = "Open", Width = 40, Margin = new Thickness(5) });
            toolBar.Items.Add(btnSave = new Button { Command = ApplicationCommands.Save, Content = "Save", Width = 40, Margin = new Thickness(5) });
            toolBar.Items.Add(btnExecute = new Button { Content = "Execute", Width = 50, Margin = new Thickness(5) });

            //editor and results
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5) });
            grid.RowDefinitions.Add(new RowDefinition());

            dockPanel.Children.Add(grid);
            var fkColor = cfg.GetColor("gui.sql.editor.Foreground", Colors.Black);
            var bkColor = cfg.GetColor("gui.sql.editor.Background", Colors.White);

            textBox = new RichTextBox
            {
                Foreground = new SolidColorBrush(fkColor),
                Background = new SolidColorBrush(bkColor)
            };

            GridSplitter splitter = new GridSplitter { Height = 5, HorizontalAlignment = HorizontalAlignment.Stretch };

            tabControl = new TabControl();
            tabControl.Items.Add(new TabItem { Header = "Messages" });

            textBox.SetValue(Grid.RowProperty, 0);
            splitter.SetValue(Grid.RowProperty, 1);
            tabControl.SetValue(Grid.RowProperty, 2);
            grid.Children.Add(textBox);
            grid.Children.Add(splitter);
            grid.Children.Add(tabControl);

            //status bar
            StatusBar statusBar = new StatusBar();
            statusBar.Items.Add(new StatusBarItem { Content = lblCursorPosition = new TextBlock { Height = 16, Width = 200 } });
            statusBar.SetValue(DockPanel.DockProperty, Dock.Bottom);
            dockPanel.Children.Add(statusBar);


            textBox.SelectionChanged += TextBox_SelectionChanged;
            textBox.Focus();
            btnExecute.Click += (sender, e) => Execute();
            btnSave.Click += (sender, e) => Save();
        }


       
        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            int row = textBox.LineNumber();
            int col = textBox.ColumnNumber();
            lblCursorPosition.Text = $"Line {row + 1}, Char {col + 1}";
        }
        

        private void Execute()
        {
            string text = textBox.GetAllText();

            var cmd = new SqlCmd(provider, text);
            cmd.ExecuteNonQuery();
            var ds = cmd.FillDataSet();

            tabControl.Items.Clear();
            foreach (DataTable dt in ds.Tables)
            {
                var tab = new TabItem { Header = "Result", Content = Display(dt) };
                tabControl.Items.Add(tab);
            }
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

            return;
        }
    }
}
