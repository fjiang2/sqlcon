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
    partial class SqlEditor : Window
    {
        private ApplicationCommand cmd;
        private Configuration cfg;
        private FileLink link;
        private ConnectionProvider provider;

        private const string untitled = "untitled.sql";
        public SqlEditor(ApplicationCommand cmd, ConnectionProvider provider, FileLink link)
        {
            this.cmd = cmd;
            this.cfg = cmd.Configuration;

            InitializeComponent(cfg);

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

            IDataPath name = comboPath.SelectedValue as IDataPath;
            if (name is DatabaseName)
                provider = (name as DatabaseName).Provider;

            Execute(provider, text);
        }

        private void Execute(ConnectionProvider provider, string sql)
        {
            tabControl.Items.Clear();

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
