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
    public class XmlDbFile : DbFile
    {
        public string XmlDbFolder { get; set; } = "db";
        private const string EXT = "xml";

        public XmlDbFile()
        {
        }

        private string getPath(ServerName sname) => string.Format("{0}\\{1}", XmlDbFolder, sname.Path);

        private string getPath(DatabaseName dname) => string.Format("{0}\\{1}", getPath(dname.ServerName), dname.Name);

        private string getDataFileName(TableName tname) => string.Format("{0}\\{1}.{2}", getPath(tname.DatabaseName), tname.ShortName, EXT);

        private string getSchemaFilName(ServerName sname) => string.Format("{0}\\{1}.{2}", getPath(sname), sname.Path, EXT);

        private string getSchemaFilName(DatabaseName dname) => string.Format("{0}\\{1}.{2}", getPath(dname.ServerName), dname.Name, EXT);

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


        public override string WriteSchema(ServerName sname)
        {
            var file = getSchemaFilName(sname);
            using (var writer = NewStreamWriter(file))
            {
                DataSet ds = sname.ServerSchema();
                ds.WriteXml(writer, XmlWriteMode.WriteSchema);
            }

            return file;
        }

        public override string WriteSchema(DatabaseName dname)
        {
            var file = getSchemaFilName(dname);
            using (var writer = NewStreamWriter(file))
            {
                DataTable dt = dname.DatabaseSchema();
                dt.WriteXml(writer, XmlWriteMode.WriteSchema);
            }
            return file;
        }


        public override string WriteData(TableName tname, DataTable dt)
        {
            string file = getDataFileName(tname);
            using (var writer = NewStreamWriter(file))
            {
                dt.TableName = tname.Name;
                dt.DataSet.DataSetName = tname.DatabaseName.Name;
                dt.WriteXml(writer, XmlWriteMode.WriteSchema);
            }

            return file;
        }


        public override int ReadData(FileLink root, TableName tname, DataSet ds, string where)
        {
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


        private static StreamWriter NewStreamWriter(string fileName)
        {
            try
            {
                string folder = Path.GetDirectoryName(fileName);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
            }
            catch (ArgumentException)
            {
            }

            return new StreamWriter(fileName);
        }
    }
}
