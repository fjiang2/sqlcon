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
        public ScriptResultControl Tabs { get; }
        public TabItem TabItem { get; set; }
        public bool IsDirty { get; }
        public FileLink Link { get; set; }

        public TableResultPane(ScriptResultControl parent, Configuration cfg, TableName tname, int top)
        {
            this.Tabs = parent;

            var dt = new TableReader(tname, top).Table;
            CreateDataTableGrid(cfg, dt);
        }


        private void CreateDataTableGrid(Configuration cfg, DataTable dt)
        {
            var fkColor = cfg.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_TABLE_FOREGROUND, Colors.White);
            var bkColor = cfg.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_TABLE_BACKGROUND, Colors.Black);
            var evenRowColor = cfg.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_TABLE_ALTERNATINGROWBACKGROUND, Colors.DimGray);
            var oddRowColor = cfg.GetSolidBrush(ConfigKey._GUI_SQL_RESULT_TABLE_ROWBACKGROUND, Colors.Black);

            DataGrid dataGrid = new DataGrid
            {
                Foreground = fkColor,
                AlternationCount = 2,
                AlternatingRowBackground = evenRowColor,
                RowBackground = oddRowColor
            };

            this.Children.Add(dataGrid);

            var style = new Style(typeof(DataGridColumnHeader));
            style.Setters.Add(new Setter { Property = Control.ForegroundProperty, Value = fkColor });
            style.Setters.Add(new Setter { Property = Control.BackgroundProperty, Value = bkColor });
            dataGrid.ColumnHeaderStyle = style;

            dataGrid.IsReadOnly = true;
            dataGrid.ItemsSource = dt.DefaultView;
        }



        public void Save()
        {

        }
    }
}
