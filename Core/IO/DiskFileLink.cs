using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Data;

namespace Sys.IO
{
    public class DiskFileLink : FileLink
    {
     
        public DiskFileLink(string url)
            :base(url)
        {
            this.url = url;
        }

        protected override bool exists()
        {
            return File.Exists(fileName);
        }


        private string fileName
        {
            get
            {
                if (url.StartsWith("file://"))
                    return url.Substring(7);
                else
                    return url;
            }
        }


        public override string PathCombine(string path1, string path2)
        {
            string root = Path.GetDirectoryName(fileName);
            return "file://" + Path.Combine(root, path1, path2);
        }



        public override DataSet ReadXml(DataSet ds)
        {
            using (var reader = new StreamReader(fileName))
            {
                ds.ReadXml(reader);
            }
            return ds;
        }
        
    }
}
