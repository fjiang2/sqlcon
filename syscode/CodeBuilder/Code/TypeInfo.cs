//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        syscode(C# Code Builder)                                                                  //
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
        public static readonly TypeInfo Anonymous = new TypeInfo { UserType = string.Empty };

        public Type Type { get; set; } = null;

        public bool Nullable { get; set; } = false;

        public string UserType { get; set; }

        public bool IsArray { get; set; }

        public TypeInfo()
        {
        }

        public TypeInfo(string userType)
        {
            this.UserType = userType;
        }

        public TypeInfo(Type type)
        {
            this.Type = type;
        }

        public Type GetElementType()
        {
            if (IsArray)
                return Type;
            else
                return Type?.GetElementType();
        }

        private string typeText()
        {
            if (UserType != null)
            {
                if (IsArray)
                    return UserType + "[]";
                else
                    return UserType;
            }

            if (Type == null)
            {
                Nullable = false;
                return "void";
            }

            if (IsArray)
            {
                Nullable = false;
                return new TypeInfo(Type).typeText() + "[]";
            }


            if (Type.IsArray)
            {
                Nullable = false;
                return new TypeInfo(Type.GetElementType()).typeText() + "[]";
            }

            if (Type == typeof(string))
            {
                Nullable = false;
                return "string";
            }

            if (Type == typeof(object))
                return "object";

            if (Type == typeof(int))
                return "int";

            if (Type == typeof(short))
                return "short";

            if (Type == typeof(long))
                return "long";

            if (Type == typeof(uint))
                return "uint";

            if (Type == typeof(ushort))
                return "ushort";

            if (Type == typeof(ulong))
                return "ulong";

            if (Type == typeof(double))
                return "double";

            if (Type == typeof(float))
                return "float";

            if (Type == typeof(bool))
                return "bool";

            if (Type == typeof(char))
                return "char";

            if (Type == typeof(byte))
                return "byte";

            if (Type == typeof(sbyte))
                return "sbyte";

            if (Type == typeof(decimal))
                return "decimal";

            if (Type.IsClass || Type.IsArray)
                Nullable = false;

            string ty = Type.Name;
            if (Type.IsGenericType)
            {
                ty = Type.Name.Substring(0, ty.IndexOf("`"));
                var args = string.Join(", ", Type.GetGenericArguments().Select(T => new TypeInfo(T).ToString()));
                ty = string.Format("{0}<{1}>", ty, args);
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

        public static implicit operator TypeInfo(Type type)
        {
            return new TypeInfo(type);
        }

        public static TypeInfo Generic<T>(TypeInfo type)
        {
            return new TypeInfo($"{type}<{new TypeInfo(typeof(T))}>");
        }

        public static TypeInfo Generic<T1, T2>(TypeInfo type)
        {
            return new TypeInfo($"{type}<{new TypeInfo(typeof(T1))}, {new TypeInfo(typeof(T2))}>");
        }
    }
}
