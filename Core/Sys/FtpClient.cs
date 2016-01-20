using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using System.IO;
using System.Diagnostics.Contracts;
using System.Text.RegularExpressions;

namespace Sys
{
    public class FtpClient  
    {
        enum Method
        {
            Upload,
            Download,
            Delete,
            Rename,
            Directory
        }

        public string Host { get; set; }
        public int Port { get; set; }
        public string UserName { get; set; }
        public string Password { get; set; }

        public string RemotePath { get; set; }
        public string LocalPath { get; set; }

        public FtpClient(string host, int port)
        {
            this.Host = host;
            this.Port = port;
            this.UserName = "anonymous";
        }

      
        private string GetFullPath(string fileName)
        {
            Contract.Requires(!string.IsNullOrEmpty(Host));

            string fullPath = string.Format("ftp://{0}/%2F", Host);

            if (RemotePath != null)
            {
                if (RemotePath.StartsWith("/"))
                    fullPath = string.Format("{0}/{1}", fullPath, RemotePath.Substring(1));
                else
                    fullPath = string.Format("{0}/{1}", fullPath, RemotePath);
            }


            if (fileName == null)
                return fullPath;
            else
                return string.Format("{0}/{1}", fullPath, fileName);

        }

        private FtpWebRequest Request(string fileName, Method method)
        {
            Contract.Requires(!string.IsNullOrEmpty(UserName));
            Contract.Requires(!string.IsNullOrEmpty(Password));

            string url = GetFullPath(fileName);
            Uri uri = new Uri(url);

            FtpWebRequest request = (FtpWebRequest)WebRequest.Create(uri);
            switch (method)
            {
                case Method.Upload:
                    request.Method = WebRequestMethods.Ftp.UploadFile;
                    break;

                case Method.Download:
                    request.Method = WebRequestMethods.Ftp.DownloadFile;
                    break;

                case Method.Delete:
                    request.Method = WebRequestMethods.Ftp.DeleteFile;
                    break;

                case Method.Rename:
                    request.Method = WebRequestMethods.Ftp.Rename;
                    break;

                case Method.Directory:
                    request.Method = WebRequestMethods.Ftp.ListDirectory;
                    break;
            }

            request.Credentials = new NetworkCredential(UserName, Password);
            request.Proxy = null;

            return request;
        }

        public IEnumerable<string> GetFileNames(string wildcard)
        {
            var names = GetFileNames();

            List<string> list = new List<string>();
            foreach (var name in names)
            {
                if (WildcardMatch(name, wildcard, false))
                    list.Add(name);
            }

            return list;
        }

        private bool WildcardMatch(string s, string wildcard, bool case_sensitive)
        {
            // Replace the * with an .* and the ? with a dot. Put ^ at the
            // beginning and a $ at the end
            String pattern = string.Format("^{0}$", Regex.Escape(wildcard).Replace(@"\*", ".*").Replace(@"\?", "."));

            // Now, run the Regex as you already know
            Regex regex;
            if (case_sensitive)
                regex = new Regex(pattern);
            else
                regex = new Regex(pattern, RegexOptions.IgnoreCase);

            return (regex.IsMatch(s));
        }



        public IEnumerable<string> GetFileNames()
        {
            var request = Request(null, Method.Directory);

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            using (StreamReader reader = new StreamReader(response.GetResponseStream()))
            {

                string text = reader.ReadToEnd();
                if (text != "")
                {
                    string[] lines = text.Split(new char[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries);

                    List<string> list = new List<string>();
                    foreach (var line in lines)
                    {
                        string[] items = line.Split('/');
                        list.Add(items[items.Length - 1]);
                    }

                    return list;
                }
                else
                    return new List<string>();
            }
        }

        public bool Exists(string fileName)
        {
            var files = GetFileNames();
            return files.FirstOrDefault(file => file.ToUpper() == fileName.ToUpper()) != null;
        }


        public string Download(string fileName, string localFileName)
        {
           Action<Stream> action = responseStream =>
           {
               string local = localFileName;
               if (LocalPath != null)
                   local = string.Format("{0}\\{1}", LocalPath, localFileName);

               using (FileStream writer = new FileStream(local, FileMode.Create))
               {
                   CopyTo(responseStream, writer);
               }

           };

            return Download(fileName, action);
        }

        public string Download(string fileName, Action<Stream> action)
        {
            FtpWebRequest request = Request(fileName, Method.Download);

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            using (Stream responseStream = response.GetResponseStream())
            {
                action(responseStream);

                return response.StatusDescription;
            }

        }

        public string Upload(string fileName, string localFileName)
        {
            FtpWebRequest request = Request(fileName, Method.Upload);

            string local = localFileName;
            if (LocalPath != null)
                local = string.Format("{0}\\{1}", LocalPath, localFileName);

            using (FileStream reader = new FileStream(local, FileMode.Open))
            {
                request.ContentLength = reader.Length;
                using (Stream requestStream = request.GetRequestStream())
                {
                    CopyTo(reader, requestStream);
                }
            }

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {

                return response.StatusDescription;
            }
        }

        public string Delete(string fileName)
        {

            FtpWebRequest request = Request(fileName, Method.Delete);

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                return response.StatusDescription;
            }
        }

        public string Rename(string fileName, string newFileName)
        {
            FtpWebRequest request = Request(fileName, Method.Rename);
            request.RenameTo = newFileName;

            using (FtpWebResponse response = (FtpWebResponse)request.GetResponse())
            {
                return response.StatusDescription;
            }

        }


        private static void CopyTo(Stream src, Stream dest)
        {
            int size = (src.CanSeek) ? Math.Min((int)(src.Length - src.Position), 0x2000) : 0x2000;
            byte[] buffer = new byte[size];
            int n;
            do
            {
                n = src.Read(buffer, 0, buffer.Length);
                dest.Write(buffer, 0, n);
            } while (n != 0);
        }
    }
}
