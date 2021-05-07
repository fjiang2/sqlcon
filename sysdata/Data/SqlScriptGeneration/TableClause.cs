using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq;
using System.Text;

namespace Sys.Data
{
    public class TableClause
    {
        private ITableSchema schema;
        private TableName tableName;
        private SqlTemplate template;

        public TableClause(ITableSchema schema)
        {
            this.schema = schema;
            this.tableName = schema.TableName;
            this.template = new SqlTemplate(tableName);
        }

        #region SELECT/UPDATE/DELETE/INSERT template

        public string SELECT(IEnumerable<IColumn> columns)
        {
            var L = columns.Select(column => "[" + column.ColumnName + "]");
            return template.Select(string.Join(",", L), primaryWhere(columns));
        }

        public string INSERT(IEnumerable<IColumn> columns, bool hasQuotationMark = true)
        {
            IEnumerable<string> x1 = columns.Select(column => "[" + column.ColumnName + "]");
            IEnumerable<string> x2 = columns.Select(column => ColumnValue.ToScript(column));
            if (!hasQuotationMark)
                x2 = columns.Select(column => column.ColumnName).Select(c => c.SqlParameterName());

            return template.Insert(string.Join(",", x1), string.Join(",", x2));
        }


        public string UPDATE(IEnumerable<IColumn> columns)
        {

            string[] C = columns.Where(c => !c.IsPrimary && !c.IsIdentity).Select(c => c.ColumnName).ToArray();

            var L = new List<string>();
            foreach (var c in C)
            {
                L.Add(string.Format("[{0}]={1}", c, c.SqlParameterName()));
            }

            return template.Update(string.Join(",", L), primaryWhere(columns));
        }

        public string INSERT_OR_UPDATE(IEnumerable<IColumn> columns)
        {
            StringBuilder builder = new StringBuilder();
            string exists = template.Select("*", primaryWhere(columns));
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
            return template.Delete(primaryWhere(columns));
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
        private string CREATE_TABLE()
        {
            string format = TableClause.GenerateCREATE_TABLE(schema);
            string script = string.Format(format, tableName.FormalName);
            return script;
        }

        public string DROP_TABLE(bool ifExists)
        {
            return template.DropTable(ifExists);
        }

        public string IF_EXISTS_DROP_TABLE()
        {
            return template.DropTable(ifExists: true);
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
            return template.AddColumn(column.GetSQLField());
        }

        public string ALTER_COLUMN(IColumn column)
        {
            return template.AlterColumn(column.GetSQLField());
        }

        public string DROP_COLUMN(IColumn column)
        {
            return template.DropColumn(column.ColumnName.ToString());
        }

        #endregion

        #region Primary Key

        public string ADD_PRIMARY_KEY(IPrimaryKeys primaryKey)
        {
            return template.AddPrimaryKey(string.Join(",", primaryKey.Keys));
        }

        public string DROP_PRIMARY_KEY(IPrimaryKeys primaryKey)
        {
            return template.DropPrimaryKey(primaryKey.ConstraintName);
        }

        #endregion

        #region Foreign Key

        public string DROP_FOREIGN_KEY(IForeignKey foreignKey)
        {
            return template.DropForeignKey(foreignKey.Constraint_Name);
        }

        public string ADD_FOREIGN_KEY(IForeignKey foreignKey)
        {
            string reference;
            if (foreignKey.PK_Schema != SchemaName.dbo)
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


        public string GenerateCreateTableScript()
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




        public static string GenerateCREATE_TABLE(ITableSchema table)
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
