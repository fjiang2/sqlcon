using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using System.Xml;
using System.Net;
using System.IO;
using System.Data;

namespace Sys.Networking
{
    public static class HttpRequest
    {
        public static T HttpPost<T>(Uri uri, long size, Action<Stream> requestWriter, Func<Stream, T> responseReader)
        {
            WebRequest request = WebRequest.Create(uri);
            request.Method = WebRequestMethods.Http.Post;
            //request.ContentType = "application/x-www-form-urlencoded";
            request.ContentLength = size;

            if (requestWriter != null)
            {
                using (Stream requestStream = request.GetRequestStream())
                {
                    requestWriter(requestStream);
                }
            }

            if (responseReader != null)
            {
                HttpWebResponse response = (HttpWebResponse)request.GetResponse();
                try
                {
                    using (Stream responseStream = response.GetResponseStream())
                    {
                        T result = responseReader(responseStream);
                        return result;
                    }
                }
                finally
                {
                    response.Close();
                }
            }

            return default(T);
        }


        public static T HttpGet<T>(Uri uri, Func<Stream, T> responseReader)
        {
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "GET";
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            try
            {
                Stream responseStream = response.GetResponseStream();
                try
                {
                    T result = responseReader(responseStream);
                    return result;
                }
                finally
                {
                    responseStream.Close();
                }
            }
            finally
            {
                response.Close();
            }
        }


        private static T HttpGetReader<T>(Uri uri, Func<StreamReader, T> responseReader)
        {
            return HttpGet<T>(uri, responseStream =>
            {
                using (StreamReader streamReader = new StreamReader(responseStream))
                {
                    T result = responseReader(streamReader);
                    return result;
                }
            });
        }

        /// <summary>
        /// Read response xml
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static DataSet ReadXml(Uri uri)
        {
            return HttpGetReader<DataSet>(uri, reader =>
            {
                DataSet ds = new DataSet();
                ds.ReadXml(reader, XmlReadMode.ReadSchema);
                return ds;
            });
        }

        public static void ReadXml(Uri uri, DataSet ds)
        {
            HttpGetReader<DataSet>(uri, reader =>
            {
                ds.ReadXml(reader, XmlReadMode.ReadSchema);
                return ds;
            });
        }

        /// <summary>
        /// post request text to Uri and read response xml
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="text"></param>
        /// <returns></returns>
        public static DataSet ReadXml(Uri uri, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);

            return HttpPost<DataSet>(uri, bytes.Length,
                  requestStream =>
                  {
                      requestStream.Write(bytes, 0, bytes.Length);
                  },

                  responseStream =>
                  {
                      DataSet ds = new DataSet();
                      using (StreamReader streamReader = new StreamReader(responseStream))
                      {
                          ds.ReadXml(streamReader, XmlReadMode.ReadSchema);
                      }
                      return ds;
                  }

             );

        }

        public static string ReadText(Uri uri)
        {
            return HttpGetReader<string>(uri, reader => reader.ReadToEnd());
        }

        /// <summary>
        /// read from uri and save into file
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="fileName"></param>
        /// <param name="size"></param>
        public static void ReadFile(Uri uri, string fileName, int size)
        {
            using (FileStream fstream = new FileStream(fileName, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite))
            {
                Read(uri, fstream, size);
            }
        }

        /// <summary>
        /// read from uri and feed into stream
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="fstream"></param>
        /// <param name="size"></param>
        public static void Read(Uri uri, Stream fstream, int size)
        {
            HttpGet<object>(uri, responseStream =>
            {
                byte[] inData = new byte[size];
                int bytesRead = responseStream.Read(inData, 0, inData.Length);
                fstream.Write(inData, 0, bytesRead);

                while (bytesRead < size)
                {
                    int moreBytes = responseStream.Read(inData, 0, inData.Length);
                    fstream.Write(inData, 0, moreBytes);

                    bytesRead += moreBytes;

                }

                return null;
            });
        }

        public static void ReadAsync(Uri uri, Stream fstream, int size, CancellationToken cancellationToken)
        {
            HttpGet<Task<object>>(uri, async (responseStream) =>
            {
                byte[] inData = new byte[size];
                int bytesRead = await responseStream.ReadAsync(inData, 0, inData.Length, cancellationToken);
                fstream.Write(inData, 0, bytesRead);

                while (bytesRead < size)
                {
                    int moreBytes = await responseStream.ReadAsync(inData, 0, inData.Length, cancellationToken);
                    fstream.Write(inData, 0, moreBytes);

                    bytesRead += moreBytes;

                }

                return null;
            });
        }


        /// <summary>
        /// read bytes from file and write into  uri
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="fileName"></param>
        public static void Write(Uri uri, string fileName)
        {
            byte[] bytes = ReadFileToBytes(fileName);

            HttpPost<object>(uri, (long)bytes.Length, requestStream => requestStream.Write(bytes, 0, bytes.Length), null);

        }


        public static bool Exists(Uri uri)
        {
            bool exists = false;
            HttpWebResponse response = null;
            var request = (HttpWebRequest)WebRequest.Create(uri);
            request.Method = "HEAD";
            request.Timeout = 3000; // milliseconds
            request.AllowAutoRedirect = false;

            try
            {
                response = (HttpWebResponse)request.GetResponse();
                exists = response.StatusCode == HttpStatusCode.OK;
            }
            catch
            {
                exists = false;
            }
            finally
            {
                if (response != null)
                    response.Close();
            }

            return exists;
        }


        #region Utility

        private static byte[] ReadFileToBytes(string fileName)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            StreamReader streamReader = new StreamReader(fileName);
            BinaryReader binaryReader = new BinaryReader(streamReader.BaseStream, encoding);
            FileInfo fileInfo = new FileInfo(fileName);
            byte[] bytes = binaryReader.ReadBytes((int)fileInfo.Length);
            binaryReader.Close();
            return bytes;
        }

        public static string md5(string filename)
        {
            byte[] bytes = ReadFileToBytes(filename);

            // encrypt bytes
            System.Security.Cryptography.MD5CryptoServiceProvider md5 = new System.Security.Cryptography.MD5CryptoServiceProvider();
            byte[] hashBytes = md5.ComputeHash(bytes);

            // Convert the encrypted bytes back to a string (base 16)
            string hashString = "";

            for (int i = 0; i < hashBytes.Length; i++)
            {
                hashString += Convert.ToString(hashBytes[i], 16).PadLeft(2, '0');
            }

            return hashString.PadLeft(32, '0');
        }

        #endregion

        public static HttpStatusCode GetHttpStatus(Uri uri)
        {
            HttpStatusCode result = HttpStatusCode.BadRequest;

            var request = HttpWebRequest.Create(uri);
            request.Method = "HEAD";
            try
            {
                using (var response = request.GetResponse() as HttpWebResponse)
                {
                    if (response != null)
                    {
                        result = response.StatusCode;
                        response.Close();
                    }
                }
            }
            catch (WebException ex)
            {
                System.Diagnostics.Debug.WriteLine(ex.Message);
            }

            return result;
        }

    }
}
