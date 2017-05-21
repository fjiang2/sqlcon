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
        public static readonly TypeInfo Anonymous = new TypeInfo { userType = string.Empty };

        public Type type { get; set; } = null;

        public bool Nullable { get; set; } = false;

        public string userType { get; set; }
        public bool isArray { get; set; }

        public TypeInfo()
        {
        }

        public TypeInfo(string userType)
        {
            this.userType = userType;
        }

        public TypeInfo(Type type)
        {
            this.type = type;
        }

        private string typeText()
        {
            if (userType != null)
                return userType;

            if (type == null)
            {
                Nullable = false;
                return "void";
            }

            if (isArray)
            {
                Nullable = false;
                return new TypeInfo(type).typeText() + "[]";
            }


            if (type.IsArray)
            {
                Nullable = false;
                return new TypeInfo(type.GetElementType()).typeText() + "[]";
            }

            if (type == typeof(string))
            {
                Nullable = false;
                return "string";
            }

            if (type == typeof(int))
                return "int";

            if (type == typeof(double))
                return "double";

            if (type == typeof(bool))
                return "bool";

            if (type == typeof(byte))
                return "byte";

            if (type == typeof(decimal))
                return "decimal";

            if (type.IsClass || type.IsArray)
                Nullable = false;

            string ty = type.Name;
            if (type.IsGenericType)
            {
                ty = type.Name.Substring(0, ty.IndexOf("`"));
                ty = string.Format("{0}<{1}>", ty, string.Join(",", type.GetGenericArguments().Select(T => T.Name)));
            }

            return ty;
        }

        public override string ToString()
        {
            string text = typeText();

            if (Nullable)
                return text + "?";
            else
                return text;
        }
    }
}
