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
            DataLakeName = nameof(DataLake);
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
            using (XmlReader reader = XmlReader.Create(stream))
            {
                this.Clear();

                while (reader.Read())
                {
                    if (reader.IsStartElement())
                    {
                        this.DataLakeName = reader.Name;
                    }

                    DataSet ds = new DataSet();
                    ds.ReadXml(reader, mode);
                    this.Add(ds.DataSetName, ds);
                }
            }

            return mode;
        }

        public override string ToString()
        {
            return $"Name={DataLakeName}, Count={Count}";
        }

    }
}
