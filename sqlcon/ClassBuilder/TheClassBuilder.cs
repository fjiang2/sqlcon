using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using Sys;
using Sys.CodeBuilder;

namespace sqlcon
{
    abstract class TheClassBuilder
    {
        protected const string LP = "{";
        protected const string RP = "}";

        public string ns { get; }
        public string cname { get; set; }

        public string _using { get; set; }
        public string _base { get; set; }

        protected CSharpBuilder builder;
        private Command cmd;

        public TheClassBuilder(string ns, Command cmd)
        {
            this.cmd = cmd;
            this._using = cmd.GetValue("using");
            this._base = cmd.GetValue("base");

            builder = new CSharpBuilder { nameSpace = ns };
        }

        public void AddOptionalUsing()
        {
            if (_using != null)
            {
                string[] items = _using.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in items)
                {
                    builder.AddUsing(item);
                }
            }
        }

        public TypeInfo[] OptionalBaseType(params TypeInfo[] inherits)
        {
            List<TypeInfo> bases = new List<TypeInfo>(inherits);

            if (_base != null)
            {
                string[] items = _base.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string item in items)
                {
                    string type = item.Replace("~", cname);
                    bases.Add(new TypeInfo { userType = type });
                }
            }

            return bases.ToArray();
        }

        protected abstract void CreateClass();

        public string WriteFile(string path)
        {
            CreateClass();

            string code = $"{ builder}";
            string file = Path.ChangeExtension(Path.Combine(path, cname), "cs");
            code.WriteIntoFile(file);

            return file;
        }
    }
}
