using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Data;
using Tie;

namespace Sys.Networking
{

    /// <summary>
    /// used for server to accept request from RemoteInvoke's request
    /// </summary>
    public class RemoteProxy
    {

        private RemoteOutputBlock result = new RemoteOutputBlock { ret = string.Empty, err = string.Empty };


        public RemoteProxy()
        {
          
        }

        static RemoteProxy()
        {
            Constant.MAX_STRING_SIZE = 10 * 1024 * 1024;
            RemoteExtension.Init();
        }

        public string Dispatcher(string json)
        {
            var input = DataContractJson.Deserialize<RemoteInputBlock>(json);

            switch (input.method)
            {
                case "Execute":
                    return ExecuteOnServer(input);

                case "Evaluate":
                    return EvaluateOnServer(input);
            }

            return string.Empty;
        }

        private string ExecuteOnServer(RemoteInputBlock input)
        {

            Memory ds = new Memory();
            if (!string.IsNullOrEmpty(input.mem))
            {
                ds = input.mem.Deserialize();
            }

            try
            {
                Script.Execute(input.code, ds);
                result.mem = ds.Serialize();
            }
            catch (Exception ex)
            {
                string message = string.Format("code={0} mem={1} exception={2}", input.code, input.mem, ex.Message);
                //log.Error(message, ex);
                result.err = ex.Message;
            }

            return DataContractJson.Serialize(result);
        }

        private string EvaluateOnServer(RemoteInputBlock input)
        {

            Memory ds = new Memory();
            if (!string.IsNullOrEmpty(input.mem))
            {
                ds = input.mem.Deserialize();
            }

            try
            {
                VAL val = Script.Evaluate(input.code, ds);
                result.mem = ds.Serialize();
                result.ret = val.ToString();
            }
            catch (Exception ex)
            {
                string message = string.Format("code={0} mem={1} exception={2}", input.code, input.mem, ex.Message);
                //log.Error(message, ex);
                result.err = ex.Message;
            }

            return DataContractJson.Serialize(result);
        }

    }
}
