using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.IO;

namespace Sys.Data.Comparison
{
    public static class Compare
    {
       

        #region compare database schema/data
        public static string DatabaseSchemaDifference(CompareSideType sideType, DatabaseName dname1, DatabaseName dname2)
        {
            TableName[] names = dname1.GetDependencyTableNames();

            StringBuilder builder = new StringBuilder();
            foreach (TableName tableName in names)
            {
#if DEBUG
                Console.WriteLine(tableName.ShortName);
#endif
                try
                {
                    string sql = TableSchemaDifference(sideType, tableName, new TableName(dname2, tableName.SchemaName, tableName.Name));
                    builder.Append(sql);
#if DEBUG
                    if (sql != string.Empty)
                        Console.WriteLine(sql);
#endif
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error:" + ex.Message);
                }
            }

            return builder.ToString();
        }


       
        public static string DatabaseDifference(CompareSideType sideType, DatabaseName dname1, DatabaseName dname2, string[] excludedTables)
        {
            TableName[] names = dname1.GetDependencyTableNames();
            excludedTables = excludedTables.Select(row => row.ToUpper()).ToArray();

            StringBuilder builder = new StringBuilder();
            foreach (TableName tableName in names)
            {
                TableName tname1 = tableName;
                TableName tname2 = new TableName(dname2, tableName.SchemaName, tableName.Name);

                TableSchema schema1 = new TableSchema(tname1);
                TableSchema schema2 = new TableSchema(tname2);
                
                Console.WriteLine(tname1.ShortName);

                if (excludedTables.Contains(tableName.ShortName.ToUpper()))
                {
                    Console.WriteLine("skip to compare data on excluded table {0}", tableName.ShortName);
                    continue;
                }

                if (schema1.PrimaryKeys.Length == 0)
                {
                    Console.WriteLine("undefined primary key");
                    continue;
                }

                if (tname2.Exists())
                {
                    builder.Append(TableDifference(sideType, schema1, schema2, schema1.PrimaryKeys.Keys, new string[] { }));
                }
                else
                {
                    builder.Append(Compare.GenerateRows(schema1, new TableReader(tname1)));
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }

        #endregion


        #region compare table schema/data

        public static string TableSchemaDifference(CompareSideType sideType, TableName tableName1, TableName tableName2)
        {

            string sql;

            if (tableName2.Exists())
            {
                TableSchemaCompare compare = new TableSchemaCompare(tableName1, tableName2) { SideType = sideType };
                sql = compare.Compare();
            }
            else
            {
                sql = tableName1.GenerateScript();
            }

            return sql;
        }

        public static string TableDifference(CompareSideType sideType, TableSchema schema1, TableSchema schema2, string[] primaryKeys, string[] exceptColumns)
        {
            TableCompare compare = new TableCompare(schema1, schema2) { SideType = sideType, ExceptColumns = exceptColumns };
            IPrimaryKeys keys = new PrimaryKeys(primaryKeys);
            return compare.Compare(keys);
        }

        #endregion}


        #region create all rows


        private static string GenerateRows(TableSchema schema, TableReader reader)
        {

            var table = reader.Table; ;

            TableScript script = new TableScript(schema);

            StringBuilder builder = new StringBuilder();
            foreach (DataRow row in table.Rows)
                builder.Append(script.INSERT(row)).AppendLine();

            if (table.Rows.Count > 0)
                builder.AppendLine(TableScript.GO);

            return builder.ToString();
        }

        public static int GenerateRows(StreamWriter writer, TableSchema schema, Locator where, bool hasIfExists)
        {
            TableName tableName = schema.TableName;
            string sql = string.Format("SELECT * FROM {0}", tableName);
            if (where != null)
                sql = string.Format("SELECT * FROM {0} WHERE {1}", tableName, where);

            SqlCmd cmd = new SqlCmd(tableName.Provider, sql);
            TableScript script = new TableScript(schema);

            int count = 0;
            cmd.Execute(
                reader =>
                {
                    DataTable schema1 = reader.GetSchemaTable();

                    string[] columns = schema1.AsEnumerable().Select(row => row.Field<string>("ColumnName")).ToArray();
                    object[] values = new object[columns.Length];

                    while (reader.HasRows)
                    {
                        while (reader.Read())
                        {
                            reader.GetValues(values);
                            if(hasIfExists)
                                writer.WriteLine(script.IF_NOT_EXISTS_INSERT(columns, values));
                            else
                                writer.WriteLine(script.INSERT(columns, values));

                            count++;
                            if (count % 5000 == 0)
                                writer.WriteLine(TableScript.GO);

                        }
                        reader.NextResult();
                    }
                });

            if (count != 0)
                writer.WriteLine(TableScript.GO);

            return count;
        }

        public static string GenerateTemplate(TableSchema schema, SqlScriptType type)
        {
            TableName tableName = schema.TableName;
            TableScript script = new TableScript(schema);
            switch (type)
            {
                case SqlScriptType.INSERT:
                    return script.INSERT(schema.Columns);

                case SqlScriptType.SELECT:
                    return script.SELECT(schema.Columns);

                case SqlScriptType.UPDATE:
                    return script.UPDATE(schema.Columns);

                case SqlScriptType.DELETE:
                    return script.DELETE(schema.Columns);
            }

            return null;
        }

        #endregion

    }
}
