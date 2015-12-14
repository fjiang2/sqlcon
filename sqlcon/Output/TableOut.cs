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


        private string LikeExpr(string wildcard, string[] columns)
        {
            wildcard = wildcard.Replace("*", "%").Replace("?", "_");

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

            string where = "";
            foreach (string column in columns)
            {
                if (where != "")
                    where += " OR ";
                where += string.Format("[{0}] LIKE '{1}'", column, wildcard);
            }

            return where;
        }

        private static void _DisplayTable(DataTable table, bool more, Command cmd)
        {
            if (table == null)
                return;

            if (cmd.ToJson)
            {
                stdio.WriteLine(ToJson(table));
                return;
            }

            if (cmd.ToCSharp)
            {
                stdio.WriteLine(ToCSharp(table));
                return;
            }

            if (cmd.IsVertical)
                table.ToVConsole(more);
            else
                table.ToConsole(more);
        }


        private static string ToJson(DataTable dt)
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
                    V.AddMember(columns[i], row[i]);
                }
                L.Add(V);
            }

            return L.ToJson();
        }

        private static string ToCSharp(DataTable dt)
        {
            //array
            if (dt.Columns.Count == 1)
            {
                var L1 = dt.ToArray(row => VAL.Boxing(row[0]).ToString());
                return "{" + string.Join(",", L1) + "}";
            }

            string[] columns = dt.Columns.Cast<DataColumn>().Select(col => col.ColumnName).ToArray();
            List<string> L = new List<string>();
            foreach (DataRow row in dt.Rows)
            {
                List<string> V = new List<string>();
                for (int i = 0; i < columns.Length; i++)
                {
                    V.Add(string.Format("{0}={1}", columns[i], VAL.Boxing(row[i]).ToString()));
                }

                L.Add("new {" + string.Join(", ", V) + "}");
            }

            return "new[] {\n" + string.Join(",\n", L) + "\n}";
        }

        private bool Display(Command cmd, SqlBuilder builder, int top)
        {
            try
            {
                DataTable table = builder.SqlCmd.FillDataTable();
                return Display(cmd, table, top);
            }
            catch (Exception ex)
            {
                stdio.ErrorFormat(ex.Message);
                return false;
            }
        }

        private bool Display(Command cmd, DataTable table, int top)
        {
            try
            {
                rTable = new UniqueTable(tname, table);
                _DisplayTable(rTable.Table, top > 0 && table.Rows.Count == top, cmd);
            }
            catch (Exception ex)
            {
                stdio.ErrorFormat(ex.Message);
                return false;
            }

            return true;
        }

        public bool Display(Command cmd)
        {
            SqlBuilder builder;
            int top = cmd.top;
            string[] columns = cmd.Columns;

            if (cmd.wildcard != null)
            {
                string where = LikeExpr(cmd.wildcard, cmd.Columns);
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
                    stdio.WriteLine("no duplicated record found");
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

            return Display(cmd, builder, top);
        }

      
        public bool Display(Command cmd, string columns, Locator locator)
        {
            SqlBuilder builder;
            if (cmd.wildcard == null)
            {
                builder = new SqlBuilder().SELECT.TOP(cmd.top).COLUMNS(columns).FROM(tname);
                if (locator != null)
                    builder.WHERE(locator);
            }
            else
            {
                string where = LikeExpr(cmd.wildcard, cmd.Columns);
                if (locator != null)
                    where = string.Format("({0}) AND ({1})", locator.Path, where);

                builder = new SqlBuilder().SELECT.COLUMNS(columns).FROM(tname).WHERE(where);
            }

            return Display(cmd, builder, cmd.top);
        }


    }
}
