using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Sys;
using Sys.Data;
using Sys.Stdio;

namespace sqlcon
{
    class TableOut
    {
        private TableName tname;
        private UniqueTable uniqueTable = null;

        public TableOut(TableName tableName)
        {
            this.tname = tableName;
        }

        public TableName TableName
        {
            get { return this.tname; }
        }

        public UniqueTable Table
        {
            get { return this.uniqueTable; }
        }


        public bool HasPhysloc
        {
            get
            {
                if (this.uniqueTable == null)
                    return false;

                return uniqueTable.HasPhysloc;
            }
        }


        private Locator LikeExpr(string wildcard, string[] columns)
        {
            if (columns.Length == 0)
            {
                var schema = new TableSchema(tname);
                List<string> L = new List<string>();
                foreach (var c in schema.Columns)
                {
                    if (c.CType == CType.NVarChar || c.CType == CType.NChar || c.CType == CType.NText)
                        L.Add(c.ColumnName);
                }
                columns = L.ToArray();
            }

            return new Locator(wildcard, columns);
        }

        private static void _DisplayTable(UniqueTable udt, bool more, ApplicationCommand cmd)
        {
            DataTable table = udt.Table;

            if (table == null)
                return;

            if (cmd.Has("json"))
            {
                cout.WriteLine(table.WriteJson(JsonStyle.Normal, excludeTableName: false));
                return;
            }

#if WINDOWS
            if (cmd.Has("edit"))
            {
                var editor = new Windows.TableEditor(udt);
                editor.ShowDialog();
                return;
            }
#endif

            int maxColumnWidth = Config.console.table.grid.MaxColumnWidth;

            table.ToConsole(vertical: cmd.IsVertical, more: more, outputDbNull: true, maxColumnWidth);
        }


        private bool Display(ApplicationCommand cmd, SqlBuilder builder, TableName tname, int top)
        {
            try
            {
                DataTable table = builder.SqlCmd.FillDataTable();
                table.SetSchemaAndTableName(tname);
                ShellHistory.SetLastResult(table);

                return Display(cmd, table, top);
            }
            catch (Exception ex)
            {
                cerr.WriteLine(ex.Message);
                return false;
            }
        }

        private bool Display(ApplicationCommand cmd, DataTable table, int top)
        {
            try
            {
                uniqueTable = new UniqueTable(tname, table);
                _DisplayTable(uniqueTable, top > 0 && table.Rows.Count == top, cmd);
            }
            catch (Exception ex)
            {
                cerr.WriteLine(ex.Message);
                return false;
            }

            return true;
        }


       
        public bool Display(ApplicationCommand cmd)
        {
            string[] columns = cmd.Columns;

            if (cmd.Has("dup"))
            {
                DuplicatedTable dup = new DuplicatedTable(tname, columns);
                if (dup.group.Rows.Count == 0)
                {
                    cout.WriteLine("no duplicated record found");
                    return true;
                }

                if (cmd.IsSchema)
                {
                    Display(cmd, dup.group, 0);
                }
                else
                {
                    dup.Dispaly(dt => Display(cmd, dt, 0));
                }

                return true;
            }

            SqlBuilder builder;
            int top = cmd.Top;
            
            if (tname.Provider.DpType == DbProviderType.Sqlite)
                top = 0;

            bool hasRowId = cmd.HasRowId;
            Locator locator = Locator.Empty;

            if (cmd.wildcard != null)
            {
                locator = LikeExpr(cmd.wildcard, cmd.Columns);
            }
            else if (cmd.where != null)
            {
                locator = new Locator(cmd.where);
            }

            builder = new SqlBuilder().SELECT().TOP(top);
            
            if (hasRowId)
                builder.COLUMNS(UniqueTable.ROWID_COLUMN(tname));
            
            builder.COLUMNS(columns).FROM(tname).WHERE(locator);

            return Display(cmd, builder, tname, top);
        }


        public bool Display(ApplicationCommand cmd, string columns, Locator locator)
        {
            SqlBuilder builder;
            if (cmd.wildcard == null)
            {
                builder = new SqlBuilder().SELECT().TOP(cmd.Top).COLUMNS(columns).FROM(tname);
                if (locator != null)
                    builder.WHERE(locator);
            }
            else
            {
                Locator where = LikeExpr(cmd.wildcard, cmd.Columns);
                if (locator != null)
                    where = locator.And(where);

                builder = new SqlBuilder().SELECT().COLUMNS(columns).FROM(tname).WHERE(where);
            }

            return Display(cmd, builder, tname, cmd.Top);
        }


    }
}
