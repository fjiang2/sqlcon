using Sys;
using Sys.CodeBuilder;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Sys.Stdio;

namespace sqlcon
{
    public class ClassMaker
    {
        protected const string LP = "{";
        protected const string RP = "}";

        private string ns;
        private string cname;
        private string mtd;

        protected IApplicationCommand cmd;

        public ClassMaker(IApplicationCommand cmd)
        {
            this.cmd = cmd;
        }

        public void SetNamespace(string ns)
        {
            this.ns = ns;
        }

        public void SetClassName(string cname)
        {
            this.cname = ident.Identifier(cname);
        }

        public void SetMethod(string mtd)
        {
            this.mtd = mtd;
        }

        protected string NamespaceName
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

        public string MethodName => this.mtd;


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

        protected Dictionary<string, string[]> Attributes
        {
            get
            {
                string attr = cmd.GetValue("attribute");
                if (attr == null)
                    return new Dictionary<string, string[]>();

                return attr.Split(',')
                    .Select(x => new AttributeDefinition(x))
                    .ToDictionary(x => x.MemberName, x => x.Attributes.ToArray());
            }
        }

        protected void PrintOutput(CSharpBuilder builder, string cname)
        {
            string code = $"{builder}";
            PrintOutput(code, cname, ".cs");
        }

        protected void PrintOutput(string text, string name, string ext)
        {
            string path = cmd.OutputFile($"{name}.cs");
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
            string path = cmd.InputPath();
            if (path == null)
                return null;

            return ReadAllText(path);
        }

        protected string ReadAllText(string path)
        {
            if (!File.Exists(path))
            {
                cerr.WriteLine($"file {path} not found");
                return null;
            }

            return File.ReadAllText(path);
        }

        class AttributeDefinition
        {
            public List<string> Attributes { get; } = new List<string>();
            public string MemberName { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="definition">[A1][A2]PropertyName</param>
            public AttributeDefinition(string definition)
            {
                int i = 0;
                while (i < definition.Length)
                {
                    char ch = definition[i];
                    int start;
                    int count;
                    switch (ch)
                    {
                        case '[':
                            i++;
                            start = i;
                            count = 0;
                            while (definition[i++] != ']')
                                count++;

                            Attributes.Add(definition.Substring(start, count));
                            break;

                        default:
                            MemberName = definition.Substring(i, definition.Length - i);
                            return;
                    }
                }
            }
        }
    }

}
