using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using System.Data.Common;

namespace Sys.Data
{

    /// <summary>
    /// Generate INSERT,
    /// UPDATE
    /// and IF EXISTS UPDATE ELSE INSERT
    /// </summary>
    class SqlScriptGeneration
    {
        private SqlScriptType type;
        private TableDataClause script;
        private ITableSchema schema;
        private int count;

        public Locator Where { get; set; }
        public SqlScriptGenerationOption Option { get; set; } = new SqlScriptGenerationOption
        {
            HasIfExists = false,
            InsertWithoutColumns = false,
        };

        public SqlScriptGeneration(SqlScriptType type, ITableSchema schema)
        {
            this.type = type;
            this.schema = schema;
            this.script = new TableDataClause(schema);
        }

        public int Generate(StreamWriter writer, IProgress<int> progress)
        {
            TableName tableName = schema.TableName;
            string sql = string.Format("SELECT * FROM {0}", tableName);
            if (Where != null)
                sql = string.Format("SELECT * FROM {0} WHERE {1}", tableName, Where);

            SqlCmd cmd = new SqlCmd(tableName.Provider, sql);

            count = 0;
            cmd.Read(reader => Generate(cmd, reader, writer, progress));
            return count;
        }

        private void Generate(SqlCmd cmd, DbDataReader reader, TextWriter writer, IProgress<int> progress)
        {
            if (reader != null)
            {
                GenerateByDbReader(reader, writer, progress);
                return;
            }

            var dt = cmd.FillDataTable();
            GenerateByDbTable(dt, writer);
        }

        public int GenerateByDbTable(DataTable dt, TextWriter writer)
        {
            string[] columns = dt.Columns.ToEnumerable<DataColumn, string>(col => col.ColumnName).ToArray();
            object[] values = new object[columns.Length];

            foreach (DataRow row in dt.Rows)
            {
                values = row.ItemArray;
                var pairs = new ColumnPairCollection(columns, values);
                GenerateRow(writer, pairs);

                count++;
                if (count % 5000 == 0)
                    writer.WriteLine(SqlScript.GO);
            }

            if (count != 0)
                writer.WriteLine(SqlScript.GO);

            return count;
        }

        private int GenerateByDbReader(DbDataReader reader, TextWriter writer, IProgress<int> progress)
        {
            DataTable schema1 = reader.GetSchemaTable();

            string[] columns = schema1.AsEnumerable().Select(row => row.Field<string>("ColumnName")).ToArray();
            object[] values = new object[columns.Length];

            int step = 0;
            //this is signle table
            //while (reader.HasRows)
            {
                while (reader.Read())
                {
                    step++;

                    if (step % 19 == 0)
                        progress?.Report(step);

                    reader.GetValues(values);
                    var pairs = new ColumnPairCollection(columns, values);
                    GenerateRow(writer, pairs);

                    count++;
                    if (count % 5000 == 0)
                        writer.WriteLine(SqlScript.GO);

                }

                reader.NextResult();
            }

            if (count != 0)
                writer.WriteLine(SqlScript.GO);

            return count;
        }

        private void GenerateRow(TextWriter writer, ColumnPairCollection pairs)
        {
            switch (type)
            {
                case SqlScriptType.INSERT:
                    if (Option.HasIfExists)
                        writer.WriteLine(script.IF_NOT_EXISTS_INSERT(pairs));
                    else
                        writer.WriteLine(script.INSERT(pairs, Option.InsertWithoutColumns, Option.IncludeIdentity));
                    break;

                case SqlScriptType.UPDATE:
                    writer.WriteLine(script.UPDATE(pairs));
                    break;

                case SqlScriptType.INSERT_OR_UPDATE:
                    writer.WriteLine(script.IF_NOT_EXISTS_INSERT_ELSE_UPDATE(pairs));
                    break;
            }
        }
    }
}
