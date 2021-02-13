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
        private TableName tname;
        private DataTable dt;

        public StringDumper(TableName tname)
        {
            this.tname = tname;

            DataSet ds = new DataSet
            {
                DataSetName = tname.DatabaseName.Name,
            };

            this.dt = new DataTable
            {
                TableName = tname.Name
            };

            ds.Tables.Add(dt);

            dt.Columns.Add(new DataColumn("File", typeof(string)) { MaxLength = 200 });
            dt.Columns.Add(new DataColumn("Line", typeof(int)));
            dt.Columns.Add(new DataColumn("Type", typeof(string)) { MaxLength = 10 });
            dt.Columns.Add(new DataColumn("Name", typeof(string)) { MaxLength = 200 });
            dt.Columns.Add(new DataColumn("Value", typeof(string)) { MaxLength = 200 });
            dt.PrimaryKey = new DataColumn[]
            {
                dt.Columns["File"],
                dt.Columns["Line"],
            };

            dt.AcceptChanges();
        }

        public DataTable Table => this.dt;

        public void Add(string file, int line, string type, string name, string value)
        {
            DataRow row = dt.NewRow();
            
            row["File"] = file;
            row["Line"] = line;
            row["Type"] = type;
            row["Name"] = name;
            row["Value"] = value;

            dt.Rows.Add(row);
        }

        public int Save(StreamWriter writer)
        {
            tname.SetTableSchema(dt);
            string SQL = tname.GenerateCreateTableClause(appendGO: true);
            writer.WriteLine(SQL);

            TableSchema schema = new TableSchema(tname);
            SqlScriptGeneration gen = new SqlScriptGeneration(SqlScriptType.INSERT, schema);
            return gen.GenerateByDbTable(dt, writer);
        }
    }
}
