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
        private static DataSet LastResult;

        public static void SetLastResult(DataTable dt)
        {
            LastResult = dt.DataSet;
        }

        public static void SetLastResult(DataSet ds)
        {
            LastResult = ds;
        }

        public static DataSet LastOrCurrentTable(TableName tname, int rows = -1)
        {
            if (LastResult != null)
                return LastResult;

            if (tname != null)
            {
                string sql;
                if (rows > 0)
                    sql = $"SELECT TOP {rows} * FROM {tname.FormalName}";
                else
                    sql = $"SELECT * FROM {tname.FormalName}";

                var dt = new SqlCmd(tname.Provider, sql).FillDataTable();
                dt.TableName = tname.Name;
                return dt.DataSet;
            }

            return null;
        }

        public static DataTable LastTable()
        {
            if (LastResult != null)
            {
                if (LastResult.Tables.Count > 0)
                    return LastResult.Tables[0];
            }

            return null;
        }

        public static DataSet LastDataSet()
        {
            return LastResult;
        }
    }
}
