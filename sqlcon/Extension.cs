using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

namespace sqlcon
{
    public static class Extension
    {
        public static void WriteLine(this TextWriter stream, DataTable dt, bool vertical = false)
        {
            stream.WriteLine($"[{dt.TableName}]");

            if (!vertical)
                dt.ToConsole(stream.WriteLine);
            else
                dt.ToVConsole(stream.WriteLine);

            stream.WriteLine("<{0} row{1}>", dt.Rows.Count, dt.Rows.Count > 1 ? "s" : "");
        }

        public static void WriteLine(this TextWriter stream, DataSet ds, bool vertical = false)
        {
            stream.WriteLine($"{ds.DataSetName}");
            foreach (DataTable dt in ds.Tables)
            {
                stream.WriteLine(dt);
            }

            stream.WriteLine();
        }
    }
}
