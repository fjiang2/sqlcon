using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using Sys.Networking;

namespace Sys.IO
{
    public class FtpFileLink : FileLink
    {
        private Uri uri;
        private FtpClient client;

        public FtpFileLink(string url, string userName, string password)
            : base(url)
        {
            this.uri = new Uri(url);
            this.client = new FtpClient(uri.Host, uri.Port)
            {
                UserName = userName,
                Password = password,
                RemotePath = remotePath
            };
        }

        private string fileName
        {
            get
            {
                return uri.Segments.Last();
            }
        }

        private string remotePath
        {
            get
            {
                var items = uri.AbsolutePath.Split('/');
                return string.Join("/", items.Take(items.Length - 1));
            }
        }
        protected override bool exists()
        {
            var names = client.GetFileNames();
            return names.FirstOrDefault(name => name.ToUpper() == fileName.ToUpper()) != null;
        }


        public override string PathCombine(string path1, string path2)
        {
            var items = url.Split('/');
            var root = string.Join("/", items.Take(items.Length - 1));
            return string.Format("{0}/{1}/{2}", root, path1, path2);
        }


        public override DataSet ReadXml(DataSet ds)
        {
            client.Download(fileName, reader => ds.ReadXml(reader, XmlReadMode.ReadSchema));
            return ds;
        }

        public override string ReadAllText()
        {
            string temp = Path.GetTempFileName();
            client.Download(fileName, temp);

            return File.ReadAllText(temp);
        }
    }
}
