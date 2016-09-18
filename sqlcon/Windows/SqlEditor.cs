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
        private Configuration cfg;
        private FileLink link;
        private ConnectionProvider provider;

        public SqlEditor(Configuration cfg, ConnectionProvider provider, FileLink link)
        {
            InitializeComponent(cfg);

            this.cfg = cfg;
            this.provider = provider;

            textBox.SelectionChanged += TextBox_SelectionChanged;
            textBox.Focus();

            btnExecute.Click += (sender, e) => Execute();
            btnSave.Click += (sender, e) => Save();


            if (link != null)
            {
                this.link = link;
                string text = link.ReadAllText();
                textBox.Document.Blocks.Clear();
                textBox.Document.Blocks.Add(new Paragraph(new Run(text)));
            }
            else
            {
                this.link = FileLink.CreateLink("untitled.sql", null, null);
            }

            this.Title = $"{this.link} - sqlcon";

        }


        private RichTextBox textBox = new RichTextBox { FontFamily = new FontFamily("Consolas"), FontSize = 12 };
        private TabControl tabControl = new TabControl();
        private TextBlock lblCursorPosition = new TextBlock { Width = 200 };

        private Button btnOpen = new Button { Command = ApplicationCommands.Open, Content = "Open", Width = 40, Margin = new Thickness(5) };
        private Button btnSave = new Button { Command = ApplicationCommands.Save, Content = "Save", Width = 40, Margin = new Thickness(5) };
        private Button btnExecute = new Button { Content = "Execute", Width = 50, Margin = new Thickness(5) };

        private void InitializeComponent(Configuration cfg)
        {
            this.Width = 1024;
            this.Height = 768;

            DockPanel dockPanel = new DockPanel();
            this.Content = dockPanel;

            //Tool bar
            ToolBarTray tray = new ToolBarTray();
            tray.SetValue(DockPanel.DockProperty, Dock.Top);
            dockPanel.Children.Add(tray);

            ToolBar toolBar;
            tray.ToolBars.Add(toolBar = new ToolBar());
            toolBar.Items.Add(btnOpen);
            toolBar.Items.Add(btnSave);
            toolBar.Items.Add(btnExecute);

            //status bar
            StatusBar statusBar = new StatusBar { Height = 20 };
            statusBar.Items.Add(new StatusBarItem { Content = lblCursorPosition });
            statusBar.SetValue(DockPanel.DockProperty, Dock.Bottom);
            dockPanel.Children.Add(statusBar);


            #region editor and results
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5) });
            grid.RowDefinitions.Add(new RowDefinition());

            dockPanel.Children.Add(grid);
            var fkColor = cfg.GetColor("gui.sql.editor.Foreground", Colors.Black);
            var bkColor = cfg.GetColor("gui.sql.editor.Background", Colors.White);
            textBox.Foreground = new SolidColorBrush(fkColor);
            textBox.Background = new SolidColorBrush(bkColor);

            //Paragraph space
            Style style = new Style { TargetType = typeof(Paragraph) };
            style.Setters.Add(new Setter { Property = FrameworkElement.MarginProperty, Value = new Thickness(0) });
            textBox.Resources.Add("line-space", style);

            GridSplitter splitter = new GridSplitter { Height = 5, HorizontalAlignment = HorizontalAlignment.Stretch };

            textBox.SetValue(Grid.RowProperty, 0);
            splitter.SetValue(Grid.RowProperty, 1);
            tabControl.SetValue(Grid.RowProperty, 2);
            grid.Children.Add(textBox);
            grid.Children.Add(splitter);
            grid.Children.Add(tabControl);

            #endregion
        }



        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            int row = textBox.LineNumber();
            int col = textBox.ColumnNumber();
            lblCursorPosition.Text = $"Ln {row}, Col {col}";
        }


        private void Execute()
        {
            tabControl.Items.Clear();

            string text = textBox.GetAllText();

            var cmd = new SqlCmd(provider, text);
            if (text.IndexOf("select", StringComparison.CurrentCultureIgnoreCase) >= 0
                && text.IndexOf("insert", StringComparison.CurrentCultureIgnoreCase) < 0
                && text.IndexOf("update", StringComparison.CurrentCultureIgnoreCase) < 0
                && text.IndexOf("delete", StringComparison.CurrentCultureIgnoreCase) < 0
                )
            {
                try
                {
                    var ds = cmd.FillDataSet();
                    int i = 1;
                    foreach (DataTable dt in ds.Tables)
                    {
                        var tab = new TabItem { Header = $"Table {i++}", Content = DisplayTable(dt) };
                        tabControl.Items.Add(tab);
                        tab.Focus();
                    }
                }
                catch (Exception ex)
                {
                    DisplayMessage(ex.Message);
                }

            }
            else
            {
                try
                {
                    int count = cmd.ExecuteNonQuery();

                    string message = $"({count} row(s) affected)";
                    DisplayMessage(message);
                }
                catch (Exception ex)
                {
                    DisplayMessage(ex.Message);
                }
            }
        }

        private void DisplayMessage(string message)
        {
            var tab = new TabItem
            {
                Header = "Messages",
                Content = new TextBox
                {
                    Text = message,
                    IsReadOnly = true,
                    TextWrapping = TextWrapping.Wrap,
                    AcceptsReturn = true
                }
            };

            tabControl.Items.Add(tab);
            tab.Focus();
        }

        private DataGrid DisplayTable(DataTable table)
        {
            var evenRowColor = cfg.GetColor("gui.table.editor.AlternatingRowBackground", Colors.DimGray);
            var fkColor = cfg.GetColor("gui.table.editor.Foreground", Colors.LightGray);
            var bkColor = cfg.GetColor("gui.table.editor.RowBackground", Colors.Black);

            var dataGrid = new DataGrid
            {
                AlternationCount = 2,
                AlternatingRowBackground = new SolidColorBrush(evenRowColor)
                // Foreground = new SolidColorBrush(fkColor),
                // RowBackground = new SolidColorBrush(bkColor)
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
