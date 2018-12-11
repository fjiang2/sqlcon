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
    class SqlValue
    {
        private const string DELIMETER = "'";
        private const string NULL = "NULL";

        private readonly object value;

        public SqlValue(object value)
        {
            this.value = value;
        }

        private static bool gb2312text(string text)
        {
            Encoding encoding = Encoding.GetEncoding("gb2312");

            for (int i = 0; i < text.Length; i++)
            {
                byte[] s2 = encoding.GetBytes(text.Substring(i, 1));
                if (s2.Length == 2)
                    return true;
            }

            return false;
        }

        public string ToString(string format)
        {
            if (value == null || value == DBNull.Value)
                return NULL;

            StringBuilder sb = new StringBuilder();

            if (value is string)
            {
                //N: used for SQL Type nvarchar
                if (format != null || gb2312text(value as string))
                    sb.Append("N");

                sb.Append(DELIMETER)
                  .Append((value as string).Replace("'", "''"))
                  .Append(DELIMETER);
            }
            else if (value is bool || value is bool?)
            {
                sb.Append((bool)value ? "1" : "0");
            }
            else if (value is DateTime || value is DateTime?)
            {
                DateTime time = (DateTime)value;
                sb.Append(DELIMETER)
                  .AppendFormat("{0} {1}", time.ToString("d"), time.ToString("HH:mm:ss.fff"))
                  .Append(DELIMETER);
            }
            else if (value is char)
            {
                sb.Append(DELIMETER).Append(value).Append(DELIMETER);
            }
            else if (value is byte[])
            {
                sb.Append("0x" + BitConverter.ToString((byte[])value).Replace("-", ""));
                //sb.Append("0x" + Comparison.ColumnValue.ByteArrayToHexString((byte[])value));
            }
            else if (value is Guid)
                sb.Append("N" + DELIMETER).Append(value).Append(DELIMETER);
            else
            {
                sb.Append(value);
            }

            return sb.ToString();
        }

        public override string ToString()
        {
            return this.ToString(null);
        }

    }
}
