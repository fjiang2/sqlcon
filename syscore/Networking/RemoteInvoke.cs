using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;
using Tie;


namespace Sys.Networking
{

    /// <summary>
    /// used for client to access server's class or method
    /// </summary>
    public class RemoteInvoke
    {
        private Uri uri;

        private Memory DS  {get ;set;}

        static RemoteInvoke()
        {
            RemoteExtension.Init();
        }

        public RemoteInvoke(Uri uri)
            :this(uri, null)
        {
        }

        /// <summary>
        /// e.g. map = new { host="127.0.0.1", port = 80}
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="map"></param>
        public RemoteInvoke(Uri uri, object map)
        {
            this.uri = uri;

            if (map != null)
                DS = map.Encode();
            else
                DS = new Memory();
        }

        public T GetValue<T>(string variable)
        {
            return DS.GetValue<T>(variable);
        }

   
        public void Execute(string code)
        {
            string json = Json.Serialize(new RemoteInputBlock { method = "Execute", code = code, mem = DS.Serialize() });
            string result = RemoteAccess(uri, json);
            RemoteOutputBlock output = Json.Deserialize<RemoteOutputBlock>(result);

            if (!string.IsNullOrEmpty(output.err))
            {
                throw new InvalidExpressionException($"failed to execute code: {code}, {output.err}");
            }

            if (!string.IsNullOrEmpty(output.mem))
            {
                DS = output.mem.Deserialize();
            }
        }

        public object Evaluate(string code)
        {
            string json = Json.Serialize(new RemoteInputBlock { method = "Evaluate", code = code, mem = DS.Serialize() });
            string result = RemoteAccess(uri, json);
            RemoteOutputBlock output = Json.Deserialize<RemoteOutputBlock>(result);

            if (!string.IsNullOrEmpty(output.err))
            {
                throw new InvalidExpressionException($"failed to evaluate code: {code}, {output.err}");
            }

            try
            {
                if (!string.IsNullOrEmpty(output.mem))
                {
                    DS = output.mem.Deserialize();
                }

                VAL val = Script.Evaluate(output.ret);
                return val.HostValue;
            }
            catch (Exception)
            {
                throw new Exception($"failed to evaluate code: {code}, {result}");
            }

        }

        private static string RemoteAccess(Uri uri, string text)
        {
            byte[] bytes = Encoding.UTF8.GetBytes(text);

            return HttpRequest.HttpPost<string>(uri, bytes.Length,
                  requestStream =>
                  {
                      requestStream.Write(bytes, 0, bytes.Length);
                  },

                  responseStream =>
                  {
                      string result = string.Empty;
                      using (StreamReader streamReader = new StreamReader(responseStream))
                      {
                          result = streamReader.ReadToEnd();
                      }
                      return result;
                  }

             );
        }

    }
}
