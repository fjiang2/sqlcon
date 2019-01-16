using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace Sys.Data
{
    public class DataLake : Dictionary<string, DataSet>
    {
        public string DataLakeName { get; set; }
        public DataLake()
        {
            DataLakeName = string.Empty;
        }

        public DataSet GetDataSet(string dataSetName)
        {
            if (!this.ContainsKey(dataSetName))
                return null;

            return this[dataSetName];
        }

        public DataTable GetDataTable(string dataSetName, int tableIndex)
        {
            var ds = GetDataSet(dataSetName);
            if (ds == null)
                return null;

            if (tableIndex >= 0 && tableIndex < ds.Tables.Count)
            {
                return ds.Tables[tableIndex];
            }

            return null;
        }

        public XmlReadMode ReadXml(TextReader reader)
        {
            return ReadXml(reader, XmlReadMode.ReadSchema);
        }

        public XmlReadMode ReadXml(TextReader reader, XmlReadMode mode)
        {
            if (reader is StreamReader)
            {
                StreamReader sreader = (StreamReader)reader;
                return ReadXml(sreader.BaseStream, mode);
            }

            throw new Exception($"{nameof(DataLake)} reader({reader.GetType().FullName}) is not {nameof(StreamReader)}");
        }

        public XmlReadMode ReadXml(Stream stream, XmlReadMode mode)
        {
            XmlReader reader = XmlReader.Create(stream);
            this.Clear();
            ReadXml(reader);

            return mode;
        }

        public void ReadXml(XmlReader reader)
        {
            this.Clear();
            this.DataLakeName = string.Empty;

            bool isEmpty = reader.IsEmptyElement;
            if (isEmpty)
                return;

            reader.MoveToContent(); //Move to <DataLake>
                                    //reader.ReadStartElement(nameof(DataLake));

            if (reader.MoveToAttribute(nameof(DataLakeName)))
                this.DataLakeName = reader.ReadContentAsString();

            while (reader.Read())
            {
                if (reader.NodeType == XmlNodeType.EndElement)
                {
                    if (reader.Name == nameof(DataLake))
                        return;
                }

                if (reader.NodeType == XmlNodeType.Element)
                {

                    reader.ReadStartElement(nameof(DataSet));

                    DataSet ds = new DataSet();
                    ds.ReadXml(reader, XmlReadMode.ReadSchema);
                    this.Add(ds.DataSetName, ds);

                    reader.ReadEndElement();
                }
            }
        }

        public void WriteXml(XmlWriter writer)
        {
            writer.WriteStartElement(nameof(DataLake));
            writer.WriteAttributeString(nameof(DataLakeName), DataLakeName);

            foreach (var kvp in this)
            {
                DataSet ds = kvp.Value;
                ds.DataSetName = kvp.Key;
                writer.WriteStartElement(nameof(DataSet));
                ds.WriteXml(writer, XmlWriteMode.WriteSchema);
                writer.WriteEndElement();
            }

            writer.WriteEndElement();
        }

        public override string ToString()
        {
            return $"Name={DataLakeName}, Count={Count}";
        }

    }
}

