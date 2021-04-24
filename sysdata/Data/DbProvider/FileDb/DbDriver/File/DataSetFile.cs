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
    class DataSetFile : DbFile
    {
        DataSet data = new DataSet();

        public DataSetFile(FileLink link)
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
            catch (Exception)
            {
                throw new Exception($"bad data source defined {fileLink}");
            }

        }

        public override int SelectData(SelectClause select, DataSet result)
        {
            return QueryData(data, select, result);
        }

    }
}
