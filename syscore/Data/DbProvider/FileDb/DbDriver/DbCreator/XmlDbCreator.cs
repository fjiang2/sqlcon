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
    public class XmlDbCreator : IDbCreator
    {
        public string XmlDbFolder { get; set; } = "db";
        private const string EXT = "xml";

        public XmlDbCreator()
        {
        }

        private string getPath(ServerName sname) => string.Format("{0}\\{1}", XmlDbFolder, sname.Path);

        private string getPath(DatabaseName dname) => string.Format("{0}\\{1}", getPath(dname.ServerName), dname.Name);

        private string getDataFileName(TableName tname) => string.Format("{0}\\{1}.{2}", getPath(tname.DatabaseName), tname.ShortName, EXT);

        private string getSchemaFilName(ServerName sname) => string.Format("{0}\\{1}.{2}", getPath(sname), sname.Path, EXT);

        private string getSchemaFilName(DatabaseName dname) => string.Format("{0}\\{1}.{2}", getPath(dname.ServerName), dname.Name, EXT);


        public string WriteSchema(ServerName sname)
        {
            var file = getSchemaFilName(sname);
            using (var writer = NewStreamWriter(file))
            {
                DataSet ds = sname.ServerSchema();
                ds.WriteXml(writer, XmlWriteMode.WriteSchema);
            }

            return file;
        }

        public string WriteSchema(DatabaseName dname)
        {
            var file = getSchemaFilName(dname);
            using (var writer = NewStreamWriter(file))
            {
                DataTable dt = dname.DatabaseSchema();
                dt.WriteXml(writer, XmlWriteMode.WriteSchema);
            }
            return file;
        }


        public string WriteData(TableName tname, DataTable dt)
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
