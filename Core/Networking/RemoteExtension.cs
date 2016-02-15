using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using Tie;


namespace Sys.Networking
{
    internal static class RemoteExtension
    {
        public static void Init()
        {
            Script.FunctionChain.Add(RemoteExtension.functions);

            Constant.MAX_STRING_SIZE = 20 * 1024 * 1024;

            Valizer.Register<DataSet>(
                 ds => RemoteExtension.ToVal(ds),
                (host, type, xml) => RemoteExtension.ToDataSet(host, type, xml)
                );

            Valizer.Register<DataTable>(
                dt => RemoteExtension.ToVal(dt),
                (host, type, xml) => RemoteExtension.ToDataTable(host, type, xml)
                );

        }

        public static VAL functions(string func, VAL parameters, Memory DS)
        {
            if (func != "devalize" && parameters.Size != 2)
                return null;

            var L0 = parameters[0];
            var L1 = parameters[1];

            Valizer.Devalize(L0, L1.Value);
            return L1;
        }

        public static VAL ToVal(this DataSet ds)
        {
            string xml = ds.ToXml();
            string code = new VAL(xml).ToString();
            code = string.Format("devalize({0}, new System.Data.DataSet())", code);
            return new VAL(code);
        }

        public static VAL ToVal(this DataTable dt)
        {
            string xml = dt.ToXml();
            string code = new VAL(xml).ToString();
            code = string.Format("devalize({0}, new System.Data.DataTable())", code);
            return new VAL(code);
        }


        public static DataSet ToDataSet(DataSet host, Type type, VAL val)
        {
            string code = (string)val;
            return code.ToDataSet(host);
        }

        public static DataTable ToDataTable(DataTable host, Type type, VAL val)
        {
            string code = (string)val;
            return code.ToDataTable(host);
        }

        public static Memory Encode(this object obj)
        {
            if (obj == null)
                return new Memory();

            VAL val = Valizer.Valize(obj);
            return new Memory(val);
        }

        public static string Serialize(this Memory mem)
        {
            string memory = string.Empty;
            if (mem != null)
            {
                memory = ((VAL)mem).Valor;
            }

            return memory;
        }

        public static Memory Deserialize(this string memeory)
        {
            VAL val = Script.Evaluate(memeory, new Memory());
            return new Memory(val);
        }



    }
}
