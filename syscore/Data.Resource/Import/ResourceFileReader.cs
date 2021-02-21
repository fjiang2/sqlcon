using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Data;
using System.IO;
using Tie;
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

        internal List<entry> Read(ResourceFormat format, string path)
        {
            switch (format)
            {
                case ResourceFormat.resx:
                    return ReadResx(path);

                case ResourceFormat.json:
                    return ReadJson(path);
            }

            return new List<entry>();
        }

        private List<entry> ReadResx(string path)
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

                entry entry = new entry
                {
                    name = name,
                    value = value,
                };

                if (list.Select(x => x.name).Contains(entry.name))
                {
                    var result = list.Find(x => x.name == entry.name);
                    cerr.WriteLine($"duplicated {result}");
                    cerr.WriteLine($"duplicated {entry}");
                }

                list.Add(entry);
                count++;
            }

            return list;
        }

        private List<entry> ReadJson(string path)
        {
            List<entry> list = new List<entry>();
            string json = File.ReadAllText(path);
            VAL val = Script.Evaluate(json);
            foreach (var item in val)
            {
                entry entry = new entry
                {
                    name = (string)item[0],
                    value = (string)item[1],
                };

                list.Add(entry);
            }

            return list;
        }
    }
}
