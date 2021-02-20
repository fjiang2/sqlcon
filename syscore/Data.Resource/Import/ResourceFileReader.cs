using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Data;

namespace Sys.Data.Resource
{
    public class ResourceFileReader
    {
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
                entry token = new entry
                {
                    name = ((string)element.Attribute("name")),
                    value = ((string)element.Element("value")),
                };

                if (list.Select(x => x.name).Contains(token.name))
                {
                    var result = list.Find(x => x.name == token.name);
                    Console.WriteLine($"duplicated {result}");
                    Console.WriteLine($"duplicated {token}");
                }

                list.Add(token);
                count++;
            }

            Console.WriteLine($"Completed {path}");
            return list;
        }
    }
}
