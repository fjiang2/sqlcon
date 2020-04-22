using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Sys.Stdio;

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
                cout.WriteLine(tableName.ShortName);
#endif
                try
                {
                    string sql = TableSchemaDifference(sideType, tableName, new TableName(dname2, tableName.SchemaName, tableName.Name));
                    builder.Append(sql);
#if DEBUG
                    if (sql != string.Empty)
                        cout.WriteLine(sql);
#endif
                }
                catch (Exception ex)
                {
                    cerr.WriteLine("error:" + ex.Message);
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

                cout.WriteLine(tname1.ShortName);

                if (excludedTables.Contains(tableName.ShortName.ToUpper()))
                {
                    cout.WriteLine("skip to compare data on excluded table {0}", tableName.ShortName);
                    continue;
                }

                if (schema1.PrimaryKeys.Length == 0)
                {
                    cout.WriteLine("undefined primary key");
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
                sql = tableName1.GenerateClause();
            }

            return sql;
        }

        public static string TableDifference(CompareSideType sideType, ITableSchema schema1, ITableSchema schema2, string[] primaryKeys, string[] exceptColumns)
        {
            //don't compare identity column or computed column
            exceptColumns = schema1.Columns
                .Where(column => column.IsComputed || (column.IsIdentity && !column.IsPrimary))
                .Select(column => column.ColumnName)
                .Union(exceptColumns)
                .Distinct()
                .ToArray();

            TableCompare compare = new TableCompare(schema1, schema2)
            {
                SideType = sideType,
                ExceptColumns = exceptColumns
            };

            IPrimaryKeys keys = new PrimaryKeys(primaryKeys);
            return compare.Compare(keys);
        }

        #endregion}


        #region create all rows


        private static string GenerateRows(ITableSchema schema, TableReader reader)
        {

            var table = reader.Table;

            TableDataClause script = new TableDataClause(schema);

            StringBuilder builder = new StringBuilder();
            foreach (DataRow row in table.Rows)
            {
                var pair = new ColumnPairCollection(row);
                builder.Append(script.INSERT(pair)).AppendLine();
            }

            if (table.Rows.Count > 0)
                builder.AppendLine(TableClause.GO);

            return builder.ToString();
        }

        public static int GenerateRows(SqlScriptType type, StreamWriter writer, ITableSchema schema, Locator where, bool hasIfExists)
        {
            SqlScriptGeneration gen = new SqlScriptGeneration(type, schema)
            {
                Where = where,
                HasIfExists = hasIfExists
            };

            return gen.Generate(writer);
        }

        public static string GenerateTemplate(ITableSchema schema, SqlScriptType type, bool ifExists)
        {
            TableName tableName = schema.TableName;
            TableClause script = new TableClause(schema);
            switch (type)
            {
                case SqlScriptType.INSERT:
                    return script.INSERT(schema.Columns);

                case SqlScriptType.SELECT:
                    return script.SELECT(schema.Columns);

                case SqlScriptType.UPDATE:
                    return script.UPDATE(schema.Columns);

                case SqlScriptType.INSERT_OR_UPDATE:
                    return script.INSERT_OR_UPDATE(schema.Columns);

                case SqlScriptType.DELETE:
                    return new Dependency(tableName.DatabaseName).DELETE(tableName)
                     + script.DELETE(schema.Columns);

                case SqlScriptType.DROP:
                    return new Dependency(tableName.DatabaseName).DROP_TABLE(tableName, ifExists)
                        + script.DROP_TABLE(ifExists);
            }

            return null;
        }

        #endregion

    }
}
