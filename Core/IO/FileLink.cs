using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace Sys.IO
{
    /// <summary>
    /// File Link is file pointing to disk, web or ftp site
    /// it supports http, https, ftp,  networking drive and local file
    /// </summary>
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

        public abstract void ReadXml(DataSet ds);

        public abstract string ReadAllText();

        public override string ToString()
        {
            return this.url;
        }


        public static FileLink CreateLink(string url, string userName, string password)
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
