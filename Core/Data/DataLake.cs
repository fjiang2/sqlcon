using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Sys.Data
{
    public class DataLake : Dictionary<string, DataSet>
    {
        public string DataLakeName { get; set; }
        public DataLake()
        {
            DataLakeName = nameof(DataLake);
        }

        public DataSet GetDataSet(string dataSetName)
        {
            if (!this.ContainsKey(dataSetName))
                return null;

            return this[dataSetName];
        }

        public DataTable GetDataTable(string dataSetName, int tableIndex)
        {
            var ds = GetDataSet(dataSetName);
            if (ds == null)
                return null;

            if (tableIndex >= 0 && tableIndex < ds.Tables.Count)
            {
                return ds.Tables[tableIndex];
            }

            return null;
        }

        public override string ToString()
        {
            return $"Name={DataLakeName}, Count={Count}";
        }

    }
}
