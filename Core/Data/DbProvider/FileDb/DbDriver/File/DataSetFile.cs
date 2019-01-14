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
    public class DataSetFile : DbFile
    {
        private const string EXT = "xml";
        DataSet data = new DataSet();

        public DataSetFile()
        {
        }

        public override void ReadSchema(FileLink link, DataSet dbSchema)
        {
            try
            {
                link.ReadXml(data);

                var schema = new DbSchemaBuilder(dbSchema);
                schema.AddSchema(data);

                if (dbSchema.Tables.Count == 0)
                    throw new Exception($"error in creating schema: {link}");

            }
            catch (Exception)
            {
                throw new Exception($"bad data source defined {link}");
            }

        }

        public override int ReadData(FileLink root, TableName tname, DataSet ds)
        {
            if (data.Tables.Contains(tname.ShortName))
            {
                DataTable dt = data.Tables[tname.ShortName];

                ds.Clear();
                ds.Tables.Add(dt.Copy());
                return dt.Rows.Count;
            }

            return -1;
        }

    }
}
