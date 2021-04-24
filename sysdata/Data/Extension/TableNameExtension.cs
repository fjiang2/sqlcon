using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data
{
    public static class TableNameExtension
    {

        public static string GenerateIfDropClause(this TableName tname)
        {
            TableSchema schema = new TableSchema(tname);
            var script = new TableClause(schema);

            StringBuilder builder = new StringBuilder();
            builder.Append(script.IF_EXISTS_DROP_TABLE())
                .AppendLine(SqlScript.GO);

            return builder.ToString();
        }


        public static string GenerateCreateTableClause(this TableName tname, bool appendGO)
        {
            TableSchema schema = new TableSchema(tname);
            var script = new TableClause(schema);

            string SQL = script.GenerateCreateTableScript();
            if (!appendGO)
                return SQL;
            else
                return new StringBuilder(SQL).AppendLine(SqlScript.GO).ToString();
        }
    }
}
