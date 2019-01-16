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
    class XmlDbFile : DbFile
    {
        private const string EXT = "xml";

        public XmlDbFile()
        {
        }

        public override void ReadSchema(FileLink link, DataSet dbSchema)
        {
            try
            {
                link.ReadXml(dbSchema);

                if (dbSchema.Tables.Count == 0)
                    throw new Exception($"error in xml schema file: {link}");

            }
            catch (Exception)
            {
                throw new Exception($"bad data source defined {link}");
            }

        }


        public override int ReadData(FileLink root, SelectClause select, DataSet ds)
        {
            TableName tname = select.TableName;
            var file = root.PathCombine(tname.DatabaseName.Name, tname.ShortName);
            file = string.Format("{0}.{1}", file, EXT);

            var link = FileLink.CreateLink(file, tname.Provider.UserId, tname.Provider.Password);
            if (!link.Exists)
                throw new InvalidDataException($"table {tname.FormalName} data file \"{file}\" not exist");

            link.ReadXml(ds);

            if (ds.Tables.Count > 0)
                return ds.Tables[0].Rows.Count;
            else
                return -1;
        }
      
    }
}
