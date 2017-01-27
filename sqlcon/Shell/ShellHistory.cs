using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace sqlcon
{
    class ShellHistory
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
