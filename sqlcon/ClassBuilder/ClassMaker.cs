using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

using Sys;
using Sys.Data;
using Sys.Data.Manager;
using Sys.CodeBuilder;

namespace sqlcon
{
    class ClassMaker
    {
        protected Command cmd;
        public ClassMaker(Command cmd)
        {
            this.cmd = cmd;
        }

        protected string NameSpace
        {
            get
            {
                string _ns = cmd.GetValue("ns") ?? "Sys.DataModel.Db";
                return _ns;
            }
        }

        protected virtual string ClassName
        {
            get
            {
                string _cname = cmd.GetValue("class");
                if (_cname != null)
                {
                    return _cname;
                }

                return nameof(DataTable);
            }
        }


        protected void PrintOutput(CSharpBuilder builder, string cname)
        {
            string code = $"{builder}";

            string path = cmd.GetValue("out");
            if (path == null)
            {
                stdio.WriteLine(code);
            }
            else
            {
                string file = Path.ChangeExtension(Path.Combine(path, cname), "cs");
                try
                {
                    code.WriteIntoFile(file);
                    stdio.WriteLine("code generated on {0}", Path.GetFullPath(file));
                }
                catch (Exception ex)
                {
                    stdio.WriteLine(ex.Message);
                }
            }
        }
    }
}
