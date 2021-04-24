using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

namespace Sys.Data.Resource
{
    public class StringDumper
    {
        public string FileName { get; set; } = "File";
        public string Line { get; set; } = "Line";
        public string Column { get; set; } = "Col";
        public string Type { get; set; } = "Type";
        public string Name { get; set; } = "Name";
        public string Value { get; set; } = "Value";

        private TableName tname;
        private DataTable dt;

        public StringDumper(TableName tname)
        {
            this.tname = tname;
        }

        public void Initialize()
        { 
            DataSet ds = new DataSet();
            this.dt = new DataTable();
            ds.Tables.Add(dt);
            dt.SetSchemaAndTableName(tname);

            dt.Columns.Add(new DataColumn(FileName, typeof(string)) { MaxLength = 200 });
            dt.Columns.Add(new DataColumn(Line, typeof(int)));
            dt.Columns.Add(new DataColumn(Column, typeof(int)));
            dt.Columns.Add(new DataColumn(Type, typeof(string)) { MaxLength = 10 });
            dt.Columns.Add(new DataColumn(Name, typeof(string)) { MaxLength = 200 });
            dt.Columns.Add(new DataColumn(Value, typeof(string)) { MaxLength = 200 });
            dt.PrimaryKey = new DataColumn[]
            {
                dt.Columns[FileName],
                dt.Columns[Line],
                dt.Columns[Column],
            };

            dt.AcceptChanges();
        }

        public DataTable Table => this.dt;

        public void Add(string file, int line, int col, string type, string name, string value)
        {
            DataRow row = dt.NewRow();

            row[FileName] = file;
            row[Line] = line;
            row[Column] = col;
            row[Type] = type;
            row[Value] = value;

            dt.Rows.Add(row);
        }

        public int Save(TextWriter writer)
        {
            return dt.WriteSql(writer, tname);
        }
    }
}
