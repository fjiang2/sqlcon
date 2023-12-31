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

        public XmlDbFile(FileLink link)
            : base(link)
        {
        }

        public override void ReadSchema(DataSet dbSchema)
        {
            try
            {
                fileLink.ReadXml(dbSchema);

                if (dbSchema.Tables.Count == 0)
                    throw new Exception($"error in xml schema file: {fileLink}");

            }
            catch (Exception ex)
            {
                throw new Exception($"bad data source defined {fileLink}, {ex.Message}");
            }

        }


        public override int SelectData(SelectClause select, DataSet ds)
        {
            TableName tname = select.TableName;
            var file = fileLink.PathCombine(tname.DatabaseName.Name, tname.ShortName);
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
