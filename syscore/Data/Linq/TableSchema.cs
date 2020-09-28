﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data.Linq
{
    class TableSchema : ITableSchema
    {
        public string TableName { get; set; }
        public string[] PrimaryKeys { get; set; }
        public string[] IdentityKeys { get; set; }

        public override string ToString()
        {
            return $"{TableName}, pk=({string.Join(",", PrimaryKeys)})";
        }
    }

}
