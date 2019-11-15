using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Networking
{
    public class RemoteAgent
    {
        public string Host { get; set; }
        public int Port { get; set; }
        public string Application { get; set; }

        private RemoteInvoke remoteInvoke;
        private object map;

        public RemoteAgent()
            : this(null)
        {
        }

        public RemoteAgent(object map)
        {
            Host = "localhost";
            Port = 80;
            Application = "app";

            this.map = map;
        }

        private Uri Uri
        {
            get
            {
                string url = string.Format("http://{0}:{1}/{2}", Host, Port, Application);
                return new Uri(url);
            }
        }


        public T GetValue<T>(string variable)
        {
            if (remoteInvoke != null)
                return remoteInvoke.GetValue<T>(variable);
            else
                return default(T);
        }

        public void Execute(string code)
        {
            remoteInvoke = new RemoteInvoke(Uri, map);
            remoteInvoke.Execute(code);
        }

        public object Evaluate(string code)
        {
            remoteInvoke = new RemoteInvoke(Uri, map);
            return remoteInvoke.Evaluate(code);
        }

    }
}
