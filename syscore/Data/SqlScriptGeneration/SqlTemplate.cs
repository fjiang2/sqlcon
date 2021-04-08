using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data
{

    class SqlTemplate
    {
        private string formalName;
        private SqlTemplateFormat format = SqlTemplateFormat.SingleLine;
        private string NewLine = string.Empty;

        public SqlTemplate(TableName tableName)
        {
            this.formalName = tableName.FormalName;
        }

        public SqlTemplate(string formalName)
        {
            this.formalName = formalName;
        }


        private SqlTemplateFormat Format
        {
            get
            {
                return format;
            }
            set
            {
                format = value;
                switch (format)
                {
                    case SqlTemplateFormat.Indent:
                        NewLine = Environment.NewLine + "  ";
                        break;

                    default:
                        NewLine = string.Empty;
                        break;
                }
            }
        }

        private string IfNotExists(string where)
            => $"IF NOT EXISTS(SELECT * FROM {formalName} WHERE {where})";
        private string IfExists(string where)
            => $"IF EXISTS(SELECT * FROM {formalName} WHERE {where})";

        public string IfNotExistsInsert(string where, string insert)
            => $"{IfNotExists(where)} {NewLine}{insert}";
        public string IfNotExistsInsertElseUpdate(string where, string insert, string update)
            => $@"{IfNotExists(where)} {NewLine}{insert} {NewLine}ELSE {NewLine}{update}";

        public string IfExistsUpdate(string where, string update)
            => $"{IfExists(where)} {NewLine}{update}";
        public string IfExistsUpdateElseInsert(string where, string update, string insert)
            => $"{IfExists(where)} {NewLine}{update} {NewLine}ELSE {NewLine}{insert}";

        public string Select(string select)
            => $"SELECT {select} {NewLine}FROM {formalName}";
        public string Select(string select, string where)
            => $"{Select(select)} {NewLine}WHERE {where}";

        public string Update(string set)
            => $"UPDATE {formalName} {NewLine}SET {set}";
        public string Update(string set, string where)
            => $"{Update(set)} {NewLine}WHERE {where}";

        public string Insert(string values)
            => $"INSERT INTO {formalName} VALUES({values})";
        public string Insert(string columns, string values)
            => $"INSERT INTO {formalName}({columns}) {NewLine}VALUES({values})";
        public string Insert(string columns, string values, string identity)
            => $"{Insert(columns, values)} {NewLine}{identity}";
        public string InsertWithIdentityOff(string columns, string values)
            => $"SET IDENTITY_INSERT {formalName} ON; {Insert(columns, values)}; SET IDENTITY_INSERT {formalName} OFF";

        public string Delete()
            => $"DELETE FROM {formalName}";
        public string Delete(string where)
            => $"{Delete()} {NewLine}WHERE {where}";


        public string AddPrimaryKey(string primaryKey)
            => $"ALTER TABLE {formalName} ADD PRIMARY KEY ({primaryKey})";
        public string DropPrimaryKey(string constraintName)
            => $"ALTER TABLE {formalName} DROP CONSTRAINT ({constraintName})";

        public string DropForeignKey(string constraintName)
            => $"ALTER TABLE {formalName} DROP CONSTRAINT ({constraintName})";

        public string AddColumn(string column)
            => $"ALTER TABLE {formalName} ADD {column}";

        public string AddColumn(string column, object defaultValue)
        {
            string value = new SqlValue(defaultValue).ToString("N");
            return $"ALTER TABLE {formalName} ADD {column} DEFAULT({value})";
        }

        public string AlterColumn(string column)
            => $"ALTER TABLE {formalName} ALTER COLUMN {column}";
        public string DropColumn(string column)
            => $"ALTER TABLE {formalName} DROP COLUMN {column}";


        public string DropTable(bool ifExists)
        {
            if (ifExists)
            {
                var builder = new StringBuilder();
                builder.AppendLine($"IF OBJECT_ID('{formalName}') IS NOT NULL")
                      .AppendLine($"  DROP TABLE {formalName}");
                return builder.ToString();
            }

            return $"DROP TABLE {formalName}";
        }

        public override string ToString()
        {
            return formalName;
        }
    }
}
