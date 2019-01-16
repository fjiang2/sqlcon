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

        public override int SelectData(SelectClause select, DataSet ds)
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
