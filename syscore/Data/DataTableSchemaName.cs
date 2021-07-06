using System.Data;

namespace Sys.Data
{
    public class DataTableSchemaName
    {
        private DataTable dt;
        public DataTableSchemaName(DataTable dt)
        {
            this.dt = dt;
        }

        public void SetSchemaAndTableName(TableName tname)
        {
            dt.TableName = tname.Name;
            UpdateSchemaName(tname.SchemaName);

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
                return dt.Prefix == Data.SchemaName.dbo;
            }
        }
        
        public void UpdateSchemaName(string schemaName)
        {
            if (schemaName != Data.SchemaName.empty)
                dt.Prefix = schemaName;
        }

        public string SchemaName
        {
            get
            {
                if (string.IsNullOrEmpty(dt.Prefix))
                    return Data.SchemaName.empty;
                else
                    return dt.Prefix;
            }
        }

        public override string ToString()
        {
            return SchemaName;
        }
    }
}
