using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;
using Sys.Data.Comparison;

namespace Sys.Data
{
    class TableClause
    {
        public static readonly string GO = "GO";

        private ITableSchema schema;
        private TableName tableName;

        public TableClause(ITableSchema schema)
        {
            this.schema = schema;
            this.tableName = schema.TableName;
        }



        #region SELECT/INSERT/UPDATE/DELETE

        private string WHERE(string[] columns, object[] values)
        {
            string[] pk = schema.PrimaryKeys.Keys;
            var L1 = new List<ColumnPair>();
            foreach (var key in pk)
            {
                for (int i = 0; i < columns.Length; i++)
                    if (key == columns[i])
                    {
                        L1.Add(new ColumnPair(key, values[i]));
                    }
            }

            return string.Join<ColumnPair>(" AND ", L1);
        }

        public string IF_NOT_EXISTS_INSERT(string[] columns, object[] values)
        {
            string where = WHERE(columns, values);
            return string.Format(ifNotExistsInsertTemplate, where, INSERT(columns, values));
        }

        public string INSERT(DataRow row)
        {
            var direct = RowCompare.Direct(row);
            return INSERT(direct);
        }

        public string INSERT(string[] columnName, object[] values)
        {
            var direct = RowCompare.Direct(columnName, values);
            return INSERT(direct);
        }


        public string INSERT(IEnumerable<ColumnPair> pairs)
        {
            string[] ik = schema.Identity.ColumnNames;
            string[] ck = schema.Columns.Where(column => column.IsComputed).Select(column => column.ColumnName).ToArray();

            var L1 = pairs
              .Where(column => !ik.Contains(column.ColumnName))
              .Where(column => !ck.Contains(column.ColumnName));

            var x1 = L1.Select(p => p.ColumnName.ColumnName());
            var x2 = L1.Select(p => p.Value.ToScript());

            return string.Format(insertCommandTemplate,
                string.Join(",", x1),
                string.Join(",", x2)
                );
        }


        public string IF_NOT_EXISTS_INSERT_ELSE_UPDATE(string[] columns, object[] values)
        {
            string where = WHERE(columns, values);
            return string.Format(ifNotExistsInsertElseUpdateTemplate, where, INSERT(columns, values), UPDATE(columns, values));
        }


        public string UPDATE(string[] columnName, object[] values)
        {
            var direct = RowCompare.Direct(columnName, values);
            return UPDATE(direct);
        }

        public string UPDATE(IEnumerable<ColumnPair> pairs)
        {
            string[] ik = schema.Identity.ColumnNames;
            string[] pk = schema.PrimaryKeys.Keys;
            string[] ck = schema.Columns.Where(column => column.IsComputed).Select(column => column.ColumnName).ToArray();

            var L1 = pairs
                .Where(column => !ik.Contains(column.ColumnName))
                .Where(column => !pk.Contains(column.ColumnName))
                .Where(column => !ck.Contains(column.ColumnName))
                .Select(p => $"{p.ColumnName.ColumnName()} = {p.Value.ToScript()}");
            string update = string.Join(",", L1);

            var L2 = pairs.Where(x => pk.Contains(x.ColumnName));
            string where = string.Join<ColumnPair>(" AND ", L2);

            if (where == string.Empty)
            {
                throw new Exception("primary key doesn't exist");
            }

            return string.Format(updateCommandTemplate, update, where);
        }


        public string UPDATE(RowCompare compare)
        {
            return string.Format(updateCommandTemplate, compare.Set, compare.Where);
        }



        public string DELETE(DataRow row, IPrimaryKeys primaryKey)
        {
            var L1 = new List<ColumnPair>();
            foreach (var column in primaryKey.Keys)
            {
                L1.Add(new ColumnPair(column, row[column]));
            }

            return string.Format(deleteCommandTemplate, string.Join<ColumnPair>(" AND ", L1));
        }
        #endregion


        #region SELECT/UPDATE/DELETE/INSERT template

        public string SELECT(IEnumerable<IColumn> columns)
        {
            var L = columns.Select(column => "[" + column.ColumnName + "]");
            return string.Format(selectCommandTemplate, string.Join(",", L), primaryWhere(columns));
        }

        public string INSERT(IEnumerable<IColumn> columns, bool hasQuotationMark = true)
        {
            IEnumerable<string> x1 = columns.Select(column => "[" + column.ColumnName + "]");
            IEnumerable<string> x2 = columns.Select(column => ColumnValue.ToScript(column));
            if (!hasQuotationMark)
                x2 = columns.Select(column => column.ColumnName).Select(c => c.SqlParameterName());

            return string.Format(insertCommandTemplate,
             string.Join(",", x1),
             string.Join(",", x2)
             );
        }


