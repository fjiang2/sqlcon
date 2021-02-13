using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Sys.Data
{
    public class SchemaName
    {
        private DataTable dt;
        public SchemaName(DataTable dt)
        {
            this.dt = dt;
        }

        public void SetSchemaAndTableName(TableName tname)
        {
            dt.TableName = tname.Name;

            if (tname.SchemaName != TableName.dbo)
                dt.Prefix = tname.SchemaName;

            if (dt.DataSet == null)
            {
                DataSet ds = new DataSet();
                ds.Tables.Add(dt);
            }

            dt.DataSet.DataSetName = tname.DatabaseName.Name;
        }

        public bool IsDbo
        {
            get
            {
                if (string.IsNullOrEmpty(dt.Prefix))
                    return true;

                return dt.Prefix == TableName.dbo;
            }
        }

        public string Name
        {
            get
            {
                if (string.IsNullOrEmpty(dt.Prefix))
                    return TableName.dbo;
                else
                    return dt.Prefix;
            }
        }

        public override string ToString()
        {
            return Name;
        }
    }
}
