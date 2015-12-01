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

namespace Sys.Data
{
    /// <summary>
    /// a value can be used on SQL statement
    /// </summary>
    internal class SqlValue
    {
        private object value;

        public SqlValue(object value)
        {
            this.value = value;
        }

        public static bool gb2312text(string text)
        {
            Encoding encoding = System.Text.Encoding.GetEncoding("gb2312");
            
            for (int i = 0; i < text.Length; i++)
            {
                byte[] s2 = encoding.GetBytes(text.Substring(i, 1));
                if (s2.Length == 2)
                    return true;
            }

            return false;
        }

        public string Text
        {
            get
            {
                if (value == null || value == DBNull.Value)
                    return "NULL";

                StringBuilder sb = new StringBuilder();

                if (value is string)
                {
                    //N: used for SQL Type nvarchar
                    if (gb2312text(value as string))
                        sb.Append("N");

                    sb.Append("'")
                      .Append((value as string).Replace("'", "''"))
                      .Append("'");
                }
                else if (value is bool || value is bool?)
                {
                    sb.Append((bool)value ? "1" : "0");
                }
                else if (value is DateTime || value is DateTime? || value is char)
                {
                    sb.Append("'").Append(value).Append("'");
                }
                else if (value is byte[])
                {
                    sb.Append("0x" + BitConverter.ToString((byte[])value).Replace("-", ""));
                }
                else
                {
                    sb.Append(value);
                }

                return sb.ToString();
            }
        }

        public override string ToString()
        {
            return this.Text;
        }
      
    }
}
