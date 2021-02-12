using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Xml.Linq;
using System.Data;

namespace Sys.Data.Resource
{
    public class Resx
    {
        List<entry> entries = new List<entry>();

        public Resx()
        {

        }

        public void Load(DataTable dt, string nameColumn, string valueColumn)
        {
            entries.Clear();

            foreach (DataRow row in dt.Rows)
            {
                string name = row.GetField<string>(nameColumn).Trim();
                string value = row.GetField<string>(valueColumn).Trim();

                if (name == string.Empty)
                    continue;

                entries.Add(new entry { name = name, value = value });
            }
        }

        public int UpdateResx(string path, bool append = true)
        {
            if (!File.Exists(path))
                throw new FileNotFoundException(path);

            XElement xdoc = XElement.Load(path);

            if (!append)
            {
                //remove all existing <data>
                xdoc.Elements().Where(el => el.Name == "data").Remove();
            }

            int count = 0;

            XNamespace xmlns = XNamespace.Xml;
            foreach (var item in entries)
            {
                XElement elemnt = new XElement("data",
                    new XAttribute("name", item.name),
                    new XAttribute(xmlns + "space", "preserve"),
                    new XElement("value", item.value)
                );

                xdoc.Add(elemnt);
                count++;
            }

            xdoc.Save(path, SaveOptions.OmitDuplicateNamespaces);
            
            return count;
        }
    }
}