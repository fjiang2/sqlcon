using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using Sys.Networking;

namespace Sys.Data.IO
{
    class FtpFileLink : FileLink
    {
        private Uri uri;
        private FtpClient client;

        public FtpFileLink(string url, string userName, string password)
            : base(url)
        {
            this.uri = new Uri(url);

            if (this.uri.UserInfo != string.Empty && string.IsNullOrEmpty(userName) && string.IsNullOrEmpty(password))
            {
                string[] items = this.uri.UserInfo.Split(':');
                if (items.Length >= 1)
                    userName = items[0];

                if (items.Length >= 2)
                    password = items[1];
            }

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


        public override void ReadXml(DataSet ds)
        {
            client.Download(fileName, reader => ds.ReadXml(reader, XmlReadMode.ReadSchema));
        }

        public override string ReadAllText()
        {
            string temp = Path.GetTempFileName();
            client.Download(fileName, temp);

            return File.ReadAllText(temp);
        }

        public override void Save(string contents)
        {
            string temp = Path.GetTempFileName();
            File.WriteAllText(temp, contents);
            client.Upload(fileName, temp);
        }
    }
}