        public string UPDATE(IEnumerable<IColumn> columns)
        {

            string[] C = columns.Where(c => !c.IsPrimary && !c.IsIdentity).Select(c => c.ColumnName).ToArray();

            var L = new List<string>();
            foreach (var c in C)
            {
                L.Add(string.Format("[{0}]={1}", c, c.SqlParameterName()));
            }

            return string.Format(updateCommandTemplate, string.Join(",", L), primaryWhere(columns));
        }

        public string INSERT_OR_UPDATE(IEnumerable<IColumn> columns)
        {
            StringBuilder builder = new StringBuilder();
            string exists = string.Format(selectCommandTemplate, "*", primaryWhere(columns));
            builder.AppendLine($"IF NOT EXISTS({exists})");
            builder.AppendLine("\t" + INSERT(schema.Columns, false));
            builder.AppendLine("ELSE");
            builder.AppendLine("\t" + UPDATE(schema.Columns));

            builder.AppendLine();
            builder.AppendLine("//C# row object");
            builder.AppendLine("var obj = new");
            builder.AppendLine("{");
            int i = 1;
            foreach (var column in columns)
            {
                Type type = column.CType.ToType();
                string VAR = column.ColumnName.SqlParameterName().Replace("@", "");
                string VAL;
                if (type == typeof(string))
                    VAL = "\"\"";
                else if (type == typeof(DateTime) || type == typeof(DateTime?))
                    VAL = "DateTime.Now";
                else if (type.IsValueType)
                    VAL = Activator.CreateInstance(type).ToString();
                else
                    VAL = "null";
                string COMMA = string.Empty;
                if (i++ < columns.Count())
                    COMMA = ",";
                var COMMENT = column.CType.GetCSharpType(column.Nullable);
                builder.AppendLine($"\t//{COMMENT}");
                builder.AppendLine($"\t{VAR} = {VAL}{COMMA}");
                if (COMMA != string.Empty)
                    builder.AppendLine();
            }

            builder.AppendLine("};");
            return builder.ToString();
        }

        public string DELETE(IEnumerable<IColumn> columns)
        {
            return string.Format(deleteCommandTemplate, primaryWhere(columns));
        }

        private string primaryWhere(IEnumerable<IColumn> columns)
        {
            string[] primaryKeys = columns.Where(c => c.IsPrimary).Select(c => c.ColumnName).ToArray();
            var L = new List<string>();
            foreach (var key in primaryKeys)
            {
                L.Add(string.Format("[{0}] = {1}", key, key.SqlParameterName()));
            }
            return string.Join(" AND ", L);
        }

        #endregion

        #region CREATE/DROP Table
        public string CREATE_TABLE()
        {
            TableSchema schema1 = new TableSchema(tableName);
            string format = TableClause.GenerateCREATE_TABLE(schema1);
            string script = string.Format(format, tableName.FormalName);
            return script;
        }

        public string DROP_TABLE(bool ifExists)
        {
            return DROP_TABLE(tableName, ifExists);
        }

        public static string DROP_TABLE(TableName tableName, bool ifExists)
        {
            if (ifExists)
            {
                var builder = new StringBuilder();
                builder.AppendLine($"IF OBJECT_ID('{tableName.FormalName}') IS NOT NULL")
                      .AppendLine($"  DROP TABLE {tableName.FormalName}")
                      .AppendLine("GO");
                return builder.ToString();
            }

            return $"DROP TABLE {tableName.FormalName}";
        }

        #endregion


        #region Add/Alter/Drop Column

        public string ADD_COLUMN(IColumn column)
        {
            if (column.Nullable)
            {
                return _ADD_COLUMN(column);
            }
            else
            {
                //add new column with type NULL
                StringBuilder builder = new StringBuilder();
                (column as ColumnSchema).Nullable = true;
                builder.AppendLine(_ADD_COLUMN(column));
                (column as ColumnSchema).Nullable = false;

                //Update Column value
                Type type = column.CType.ToType();
                string val = string.Empty;
                try
                {
                    object obj;
                    if (type == typeof(string))  //class string doesn't have default constructor
                        obj = string.Empty;
                    else
                        obj = Activator.CreateInstance(type);

                    val = new ColumnValue(obj).ToScript();
                }
                catch (Exception ex)
                {
                    throw new Exception($"doesn't support to get default value of type:{type}, {ex.Message}");
                }

                builder.AppendLine($"UPDATE {tableName.FormalName} SET {column.ColumnName} = {val}");

                //Change column type to NOT NULL
                builder.AppendLine(ALTER_COLUMN(column));
                return builder.ToString();
            }
        }
        private string _ADD_COLUMN(IColumn column)
        {
            return string.Format("ALTER TABLE {0} ADD {1}", tableName.FormalName, column.GetSQLField());
        }

