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

namespace Sys
{
    public sealed class ident : IComparable, IComparable<string>, IEquatable<ident>
    {
        private string id;

        public ident(string id)
        {
            this.id = id;

            if (!Validate())
                throw new MessageException("Invalid ident: {0}", id);
        }


        /// <summary>
        /// valid ident:
        ///     "[Last Name#]"
        ///     "@LastName"
        ///     "_LastName"
        ///     "LastName2"
        /// </summary>
        /// <returns></returns>
        private bool Validate()
        {
            if (id[0] == '[' && id[id.Length - 1] == ']')
                return true;

            int i = 0;
            char ch = id[i++];

            if (!char.IsLetter(ch) && ch != '_' && ch != '@')
                return false;

            while (i < id.Length) 
            {
                ch = id[i++];

                if (ch != '_' && !char.IsLetterOrDigit(ch))
                    return false;
            } 

            return true;
        }

        public override int GetHashCode()
        {
            return id.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            return id.Equals(((ident)obj).id);
        }

        public bool Equals(ident obj)
        {
            return id.Equals(obj.id);
        }

        public override string ToString()
        {
            return this.id;
        }

        public static bool operator == (ident id1, ident id2)
        {
            return id1.id.Equals(id2.id);
        }

        public static bool operator !=(ident id1, ident id2)
        {
            return !(id1==id2);
        }

        public static explicit operator string(ident ident)
        {
            return ident.id; 
        }

        public int CompareTo(object obj)
        {
            return this.id.CompareTo(obj);
        }

        public int CompareTo(string other)
        {
            return this.id.CompareTo(other);
        }

        public static string Identifier(string s)
        {
            s = s.Trim();
            StringBuilder sb = new StringBuilder();
            
            if (char.IsDigit(s[0]))
                sb.Append("_");

            for (int i = 0; i < s.Length; i++)
                if (IsBasicLetter(s[i]) || s[i] == '_')
                    sb.Append(s[i]);
                else
                    sb.Append('_');

            return sb.ToString();
        }

        public static bool IsBasicLetter(char c)
        {
            return (c >= 'a' && c <= 'z') || (c >= 'A' && c <= 'Z');
        }
    }
}
