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
    interface IResultPane
    {
        ScriptResultControl Tabs { get; }
        TabItem TabItem { get; set; }
        FileLink Link { get; }
        bool IsDirty { get; }
        void Save();
    }

    class TableResultPane : Grid, IResultPane
    {
        private TextBlock lblRowCount;

        public ScriptResultControl Tabs { get; }
        public TabItem TabItem { get; set; }
        public bool IsDirty { get; }
        public FileLink Link { get; set; }

        public TableResultPane(ScriptResultControl parent, TableName tname, int top)
        {
            this.Tabs = parent;
            var dt = new TableReader(tname) { Top = top }.Table;

            InitializeComponent(dt);

            lblRowCount.Text = $"{dt.Rows.Count} row(s)";
        }

        private void InitializeComponent(DataTable dt)
        {
            Grid grid = this;
            grid.RowDefinitions.Add(new RowDefinition());
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(25) });

            DataGrid dataGrid = dt.CreateDataGrid();

            StatusBar statusBar = new StatusBar { Height = 20 };
            lblRowCount = new TextBlock { Width = 200, HorizontalAlignment = HorizontalAlignment.Right };
            statusBar.Items.Add(new StatusBarItem { Content = lblRowCount, HorizontalAlignment = HorizontalAlignment.Right });

            dataGrid.SetValue(Grid.RowProperty, 0);
            statusBar.SetValue(Grid.RowProperty, 1);

            this.Children.Add(dataGrid);
            this.Children.Add(statusBar);
        }

        public void Save()
        {

        }
    }
}
