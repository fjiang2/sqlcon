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

        internal List<Entry> Read(ResourceFormat format, string path)
        {
            switch (format)
            {
                case ResourceFormat.resx:
                    return ReadResx(path);

                case ResourceFormat.json:
                    return ReadJson(path);

                case ResourceFormat.xlf:
                    return ReadXlf(path);
            }

            return new List<Entry>();
        }

        private List<Entry> ReadResx(string path)
        {
            List<Entry> list = new List<Entry>();
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

                Entry entry = new Entry
                {
                    name = name,
                    value = value,
                };

                if (list.Select(x => x.name).Contains(entry.name))
                {
                    var result = list.Find(x => x.name == entry.name);
                    Cerr.WriteLine($"duplicated in resx: {result}");
                    Cerr.WriteLine($"duplicated in resx: {entry}");
                }

                list.Add(entry);
                count++;
            }

            return list;
        }

        private List<Entry> ReadJson(string path)
        {
            List<Entry> list = new List<Entry>();
            string json = File.ReadAllText(path);
            VAL val = Script.Evaluate(json);
            foreach (var item in val)
            {
                Entry entry = new Entry
                {
                    name = (string)item[0],
                    value = (string)item[1],
                };

                list.Add(entry);
            }

            return list;
        }

        private List<Entry> ReadXlf(string path)
        {
            List<Entry> list = new List<Entry>();

            XNamespace xmlns = "urn:oasis:names:tc:xliff:document:1.2";

            XElement xdoc = XElement.Load(path);
            var units = xdoc.Element(xmlns + "file").Element(xmlns + "body").Elements();

            foreach (XElement unit in units)
            {
                XElement source = unit.Element(xmlns + "source");
                XElement target = unit.Element(xmlns + "target");

                if (source == null)
                {
                    Cerr.WriteLine($"cannot find <source>");
                    continue;
                }

                if (target == null)
                {
                    Cerr.WriteLine($"cannot find <target> in {source}");
                    continue;
                }

                Entry entry = new Entry();
                if (source.Elements().Any())
                {
                    //todo: when source has children elements
                }
                else
                {
                    entry.name = source.Value;
                }


                if (target.Elements().Any())
                {
                    //todo: when target has children elements
                }
                else
                {
                    entry.value = target.Value;
                }


                list.Add(entry);
            }

            return list;

        }
    }
}
