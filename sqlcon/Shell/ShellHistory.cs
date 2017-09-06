using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Sys.Data;

namespace sqlcon
{
    static class ShellHistory
    {
        /// <summary>
        /// keep last answer
        /// </summary>
        private static object LastResult;

        public static void SetLastResult(DataTable dt)
        {
            LastResult = dt;
        }

        public static void SetLastResult(DataSet ds)
        {
            LastResult = ds;
        }

        public static DataTable LastOrCurrentTable(TableName tname, int rows = -1)
        {
            var dt = LastTable();
            if (dt != null)
                return dt;

            if (tname != null)
            {
                string sql;
                if (rows > 0)
                    sql = $"SELECT TOP {rows} * FROM {tname.FormalName}";
                else
                    sql = $"SELECT * FROM {tname.FormalName}";

                dt = new SqlCmd(tname.Provider, sql).FillDataTable();
                dt.TableName = tname.Name;
            }

            return dt;
        }

        public static DataTable LastTable()
        {
            DataTable dt = null;
            if (LastResult is DataTable)
            {
                dt = LastResult as DataTable;
            }
            else if (LastResult is DataSet)
            {
                var ds = LastResult as DataSet;
                if (ds.Tables.Count > 0)
                    dt = ds.Tables[0];
            }

            return dt;
        }

        public static DataSet LastDataSet()
        {
            if (LastResult is DataTable)
            {
                return (LastResult as DataTable).DataSet;
            }
            else if (LastResult is DataSet)
            {
                return LastResult as DataSet;
            }

            return null;
        }
    }
}
