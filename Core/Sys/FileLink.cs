using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;

namespace Sys
{
    public class FileLink
    {
        enum LinkType
        {
            File,
            Ftp,
            Http,
            Https
        }

        LinkType link = LinkType.File;
        string url;

        public FileLink(string url)
        {
            this.url = url;

            if (url.StartsWith("file://"))
                link = LinkType.File;
            else if (url.StartsWith("http://"))
                link = LinkType.Http;
            else if (url.StartsWith("https://"))
                link = LinkType.Https;
            else if (url.StartsWith("ftp://"))
                link = LinkType.Ftp;
        }

        public bool Exists
        {
            get { return exists(); }
        }

        private bool exists()
        {
            switch (link)
            {
                case LinkType.File:
                    return File.Exists(fileName);

                case LinkType.Http:
                case LinkType.Https:
                    return HttpRequest.Exists(new Uri(url));

                case LinkType.Ftp:
                    return false;
            }

            return false;
        }


        private string fileName
        {
            get
            {
                if (link == LinkType.File)
                {
                    if (url.StartsWith("file://"))
                        return url.Substring(7);
                    else
                        return url;
                }

                throw new Exception("it isn't local file name");
            }
        }
        public string PathCombine(string path1, string path2)
        {
            string root;

            switch (link)
            {
                case LinkType.File:
                    root = Path.GetDirectoryName(fileName);
                    return "file://" + Path.Combine(root, path1, path2);

                case LinkType.Http:
                case LinkType.Https:
                case LinkType.Ftp:
                    var items = url.Split('/');
                    root = string.Join("/", items.Take(items.Length - 1));
                    return string.Format("{0}/{1}/{2}", root, path1, path2);
            }

            return null;
        }

        public DataSet ReadXml()
        {
            DataSet ds = new DataSet();
            return ReadXml(ds);
        }

        public DataSet ReadXml(DataSet ds)
        {
            switch (link)
            {
                case LinkType.File:
                    using (var reader = new StreamReader(fileName))
                    {
                        ds.ReadXml(reader);
                    }
                    break;

                case LinkType.Http:
                case LinkType.Https:
                    ds = HttpRequest.ReadXml(new Uri(url));
                    break;

                case LinkType.Ftp:
                    break;
            }

            return ds;
        }

    }
}
