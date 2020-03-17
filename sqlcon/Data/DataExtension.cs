﻿using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Sys.Data;
using Sys.Stdio;

namespace sqlcon
{
    public static class DataExtension
    {
        public static DataSet ReadDataSet(this string path)
        {
            string ext = Path.GetExtension(path);
            try
            {
                DataSet ds = new DataSet();
                switch (ext)
                {
                    case ".json":
                        string json = File.ReadAllText(path);
                        ds.ReadJson(json);
                        return ds;

                    case ".xml":
                        ds.ReadXml(path, XmlReadMode.ReadSchema);
                        return ds;
                }
            }
            catch (Exception ex)
            {
                cerr.WriteLine($"invalid data set file: {path}, {ex.Message}");
            }

            return null;
        }

        public static void WriteDataSet(this string path, DataSet ds)
        {
            string directory = Path.GetDirectoryName(path);
            if (directory != string.Empty)
            {
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);
            }

            if (Path.GetExtension(path) == string.Empty)
                path = Path.ChangeExtension(path, ".xml");

            if (Path.GetExtension(path) == ".json")
            {
                string json = ds.WriteJson(JsonStyle.Normal);
                File.WriteAllText(path, json);
            }
            else
            {
                ds.WriteXml(path, XmlWriteMode.WriteSchema);
            }
        }


    }
}
