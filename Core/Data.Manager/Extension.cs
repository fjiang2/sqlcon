using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace Sys.Data.Manager
{
    public static class Extension
    {

        public static TableSchema GetSchema(this TableName tname)
        {
            var schema = new TableSchema(tname);
            return schema;
        }



      
    }
}
