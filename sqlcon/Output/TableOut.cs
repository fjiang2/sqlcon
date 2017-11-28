using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Sys;
using Sys.Data;
using Tie;

namespace sqlcon
{
    class TableOut
    {
        private TableName tname;
        private UniqueTable rTable = null;

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
            get { return this.rTable; }
        }


        public bool HasPhysloc
        {
            get
            {
                if (this.rTable == null)
                    return false;

                return rTable.HasPhysloc;
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

        private static void _DisplayTable(UniqueTable udt, bool more, Command cmd)
        {
            DataTable table = udt.Table;

            if (table == null)
                return;

            if (cmd.Has("json"))
            {
                cout.WriteLine(ToJson(table));
                return;
            }

            if (cmd.Has("edit"))
            {
                var editor = new Windows.TableEditor(cmd.Configuration, udt);
                editor.ShowDialog();
                return;
            }

            if (cmd.IsVertical)
                table.ToVConsole(more);
            else
                table.ToConsole(more);
        }


        public static string ToJson(DataTable dt)
        {
            //array
            if (dt.Columns.Count == 1)
            {
                //string name = dt.Columns[0].ColumnName;
                string json = VAL.Boxing(dt.ToArray(row => row[0])).ToJson();
                //string.Format("{0}={1}", name, json);
                return json;
            }

            string[] columns = dt.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray();
            VAL L = new VAL();
            foreach (DataRow row in dt.Rows)
            {
                VAL V = new VAL();
                for (int i = 0; i < columns.Length; i++)
                {
                    object obj;
                    switch (row[i])
                    {
                        case Guid x:
                            obj = "{" + row[i].ToString() + "}";
                            break;

                        default:
                            obj = row[i];
                            break;
                    }

                    V.AddMember(columns[i], obj);
                }
                L.Add(V);
            }

            return L.ToJson();
        }

        private bool Display(Command cmd, SqlBuilder builder, TableName tname, int top)
        {
            try
            {
                DataTable table = builder.SqlCmd.FillDataTable();
                table.TableName = tname.ShortName;
                ShellHistory.SetLastResult(table);

                return Display(cmd, table, top);
            }
            catch (Exception ex)
            {
                cerr.WriteLine(ex.Message);
                return false;
            }
        }

        private bool Display(Command cmd, DataTable table, int top)
        {
            try
            {
                rTable = new UniqueTable(tname, table);
                _DisplayTable(rTable, top > 0 && table.Rows.Count == top, cmd);
            }
            catch (Exception ex)
            {
                cerr.WriteLine(ex.Message);
                return false;
            }

            return true;
        }

        public bool Display(Command cmd)
        {
            SqlBuilder builder;
            int top = cmd.Top;
            string[] columns = cmd.Columns;

            if (cmd.wildcard != null)
            {
                Locator where = LikeExpr(cmd.wildcard, cmd.Columns);
                builder = new SqlBuilder().SELECT.ROWID(cmd.HasRowId).COLUMNS().FROM(tname).WHERE(where);
            }
            else if (cmd.where != null)
            {
                var locator = new Locator(cmd.where);
                builder = new SqlBuilder().SELECT.TOP(top).ROWID(cmd.HasRowId).COLUMNS(columns).FROM(tname).WHERE(locator);
            }
            else if (cmd.Has("dup"))
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
            else
                builder = new SqlBuilder().SELECT.TOP(top).ROWID(cmd.HasRowId).COLUMNS(columns).FROM(tname);

            return Display(cmd, builder, tname, top);
        }


        public bool Display(Command cmd, string columns, Locator locator)
        {
            SqlBuilder builder;
            if (cmd.wildcard == null)
            {
                builder = new SqlBuilder().SELECT.TOP(cmd.Top).COLUMNS(columns).FROM(tname);
                if (locator != null)
                    builder.WHERE(locator);
            }
            else
            {
                Locator where = LikeExpr(cmd.wildcard, cmd.Columns);
                if (locator != null)
                    where = locator.And(where);

                builder = new SqlBuilder().SELECT.COLUMNS(columns).FROM(tname).WHERE(where);
            }

            return Display(cmd, builder, tname, cmd.Top);
        }


    }
}
