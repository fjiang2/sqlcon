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
        public string nameSpace { get; set; } = "Sys.Unknown";

        List<string> usings = new List<string>();
        List<Declare> classes = new List<Declare>();

        public CSharpBuilder()
        {
        }


        public CSharpBuilder AddUsing(string name)
        {
            this.usings.Add(name);
            return this;
        }
        public CSharpBuilder AddUsingRange(IEnumerable<string> names)
        {
            foreach (var name in names)
                this.usings.Add(name);

            return this;
        }

        public CSharpBuilder AddClass(Class clss)
        {
            classes.Add(clss);
            return this;
        }

        public CSharpBuilder AddEnum(Enum _enum)
        {
            classes.Add(_enum);
            return this;
        }

        protected override void BuildBlock(CodeBlock block)
        {
            foreach (var name in usings)
                block.AppendFormat("using {0};", name);

            block.AppendLine();

            block.AppendFormat("namespace {0}", this.nameSpace);

            var c = new CodeBlock();

            classes.ForEach(
                    clss => c.Add(clss.GetBlock()),
                    clss => c.AppendLine()
                );

            block.AddWithBeginEnd(c);

        }

        public void Output(string directory, string cname)
        {
            string code = this.ToString();
            string file = Path.ChangeExtension(Path.Combine(directory, cname), "cs");
            File.WriteAllText(directory, code);
        }
    }
}
