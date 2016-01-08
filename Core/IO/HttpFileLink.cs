using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;

namespace Sys.IO
{
    public class HttpFileLink : FileLink
    {
        public HttpFileLink(string url)
            : base(url)
        {
        }

        protected override bool exists()
        {
            return HttpRequest.Exists(new Uri(url));
        }


        public override string PathCombine(string path1, string path2)
        {
            var items = url.Split('/');
            var root = string.Join("/", items.Take(items.Length - 1));
            return string.Format("{0}/{1}/{2}", root, path1, path2);
        }


        public override DataSet ReadXml(DataSet ds)
        {
            HttpRequest.ReadXml(new Uri(url), ds);
            return ds;
        }

    }
}
