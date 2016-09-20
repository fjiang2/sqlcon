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
using Sys.Data;
using Sys.IO;

namespace sqlcon.Windows
{
    class SqlEditor : Window
    {
        private Configuration cfg;
        private FileLink link;
        private ConnectionProvider provider;

        private const string untitled = "untitled.sql";

        public SqlEditor(Configuration cfg, ConnectionProvider provider, FileLink link)
        {
            InitializeComponent(cfg);

            this.cfg = cfg;
            this.provider = provider;

            textBox.SelectionChanged += TextBox_SelectionChanged;
            textBox.Focus();


            textBox.Document.Blocks.Clear();
            if (link != null)
            {
                this.link = link;
                string text = link.ReadAllText();
                textBox.Document.Blocks.Add(new Paragraph(new Run(text)));
            }
            else
            {
                textBox.Document.Blocks.Clear();
                this.link = FileLink.CreateLink(untitled);
            }

            UpdateTitle();

            CommandBinding binding;
            RoutedUICommand[] commands = new RoutedUICommand[]
               {
                  ApplicationCommands.New,
                  ApplicationCommands.Open,
                  ApplicationCommands.Save,
                  ExecuteCommand
               };

            foreach (var cmd in commands)
            {
                binding = new CommandBinding(cmd);
                binding.Executed += commandExecute;
                binding.CanExecute += commandCanExecute;
                this.CommandBindings.Add(binding);
            }

        }

        private void commandCanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            if (e.Command == ExecuteCommand)
            {
                e.CanExecute = textBox.GetSelectionOrAllText() != string.Empty;
            }
            else if (e.Command == ApplicationCommands.Save)
                e.CanExecute = link.IsLocalLink;
            else
                e.CanExecute = true;
        }

