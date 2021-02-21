using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Data;
using Sys.Stdio;

namespace Sys.Data.Resource
{
    public class ResourceFileReader
    {
        /// <summary>
        /// remove extra space from attribute "name"
        /// </summary>
        public bool TrimPropertyName { get; set; }

        /// <summary>
        /// remove extra space from element "value"
        /// </summary>
        public bool TrimPropertyValue { get; set; }


        public ResourceFileReader()
        {
        }

        internal List<entry> ReadResx(string path)
        {
            List<entry> list = new List<entry>();
            XElement xdoc = XElement.Load(path);

            XNamespace xmlns = XNamespace.Xml;
            var elements = xdoc.Elements("data");

            int count = 0;
            foreach (XElement element in elements)
            {
                string name = ((string)element.Attribute("name"));
                string value = ((string)element.Element("value"));

                if (TrimPropertyName)
                    name = name.Trim();

                if (TrimPropertyValue)
                    value = value.Trim();

                entry token = new entry
                {
                    name = name,
                    value = value,
                };

                if (list.Select(x => x.name).Contains(token.name))
                {
                    var result = list.Find(x => x.name == token.name);
                    cerr.WriteLine($"duplicated {result}");
                    cerr.WriteLine($"duplicated {token}");
                }

                list.Add(token);
                count++;
            }

            return list;
        }
    }
}
