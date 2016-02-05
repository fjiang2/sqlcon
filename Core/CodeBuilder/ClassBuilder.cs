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

namespace Sys.CodeBuilder
{
    public class ClassBuilder : Buildable
    {
        public string nameSpace { get; set; } = "Sys.Unknown";

        List<string> usings = new List<string>();
        List<Class> classes = new List<Class>();

        public ClassBuilder()
        {
        }


        public ClassBuilder AddUsing(string name)
        {
            this.usings.Add(name);
            return this;
        }

        public ClassBuilder AddClass(Class clss)
        {
            classes.Add(clss);
            return this;
        }

        protected override CodeBlock BuildBlock()
        {
            CodeBlock block = base.BuildBlock();

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

            return block;
        }

    }
}