        private void commandExecute(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ExecuteCommand)
                Execute();
            else if (e.Command == ApplicationCommands.New)
                New();
            else if (e.Command == ApplicationCommands.Save)
                Save();
            else if (e.Command == ApplicationCommands.Open)
                Open();
        }

        private void UpdateTitle()
        {
            this.Title = $"{this.link} - sqlcon";
        }


        private RichTextBox textBox = new RichTextBox { FontFamily = new FontFamily("Consolas"), FontSize = 12 };
        private TabControl tabControl = new TabControl();
        private TextBlock lblMessage = new TextBlock { Width = 200 };
        private TextBlock lblCursorPosition = new TextBlock { Width = 200, HorizontalAlignment = HorizontalAlignment.Right };

        private static RoutedUICommand ExecuteCommand = new RoutedUICommand("Execute", "execute", typeof(SqlEditor), new InputGestureCollection { new KeyGesture(Key.F5, ModifierKeys.None, "F5") });

        private void InitializeComponent(Configuration cfg)
        {
            this.Width = 1024;
            this.Height = 768;

            Button btnNew = NewImageButton(ApplicationCommands.New, "New", "New(Ctrl-N)", "New_16x16.png");
            Button btnOpen = NewImageButton(ApplicationCommands.Open, "Open", "Open(Ctrl-O)", "Open_16x16.png");
            Button btnSave = NewImageButton(ApplicationCommands.Save, "Save", "Save(Ctrl-S)", "Save_16x16.png");
            Button btnExecute = NewImageButton(ExecuteCommand, "Execute", "Execute(F5)", "Next_16x16.png");

            DockPanel dockPanel = new DockPanel();
            this.Content = dockPanel;

            //Tool bar
            ToolBarTray tray = new ToolBarTray();
            tray.SetValue(DockPanel.DockProperty, Dock.Top);
            dockPanel.Children.Add(tray);

            ToolBar toolBar;
            tray.ToolBars.Add(toolBar = new ToolBar());
            toolBar.Items.Add(btnNew);
            toolBar.Items.Add(btnOpen);
            toolBar.Items.Add(btnSave);
            tray.ToolBars.Add(toolBar = new ToolBar());
            toolBar.Items.Add(btnExecute);

            //status bar
            StatusBar statusBar = new StatusBar { Height = 20 };
            statusBar.Items.Add(new StatusBarItem { Content = lblMessage, HorizontalAlignment = HorizontalAlignment.Left });
            statusBar.Items.Add(new StatusBarItem { Content = lblCursorPosition, HorizontalAlignment = HorizontalAlignment.Right });
            statusBar.SetValue(DockPanel.DockProperty, Dock.Bottom);
            dockPanel.Children.Add(statusBar);


            #region editor and results
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5) });
            grid.RowDefinitions.Add(new RowDefinition());

            dockPanel.Children.Add(grid);
            textBox.Foreground = cfg.GetSolidBrush("gui.sql.editor.Foreground", Colors.Black);
            textBox.Background = cfg.GetSolidBrush("gui.sql.editor.Background", Colors.White);

            //Paragraph space
            Style style = new Style { TargetType = typeof(Paragraph) };
            style.Setters.Add(new Setter { Property = Block.MarginProperty, Value = new Thickness(0) });
            textBox.Resources.Add(typeof(Paragraph), style);

            GridSplitter splitter = new GridSplitter { Height = 5, HorizontalAlignment = HorizontalAlignment.Stretch };
            tabControl.Foreground = cfg.GetSolidBrush("gui.sql.editor.Foreground", Colors.Black);
            tabControl.Background = cfg.GetSolidBrush("gui.sql.editor.Background", Colors.White);

            textBox.SetValue(Grid.RowProperty, 0);
            splitter.SetValue(Grid.RowProperty, 1);
            tabControl.SetValue(Grid.RowProperty, 2);
            grid.Children.Add(textBox);
            grid.Children.Add(splitter);
            grid.Children.Add(tabControl);

            #endregion
        }

        private static Button NewImageButton(ICommand command, string text, string toolTip, string image)
        {
            //make sure: build action on image to Resource
            string pack = "pack://application:,,,/sqlcon;component/Windows/images";
            StackPanel stackPanel = new StackPanel { Orientation = Orientation.Horizontal };
            stackPanel.Children.Add(new Image { Source = new BitmapImage(new Uri($"{pack}/{image}", UriKind.Absolute)) });
            stackPanel.Children.Add(new TextBlock { Text = text });

            return new Button
            {
                Command = command,
                Content = stackPanel,
                Margin = new Thickness(5),
                ToolTip = toolTip
            };
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

            string text = textBox.GetSelectionOrAllText();

            var cmd = new SqlCmd(provider, text);
            if (text.IndexOf("select", StringComparison.CurrentCultureIgnoreCase) >= 0
                && text.IndexOf("insert", StringComparison.CurrentCultureIgnoreCase) < 0
                && text.IndexOf("update", StringComparison.CurrentCultureIgnoreCase) < 0
                && text.IndexOf("delete", StringComparison.CurrentCultureIgnoreCase) < 0
                )
            {
                try
                {
                    StringBuilder builder = new StringBuilder();
                    var ds = cmd.FillDataSet();
                    int i = 1;
                    foreach (DataTable dt in ds.Tables)
                    {
                        var tab = new TabItem { Header = $"Table {i++}", Content = DisplayTable(dt) };
                        tabControl.Items.Add(tab);
                        builder.AppendLine($"{dt.Rows.Count} row(s) affected");
                    }

                    DisplayMessage(builder.ToString());
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
                    string message = $"{count} row(s) affected";
                    DisplayMessage(message);
                }
                catch (Exception ex)
                {
                    DisplayMessage(ex.Message);
                }
            }

            if (tabControl.HasItems)
                (tabControl.Items[0] as TabItem).Focus();
        }

        private void DisplayMessage(string message)
        {
            var fkColor = cfg.GetSolidBrush("gui.sql.result.message.Foreground", Colors.White);
            var bkColor = cfg.GetSolidBrush("gui.sql.result.message.Background", Colors.Black);

            var tab = new TabItem
            {
                Header = "Messages",
                Content = new TextBox
                {
                    Text = message,
                    Foreground = fkColor,
                    Background = bkColor,
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
            var fkColor = cfg.GetSolidBrush("gui.sql.result.table.Foreground", Colors.White);
            var bkColor = cfg.GetSolidBrush("gui.sql.result.table.Background", Colors.Black);
            var evenRowColor = cfg.GetSolidBrush("gui.sql.result.table.AlternatingRowBackground", Colors.DimGray);
            var oddRowColor = cfg.GetSolidBrush("gui.sql.result.table.RowBackground", Colors.Black);

            var dataGrid = new DataGrid
            {
                Foreground = fkColor,
                AlternationCount = 2,
                AlternatingRowBackground = evenRowColor,
                RowBackground = oddRowColor
            };

            var style = new Style(typeof(DataGridColumnHeader));
            style.Setters.Add(new Setter { Property = ForegroundProperty, Value = fkColor });
            style.Setters.Add(new Setter { Property = BackgroundProperty, Value = bkColor });
            dataGrid.ColumnHeaderStyle = style;

            dataGrid.IsReadOnly = true;
            dataGrid.ItemsSource = table.DefaultView;

            return dataGrid;
        }
        public void New()
        {
            link = FileLink.CreateLink(untitled);
            textBox.Document.Blocks.Clear();
            UpdateTitle();
        }

        public void Open()
        {
            var openFile = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Sql Script Files (*.sql)|*.sql|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                FileName = link.Url
            };

            if (openFile.ShowDialog(this) == true)
            {
                link = FileLink.CreateLink(openFile.FileName);
                string text = link.ReadAllText();
                textBox.Document.Blocks.Clear();
                textBox.Document.Blocks.Add(new Paragraph(new Run(text)));
                UpdateTitle();
            }
        }

        public void Save()
        {

            if (link.Url != untitled)
            {
                try
                {
                    link.Save(textBox.GetAllText());
                    lblMessage.Text = "saved successfully";
                }
                catch (Exception ex)
                {
                    lblMessage.Text = ex.Message;
                }
                return;
            }

            var saveFile = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Sql Script Files (*.sql)|*.sql|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                FileName = link.Url
            };

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

                    link = FileLink.CreateLink(saveFile.FileName);
                    UpdateTitle();
                }
            }

            return;
        }
    }
}
