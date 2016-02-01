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
    public class TypeInfo
    {
        Type type;
        public bool Nullable { get; set; } = false;

        public TypeInfo(Type type)
        {
            this.type = type;
        }

        public string NewInstance(params Argument[] args)
        {
            return string.Format("new {0}({1})", 
                this.Text, 
                string.Join(
                    ",", 
                    args.Select(arg => arg.ToString())
                    )
                );
        }

        private string typeText()
        {
            if (type == typeof(string))
                return "string";

            if (type == typeof(int))
                return "int";

            if (type == typeof(double))
                return "double";

            if (type == typeof(bool))
                return "bool";

            if (type == typeof(byte))
                return "byte";

            string ty = type.Name;
            if (type.IsGenericType)
            {
                ty = type.Name.Substring(0, ty.IndexOf("`"));
                ty = string.Format("{0}<{1}>", ty, string.Join(",", type.GetGenericArguments().Select(T => T.Name)));
            }

            return ty;
        }

        public string Text
        {
            get
            {
                string text = typeText();
                if (Nullable)
                    return text + "?";
                else
                    return text;
              
            }
        }

        public override string ToString()
        {
            return this.Text;
        }
    }
}
