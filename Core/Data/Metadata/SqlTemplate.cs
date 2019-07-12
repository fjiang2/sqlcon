using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data
{
    enum SqlTemplateFormat
    {
        SingleLine,
        Indent,
    }

    class SqlTemplate
    {
        private TableName tableName;
        private SqlTemplateFormat format = SqlTemplateFormat.SingleLine;
        private string NewLine = string.Empty;

        public SqlTemplate(TableName tableName)
        {
            this.tableName = tableName;
        }

        private string tname => tableName.FormalName;

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


        public string IfNotExistsInsert(string where, string insert) 
            => $"IF NOT EXISTS(SELECT * FROM {tname} WHERE {where}) {NewLine}{insert}";
        public string IfNotExistsInsertElseUpdate(string where, string insert, string update) 
            => $@"IF NOT EXISTS(SELECT * FROM {tname} WHERE {where}) {NewLine}{insert} {NewLine}ELSE {NewLine}{update}";

        public string IfExistsUpdate(string where, string update) 
            => $"IF EXISTS(SELECT * FROM {tname} WHERE {where}) {NewLine}{update}";
        public string IfExistsUpdateElseInsert(string where, string update, string insert) 
            => $"IF EXISTS(SELECT * FROM {tname} WHERE {where}) {update} ELSE {insert}";

        public string Select(string select) 
            => $"SELECT {select} {NewLine}FROM {tname}";
        public string Select(string select, string where) 
            => $"SELECT {select} {NewLine}FROM {tname} {NewLine}WHERE {where}";

        public string Update(string set, string where) 
            => $"UPDATE {tname} {NewLine}SET {set} {NewLine}WHERE {where}";
        public string Insert(string columns, string values) 
            => $"INSERT INTO {tname}({columns}) {NewLine}VALUES({values})";

        public string Delete(string where) 
            => $"DELETE FROM {tname} {NewLine}WHERE {where}";
        public string Delete() 
            => $"DELETE FROM {tname} {NewLine}";


        public string AddPrimaryKey(string primaryKey) 
            => $"ALTER TABLE {tname} ADD PRIMARY KEY ({primaryKey})";
        public string DropPrimaryKey(string constraintName) 
            => $"ALTER TABLE {tname} DROP CONSTRAINT ({constraintName})";

        public string DropForeignKey(string constraintName) 
            => $"ALTER TABLE {tname} DROP CONSTRAINT ({constraintName})";

        public string AddColumn(string column) 
            => $"ALTER TABLE {tname} ADD {column}";
        public string AlterColumn(string column) 
            => $"ALTER TABLE {tname} ALTER COLUMN {column}";
        public string DropColumn(string column) 
            => $"ALTER TABLE {tname} DROP COLUMN {column}";


        public string DropTable(bool ifExists)
        {
            if (ifExists)
            {
                var builder = new StringBuilder();
                builder.AppendLine($"IF OBJECT_ID('{tname}') IS NOT NULL")
                      .AppendLine($"  DROP TABLE {tname}")
                      .AppendLine("GO");
                return builder.ToString();
            }

            return $"DROP TABLE {tname}";
        }

        public override string ToString()
        {
            return tableName.ToString();
        }
    }
}
