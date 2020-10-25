using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data
{
    public class TableSchemaCache
    {
        private static Dictionary<TableName, TableSchema> schemas = new Dictionary<TableName, TableSchema>();

        public static TableSchema GetSchema(TableName tname)
        {
            if (!schemas.ContainsKey(tname))
                schemas.Add(tname, new TableSchema(tname));

            return schemas[tname];
        }

        public static void Clear()
        {
            schemas.Clear();
        }
    }

}
