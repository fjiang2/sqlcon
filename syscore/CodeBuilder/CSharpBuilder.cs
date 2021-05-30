//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        DPO(Data Persistent Object)                                                               //
//                                                                                                  //
//          Copyright(c) Datum Connect Inc.                                                         //
//                                                                                                  //
// This source code is subject to terms and conditions of the Datum Connect Software License. A     //
// copy of the license can be found in the License.html file at the root of this distribution. If   //
// you cannot locate the  Datum Connect Software License, please send an email to                   //
// datconn@gmail.com. By using this source code in any fashion, you are agreeing to be bound        //
// by the terms of the Datum Connect Software License.                                              //
//                                                                                                  //
// You must not remove this notice, or any other, from this software.                               //
//                                                                                                  //
//                                                                                                  //
//--------------------------------------------------------------------------------------------------//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.IO;

namespace Sys.CodeBuilder
{
    public class CSharpBuilder : Buildable
    {
        public string Namespace { get; set; } = "Sys.Unknown";

        private readonly List<string> usings = new List<string>();
        private readonly List<Prototype> classes = new List<Prototype>();

        public CSharpBuilder()
        {
        }


        public CSharpBuilder AddUsing(string name)
        {
            if (usings.IndexOf(name) < 0)
                this.usings.Add(name);

            return this;
        }

        public CSharpBuilder AddUsingRange(IEnumerable<string> names)
        {
            foreach (var name in names)
                AddUsing(name);

            return this;
        }

        public CSharpBuilder AddClass(Class clss)
        {
            classes.Add(clss);
            clss.Parent = this;
            return this;
        }

        public CSharpBuilder AddEnum(EnumType _enum)
        {
            classes.Add(_enum);
            _enum.Parent = this;
            return this;
        }

        protected override void BuildBlock(CodeBlock block)
        {
            foreach (var name in usings)
                block.AppendFormat("using {0};", name);

            block.AppendLine();

            block.AppendFormat("namespace {0}", this.Namespace);

            var c = new CodeBlock();

            classes.ForEach(
                    clss => c.Add(clss.GetBlock()),
                    clss => c.AppendLine()
                );

            block.AddWithBeginEnd(c);

        }

        public void Output(TextWriter writer)
        {
            writer.Write(this.ToString());
        }

        public void Output(string directory, string cname)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            string code = this.ToString();
            string file = Path.ChangeExtension(Path.Combine(directory, cname), "cs");
            File.WriteAllText(file, code);
        }

        public void Output(string directory)
        {
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            foreach (Prototype clss in classes)
            {
                CodeBlock block = new CodeBlock();
                foreach (var name in usings)
                    block.AppendFormat("using {0};", name);

                block.AppendLine();
                block.AppendFormat("namespace {0}", this.Namespace);

                var c = new CodeBlock();
                c.Add(clss.GetBlock());
                block.AddWithBeginEnd(c);

                string code = block.ToString();

                string folder = directory;
                if (!string.IsNullOrEmpty(clss.Subdirectory))
                {
                    folder = Path.Combine(directory, clss.Subdirectory);
                    if (!Directory.Exists(folder))
                        Directory.CreateDirectory(folder);
                }

                string file = Path.ChangeExtension(Path.Combine(folder, clss.Name), "cs");
                File.WriteAllText(file, code);
            }
        }
    }
}
