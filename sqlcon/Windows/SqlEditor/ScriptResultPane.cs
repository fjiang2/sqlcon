using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Input;
using System.IO;
using System.Windows.Controls.Primitives;
using System.Windows.Documents;
using System.Data;
using System.Data.SqlClient;

using Sys.Data;
using Sys.Data.IO;

namespace sqlcon.Windows
{
    class ScriptResultPane : Grid, IResultPane
    {
        private TextBlock lblRowCount;

        public ScriptResultControl Tabs { get; }
        public TabItem TabItem { get; set; }
        public TabControl TabControl { get; private set; }
        public RichTextBox TextBox { get; private set; }
        public FileLink Link { get; set; }
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
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25) });

            TextBox = new RichTextBox
            {
                FontFamily = new FontFamily("Consolas"),
                FontSize = 12,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto
            };
            TextBox.Foreground = Themes.SqlEditor.Foreground;
            TextBox.Background = Themes.SqlEditor.Background;

            //Paragraph space
            Style style = new Style { TargetType = typeof(Paragraph) };
            style.Setters.Add(new Setter { Property = Block.MarginProperty, Value = new Thickness(0) });
            TextBox.Resources.Add(typeof(Paragraph), style);

            GridSplitter hSplitter = new GridSplitter { Height = 5, HorizontalAlignment = HorizontalAlignment.Stretch };
            TabControl = new TabControl();
            TabControl.Foreground = Themes.SqlEditor.Foreground;
            TabControl.Background = Themes.SqlEditor.Background; 

            lblRowCount = new TextBlock { Width = 200, HorizontalAlignment = HorizontalAlignment.Right };
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

        public string Text => TextBox.GetSelectionOrAllText();

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

        public void Execute(ConnectionProvider provider)
        {
            string sql = TextBox.GetSelectionOrAllText();
            if (sql == string.Empty)
                return;

            TabControl.Items.Clear();

            var cmd = new SqlCmd(provider, sql);
            if (sql.IndexOf("select", StringComparison.CurrentCultureIgnoreCase) >= 0
                && sql.IndexOf("insert", StringComparison.CurrentCultureIgnoreCase) < 0
                && sql.IndexOf("update", StringComparison.CurrentCultureIgnoreCase) < 0
                && sql.IndexOf("delete", StringComparison.CurrentCultureIgnoreCase) < 0
                )
            {
                try
                {
                    StringBuilder builder = new StringBuilder();
                    var ds = cmd.FillDataSet();
                    int i = 1;
                    foreach (DataTable dt in ds.Tables)
                    {
                        var tab = new TabItem { Header = $"Table {i++}", Content = dt.CreateDataGrid() };
                        TabControl.Items.Add(tab);
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

            if (TabControl.HasItems)
                (TabControl.Items[0] as TabItem).Focus();
        }

        private void DisplayMessage(string message)
        {
            var tab = new TabItem
            {
                Header = "Messages",
                Content = new TextBox
                {
                    Text = message,
                    Foreground = Themes.SqlResult.Message.Foreground,
                    Background = Themes.SqlResult.Message.Background,
                    IsReadOnly = true,
                    TextWrapping = TextWrapping.Wrap,
                    AcceptsReturn = true
                }
            };

            TabControl.Items.Add(tab);
            tab.Focus();
        }


        public void Save()
        {
            if (!Link.Url.StartsWith("untitled") || !Link.Url.EndsWith(".sql"))
            {
                try
                {
                    Link.Save(TextBox.GetAllText());
                    Tabs.Editor.ShowStatus("saved successfully");
                    IsDirty = false;
                }
                catch (Exception ex)
                {
                    Tabs.Editor.ShowStatus(ex.Message);
                }
                return;
            }

            var saveFile = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Sql Script Files (*.sql)|*.sql|Text Files (*.txt)|*.txt|All Files (*.*)|*.*",
                FileName = Link.Url
            };

            if (saveFile.ShowDialog() == true)
            {
                TextRange documentTextRange = new TextRange(TextBox.Document.ContentStart, TextBox.Document.ContentEnd);

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

                    Link = FileLink.CreateLink(saveFile.FileName);
                    IsDirty = false;
                }
            }

            return;
        }

    }
}
