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

namespace Sys.CodeBuilder
{
    class CompoundStatement : List<string>
    {
        int tab;

        public CompoundStatement(int tab)
        {
            this.tab = tab;
        }

        public string Code
        {
            get
            {
                StringBuilder sb = new StringBuilder();

                string TAB1 = new string('\t', tab);
                string TAB2 = new string('\t', tab + 1);

                sb.Append(TAB1).AppendLine("{");
                foreach (string sent in this)
                {
                    if (sent == "")
                        sb.AppendLine();
                    else
                    {
                        var expr = sent.Split(new string[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                        sb.Append(string.Join(Environment.NewLine, expr.Select(exp => TAB2 + exp)));
                        sb.AppendLine(";");
                    }
                }

                sb.Append(TAB1).AppendLine("}");
                return sb.ToString();
            }
        }

        public override string ToString()
        {
            return Code;
        }
    }
}
