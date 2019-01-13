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

            textBox.Document.Blocks.Clear();
            if (link != null)
            {
                this.link = link;
                string text = link.ReadAllText();
                textBox.Document.Blocks.Add(new Paragraph(new Run(text)));
            }
            else
            {
                this.link = FileLink.CreateLink(untitled);
            }
            UpdateTitle();

            tabControl.SelectionChanged += TabControl_SelectionChanged;
            textBox.SelectionChanged += TextBox_SelectionChanged;
            textBox.TextChanged += TextBox_TextChanged;
            textBox.Focus();
        }




        private void UpdateTitle()
        {
            this.Title = $"{this.link} - sqlcon";
        }


        private RichTextBox textBox = new RichTextBox
        {
            FontFamily = new FontFamily("Consolas"),
            FontSize = 12,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto
        };

        #region InitializeComponent

        private TabControl tabControl = new TabControl();
        private TextBlock lblMessage = new TextBlock { Width = 300 };
        private TextBlock lblCursorPosition = new TextBlock { Width = 200, HorizontalAlignment = HorizontalAlignment.Left };
        private TextBlock lblRowCount = new TextBlock { Width = 200, HorizontalAlignment = HorizontalAlignment.Right };

        private static RoutedUICommand ExecuteCommand = new RoutedUICommand("Execute", "execute", typeof(SqlEditor), new InputGestureCollection { new KeyGesture(Key.F5, ModifierKeys.None, "F5") });
        private ComboBox comboPath;

        private void InitializeComponent(Configuration cfg)
        {
            this.Width = 1024;
            this.Height = 768;

            Button btnNew = WpfUtils.NewImageButton(ApplicationCommands.New, "New", "New(Ctrl-N)", "New_16x16.png");
            Button btnOpen = WpfUtils.NewImageButton(ApplicationCommands.Open, "Open", "Open(Ctrl-O)", "Open_16x16.png");
            Button btnSave = WpfUtils.NewImageButton(ApplicationCommands.Save, "Save", "Save(Ctrl-S)", "Save_16x16.png");
            Button btnExecute = WpfUtils.NewImageButton(ExecuteCommand, "Execute", "Execute(F5)", "Next_16x16.png");
            this.comboPath = new ComboBox { Width = 300, HorizontalAlignment = HorizontalAlignment.Right };
            this.comboPath.SelectionChanged += ComboPath_SelectionChanged;
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
            tray.ToolBars.Add(toolBar = new ToolBar());
            toolBar.Items.Add(comboPath);

            //status bar
            StatusBar statusBar = new StatusBar { Height = 20 };
            statusBar.Items.Add(new StatusBarItem { Content = lblMessage, HorizontalAlignment = HorizontalAlignment.Left });
            statusBar.Items.Add(new StatusBarItem { Content = lblCursorPosition, HorizontalAlignment = HorizontalAlignment.Right });
            statusBar.Items.Add(new StatusBarItem { Content = lblRowCount, HorizontalAlignment = HorizontalAlignment.Right });
            statusBar.SetValue(DockPanel.DockProperty, Dock.Bottom);
            dockPanel.Children.Add(statusBar);


            #region tree, editor and results
            Grid grid = new Grid();
            dockPanel.Children.Add(grid);

            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });
            grid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(5) });
            grid.ColumnDefinitions.Add(new ColumnDefinition());

            Grid grid1 = new Grid();
            GridSplitter vSplitter = new GridSplitter { Width = 5, VerticalAlignment = VerticalAlignment.Stretch };
            Grid grid2 = new Grid();

            grid1.SetValue(Grid.ColumnProperty, 0);
            vSplitter.SetValue(Grid.ColumnProperty, 1);
            grid2.SetValue(Grid.ColumnProperty, 2);
            grid.Children.Add(grid1);
            grid.Children.Add(vSplitter);
            grid.Children.Add(grid2);

            //Database Tree
            DbTreeUI treeView = new DbTreeUI
            {
                //Width = 160,
                Foreground = Brushes.White,
                //Background = Brushes.Black
            };
            grid1.Children.Add(treeView);

            treeView.CreateTree(cfg);
            treeView.PathChanged += TreeView_PathChanged;

            grid2.RowDefinitions.Add(new RowDefinition());
            grid2.RowDefinitions.Add(new RowDefinition { Height = new GridLength(5) });
            grid2.RowDefinitions.Add(new RowDefinition());

            textBox.Foreground = cfg.GetSolidBrush(ConfigKey._GUI_SQL_EDITOR_FOREGROUND, Colors.Black);
            textBox.Background = cfg.GetSolidBrush(ConfigKey._GUI_SQL_EDITOR_BACKGROUND, Colors.White);

            //Paragraph space
            Style style = new Style { TargetType = typeof(Paragraph) };
            style.Setters.Add(new Setter { Property = Block.MarginProperty, Value = new Thickness(0) });
            textBox.Resources.Add(typeof(Paragraph), style);

            GridSplitter hSplitter = new GridSplitter { Height = 5, HorizontalAlignment = HorizontalAlignment.Stretch };
            tabControl.Foreground = cfg.GetSolidBrush(ConfigKey._GUI_SQL_EDITOR_FOREGROUND, Colors.Black);
            tabControl.Background = cfg.GetSolidBrush(ConfigKey._GUI_SQL_EDITOR_BACKGROUND, Colors.White);

            textBox.SetValue(Grid.RowProperty, 0);
            hSplitter.SetValue(Grid.RowProperty, 1);
            tabControl.SetValue(Grid.RowProperty, 2);
            grid2.Children.Add(textBox);
            grid2.Children.Add(hSplitter);
            grid2.Children.Add(tabControl);

            #endregion


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

        private void ComboPath_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox combo = (ComboBox)sender;
            string path = combo.SelectedValue as string;
        }

        private void TreeView_PathChanged(object sender, EventArgs<TreeNode<IDataPath>> e)
        {
            TreeNode<IDataPath> node = e.Value;
            IDataPath name = node.Item;
            if (!(name is DatabaseName))
                return;

            //string path = node.XPath(x => x.Path);

            var found = comboPath.Items.OfType<IDataPath>().FirstOrDefault(x => x == name);
            if (found == null)
            {
                comboPath.Items.Add(name);
                found = name;
            }

            comboPath.SelectedValue = found;
        }


        #endregion


        private void TextBox_SelectionChanged(object sender, RoutedEventArgs e)
        {
            int row = textBox.LineNumber();
            int col = textBox.ColumnNumber();
            lblCursorPosition.Text = $"Ln {row}, Col {col}";
        }


        private bool isDirty = false;
        private void TextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            isDirty = true;
        }


        //display #of rows
        private void TabControl_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            TabItem tab = (tabControl.SelectedItem as TabItem);

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

        private void Execute()
        {
            string text = textBox.GetSelectionOrAllText();

            if (text == string.Empty)
                return;

            tabControl.Items.Clear();

            IDataPath name = comboPath.SelectedValue as IDataPath;
            if (name is DatabaseName)
                provider = (name as DatabaseName).Provider;

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
                catch (SqlException ex)
                {
                    DisplayMessage(ex.Message());
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
                catch (SqlException ex)
                {
                    DisplayMessage(ex.Message());
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
            var fkColor = cfg.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_MESSAGE_FOREGROUND, Colors.White);
            var bkColor = cfg.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_MESSAGE_BACKGROUND, Colors.Black);

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
            style.Setters.Add(new Setter { Property = ForegroundProperty, Value = fkColor });
            style.Setters.Add(new Setter { Property = BackgroundProperty, Value = bkColor });
            dataGrid.ColumnHeaderStyle = style;

            dataGrid.IsReadOnly = true;
            dataGrid.ItemsSource = table.DefaultView;

            return dataGrid;
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (isDirty && link.IsLocalLink)
            {
                var result = MessageBox.Show($"Save changes to {this.link}", "Warning", MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
                if (result == MessageBoxResult.Yes)
                {
                    Save();
                }
                else if (result == MessageBoxResult.Cancel)
                {
                    e.Cancel = true;
                }
            }

            base.OnClosed(e);
        }

        #region Commands

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

        public void New()
        {
            link = FileLink.CreateLink(untitled);
            textBox.Document.Blocks.Clear();
            UpdateTitle();
            isDirty = false;
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
                isDirty = false;
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
                    isDirty = false;
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
                    isDirty = false;
                }
            }

            return;
        }

        #endregion
    }
}
