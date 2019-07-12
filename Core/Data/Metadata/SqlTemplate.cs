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
        private string delimiter = string.Empty;

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
                        delimiter = Environment.NewLine + "  ";
                        break;

                    default:
                        delimiter = string.Empty;
                        break;
                }
            }
        }

        
        public string IfNotExistsInsert(string where, string insert) => $@"
IF NOT EXISTS(SELECT * FROM {tname} WHERE {where})
  {insert}";

        public string IfNotExistsInsertElseUpdate(string where, string insert, string update) => $@"
IF NOT EXISTS(SELECT * FROM {tname} WHERE {where})
  {insert}
ELSE 
  {update}";

        public string Select(string select) => $"SELECT {select} {delimiter}FROM {tname}";
        public string Select(string select, string where) => $"SELECT {select} {delimiter}FROM {tname} {delimiter}WHERE {where}";
        public string Update(string set, string where) => $"UPDATE {tname} {delimiter}SET {set} {delimiter}WHERE {where}";
        public string Insert(string columns, string values) => $"INSERT INTO {tname}({columns}) {delimiter}VALUES({values})";
        public string Delete(string where) => $"DELETE FROM {tname} {delimiter}WHERE {where}";

        public override string ToString()
        {
            return tableName.ToString();
        }
    }
}
