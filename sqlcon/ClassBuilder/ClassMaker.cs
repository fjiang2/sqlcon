using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.IO;

using Sys;
using Sys.CodeBuilder;

namespace sqlcon
{
    class ClassMaker
    {
        protected const string LP = "{";
        protected const string RP = "}";

        public string ns { get; set; }
        public string cname { get; set; }

        protected Command cmd;

        public ClassMaker(Command cmd)
        {
            this.cmd = cmd;
        }

        protected string NameSpace
        {
            get
            {
                if (ns != null)
                    return ns;

                string _ns = cmd.GetValue("ns") ?? "Sys.DataModel.Db";
                return _ns;
            }
        }

        protected virtual string ClassName
        {
            get
            {
                if (cname != null)
                    return cname;

                string _cname = cmd.GetValue("class");
                if (_cname != null)
                {
                    return _cname;
                }

                return nameof(DataTable);
            }
        }

        protected string[] Usings
        {
            get
            {
                string __using = cmd.GetValue("using");
                if (__using == null)
                {
                    return new string[] { };
                }

                return __using.Split(';');
            }
        }

        protected void PrintOutput(CSharpBuilder builder, string cname)
        {
            string code = $"{builder}";
            PrintOutput(code, cname, ".cs");
        }

        protected void PrintOutput(string text, string name, string ext)
        {
            string path = cmd.GetValue("out");
            if (path == null)
            {
                cout.WriteLine(text);
            }
            else
            {
                string file;
                if (Path.GetExtension(path) == ext)
                    file = path;
                else
                    file = Path.ChangeExtension(Path.Combine(path, name), ext);

                try
                {
                    text.WriteIntoFile(file);
                    cout.WriteLine("created on {0}", Path.GetFullPath(file));
                }
                catch (Exception ex)
                {
                    cout.WriteLine(ex.Message);
                }
            }
        }

        protected string ReadAllText()
        {
            string path = cmd.GetValue("in");
            if (path == null)
                return null;

            return ReadAllText(path);
        }

        protected string ReadAllText(string path)
        {
            if (!File.Exists(path))
            {
                cout.Error($"file {path} not found");
                return null;
            }

            return File.ReadAllText(path);
        }
    }
}
