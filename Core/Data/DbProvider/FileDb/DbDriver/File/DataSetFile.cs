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

        public override int ReadData(FileLink link, SelectClause select, DataSet ds)
        {
            TableName tname = select.TableName;
            if (!data.Tables.Contains(tname.ShortName))
                return -1;

            DataTable dt = data.Tables[tname.ShortName];
            ds.Clear();

            DataView dv = new DataView(dt)
            {
                RowFilter = select.Where,
            };

            DataTable dt2;
            if (select.Columns != null)
                dt2 = dv.ToTable(dt.TableName, false, select.Columns);
            else
                dt2 = dv.ToTable(dt.TableName);

            ds.Tables.Add(dt2);
            return dt2.Rows.Count;
        }

    }
}