        public string ALTER_COLUMN(IColumn column)
        {
            return string.Format("ALTER TABLE {0} ALTER COLUMN {1}", tableName.FormalName, column.GetSQLField());
        }

        public string DROP_COLUMN(IColumn column)
        {
            return string.Format("ALTER TABLE {0} DROP  COLUMN {1}", tableName.FormalName, column.ColumnName);
        }

        #endregion

        #region Primary Key

        public string ADD_PRIMARY_KEY(IPrimaryKeys primaryKey)
        {
            return string.Format("ALTER TABLE {0} ADD PRIMARY KEY ({1})", tableName.FormalName, string.Join(",", primaryKey.Keys));
        }

        public string DROP_PRIMARY_KEY(IPrimaryKeys primaryKey)
        {
            return string.Format("ALTER TABLE {0} DROP CONSTRAINT ({1})", tableName.FormalName, primaryKey.ConstraintName);
        }

        #endregion

        #region Foreign Key

        public string DROP_FOREIGN_KEY(IForeignKey foreignKey)
        {
            return string.Format("ALTER TABLE {0} DROP CONSTRAINT ({1})", tableName.FormalName, foreignKey.Constraint_Name);
        }

        public string ADD_FOREIGN_KEY(IForeignKey foreignKey)
        {
            string reference;
            if (foreignKey.PK_Schema != TableName.dbo)
                reference = string.Format(" [{0}].[{1}]([{2}])", foreignKey.PK_Schema, foreignKey.PK_Table, foreignKey.PK_Column);
            else
                reference = string.Format(" [{0}]([{1}])", foreignKey.PK_Table, foreignKey.PK_Column);

            return string.Format("ALTER TABLE {0} ADD CONSTRAINT [{1}] FOREIGN KEY ([{2}])\nREFERENCES {3}",
                tableName.FormalName,
                foreignKey.Constraint_Name,
                foreignKey.FK_Column,
                reference
                );
        }

        #endregion

        #region Insert/Update/Delete template

        private string ifNotExistsInsertTemplate => $@"
IF NOT EXISTS(SELECT * FROM {tableName.FormalName} WHERE {{0}})
  {{1}}";

        private string ifNotExistsInsertElseUpdateTemplate => $@"
IF NOT EXISTS(SELECT * FROM {tableName.FormalName} WHERE {{0}})
  {{1}}
ELSE 
  {{2}}";

        private string selectCommandTemplate => $"SELECT {{0}} FROM {tableName.FormalName} WHERE {{1}}";
        private string updateCommandTemplate => $"UPDATE {tableName.FormalName} SET {{0}} WHERE {{1}}";
        private string insertCommandTemplate => $"INSERT INTO {tableName.FormalName}({{0}}) VALUES({{1}})";
        private string deleteCommandTemplate => $"DELETE FROM {tableName.FormalName} WHERE {{0}}";

        #endregion


        public string GenerateScript()
        {
            string sql;
            sql = CREATE_TABLE();

            StringBuilder builder = new StringBuilder(sql);

            var fk1 = schema.ForeignKeys;
            if (fk1.Keys.Length > 0)
            {
                foreach (var fk in fk1.Keys)
                {
                    builder.AppendLine(ADD_FOREIGN_KEY(fk));
                }

            }

            sql = builder.ToString();
            return sql;
        }


        public string IF_EXISTS_DROP_TABLE()
        {
            string drop =
@"IF OBJECT_ID('{0}') IS NOT NULL
  DROP TABLE {1}
";
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(drop, tableName.FormalName, tableName.FormalName);
            return builder.ToString();
        }



        internal static string GenerateCREATE_TABLE(ITableSchema table)
        {
            string fields = string.Join(",\r\n", table.Columns.Select(column => "\t" + column.GetSQLField()));
            return CREATE_TABLE(fields, table.PrimaryKeys);

        }

        public static string CREATE_TABLE(string fields, IPrimaryKeys primary)
        {

            string primaryKey = "";
            if (primary.Length > 0)
                primaryKey = string.Format("\tPRIMARY KEY({0})", string.Join(",", primary.Keys.Select(key => string.Format("[{0}]", key))));


            string SQL = @"
CREATE TABLE {0}
(
{1}
{2}
) 
";
            return string.Format(SQL, "{0}", fields, primaryKey);
        }

    }
}
