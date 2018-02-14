﻿using System;
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
        public static void WriteData(this TextWriter stream, DataTable dt, bool header = false, bool footer = false, bool vertical = false)
        {
            if (header)
                stream.WriteLine($"[{dt.TableName}]");

            if (!vertical)
                dt.ToConsole(stream.WriteLine);
            else
                dt.ToVConsole(stream.WriteLine);

            if (footer)
                stream.WriteLine("<{0} row{1}>", dt.Rows.Count, dt.Rows.Count > 1 ? "s" : "");
        }

        public static void WriteData(this TextWriter stream, DataSet ds, bool header = false, bool tableHeader=false, bool tableFooter = false, bool vertical = false)
        {
            if (header)
                stream.WriteLine($"\"{ds.DataSetName}\"");

            foreach (DataTable dt in ds.Tables)
            {
                WriteData(stream, dt, tableHeader, tableFooter, vertical);
            }
        }


        public static void WriteData<T>(this TextWriter stream, IEnumerable<T> source, bool vertical = false)
        {
            if (!vertical)
                source.ToConsole(stream.WriteLine);
            else
                source.ToVConsole(stream.WriteLine);

        }


    }
}