using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys.Data.IO;

namespace Sys.Data
{
    class DataLakeFile : DbFile
    {
        DataLake data = new DataLake();

        public DataLakeFile(FileLink link)
            : base(link)
        {
        }

        public override void ReadSchema(DataSet dbSchema)
        {
            try
            {
                fileLink.ReadXml(data);

                var schema = new DbSchemaBuilder(dbSchema);
                schema.AddSchema(data);

                if (dbSchema.Tables.Count == 0)
                    throw new Exception($"error in creating schema: {fileLink}");

            }
            catch (Exception ex)
            {
                throw new Exception($"bad data source defined {fileLink}, {ex.Message}");
            }

        }

        public override int SelectData(SelectClause select, DataSet result)
        {
            string key = select.TableName.DatabaseName.Name;

            if (!data.ContainsKey(key))
                return -1;

            DataSet _ds = data[key];
            return QueryData(_ds, select, result);
        }

    }
}
