using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using Sys;
using Sys.CodeBuilder;

namespace sqlcon
{
    abstract class TheClassBuilder : ClassMaker
    {
        public string cname { get; set; }
        protected CSharpBuilder builder;

        public TheClassBuilder(string ns, Command cmd)
            : base(cmd)
        {
            builder = new CSharpBuilder { nameSpace = ns };
        }

        public void AddOptionalUsing()
        {
            builder.AddUsingRange(base.Usings);
        }

        public TypeInfo[] OptionalBaseType(params TypeInfo[] inherits)
        {
            List<TypeInfo> bases = new List<TypeInfo>(inherits);

            string _base = cmd.GetValue("base");
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

            base.PrintOutput(builder, cname);
            string code = $"{ builder}";
            string file = Path.ChangeExtension(Path.Combine(path, cname), "cs");
            code.WriteIntoFile(file);

            return file;
        }
    }
}
