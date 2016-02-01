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
    public class Format
    {
        protected int tab = 0;
        protected StringBuilder code = new StringBuilder();

        private static string[] TABS = new string[] { "", "\t", "\t\t", "\t\t\t", "\t\t\t\t", "\t\t\t\t\t" };
        public Format()
        { 
        
        }

        public void Indent(bool yes)
        {
            if (yes)
                tab++;
            else
                tab--;
        }

        public Format Add(string str)
        {
            code.Append(TAB).AppendLine(str);
            return this;
        }

        public Format AddFormat(string format, params object[] args)
        {
            code.Append(TAB).AppendFormat(format, args).AppendLine();
            return this;
        }

        protected string Tab(int n)
        {
            return new string('\t', n);
        }

        public string TAB
        {
            get
            {
                if (tab < TABS.Length)
                    return TABS[tab];

                return new string('\t', tab);
            }
        }

        public override string ToString()
        {
            return code.ToString();
        }
    }
}
