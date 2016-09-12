using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace Sys.IO
{
    public abstract class FileLink
    {
      
        protected string url;

        protected FileLink(string url)
        {
            this.url = url;
        }

        public bool Exists
        {
            get { return exists(); }
        }

        protected abstract bool exists();

        public abstract string PathCombine(string path1, string path2);

        public DataSet ReadXml()
        {
            DataSet ds = new DataSet();
            return ReadXml(ds);
        }

        public abstract DataSet ReadXml(DataSet ds);

        public abstract string ReadAllText();

        public override string ToString()
        {
            return this.url;
        }


        public static FileLink Factory(string url, string userName, string password)
        {
            FileLink link = null;

            if (url.StartsWith("file://"))
                link = new DiskFileLink(url);
            else if (url.StartsWith("http://"))
                link = new HttpFileLink(url);
            else if (url.StartsWith("https://"))
                link = new HttpFileLink(url);
            else if (url.StartsWith("ftp://"))
                link = new FtpFileLink(url, userName, password);
            else
                link = new DiskFileLink(url);

            return link;
        }
    }
}
