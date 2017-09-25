﻿using System;
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
            string path = cmd.GetValue("out");

            string code = $"{builder}";
            if (path == null)
            {
                stdio.WriteLine(code);
            }
            else
            {
                string file;
                if (Path.GetExtension(path) == ".cs")
                    file = path;
                else
                    file = Path.ChangeExtension(Path.Combine(path, cname), "cs");

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

        protected string ReadTieCode()
        {
            string path = cmd.GetValue("in");
            if (path == null)
                return null;

            if (!File.Exists(path))
            {
                stdio.Error($"file {path} not found");
                return null;
            }

            return File.ReadAllText(path);
        }

    }
}
