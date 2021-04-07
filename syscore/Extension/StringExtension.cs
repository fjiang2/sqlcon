using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;

namespace Sys
{
    public static class StringExtension
    {
        #region Hex <---> String

        /// <summary>
        /// Utility function:
        ///     conver string into byte array
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] HexStringToByteArray(String hexString)
        {
            int numberChars = hexString.Length;
            byte[] bytes = new byte[numberChars / 2];
            for (int i = 0; i < numberChars; i += 2)
            {
                bytes[i / 2] = Convert.ToByte(hexString.Substring(i, 2), 16);
            }

            return bytes;
        }

        /// <summary>
        /// Utility function:
        ///     convert byte array into string
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string ByteArrayToHexString(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];
            byte b;
            for (int i = 0; i < bytes.Length; ++i)
            {
                b = ((byte)(bytes[i] >> 4));
                c[i * 2] = (char)(b > 9 ? b + 0x37 : b + 0x30);

                b = ((byte)(bytes[i] & 0xF));
                c[i * 2 + 1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            }

            return new string(c);
        }


        #endregion

        #region String Encrypt/Decrypt

        public static string Encrypt(this string jsonString)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            byte[] bytes = encoding.GetBytes(jsonString);
            return ByteArrayToHexString(bytes);
        }

        public static string Decrypt(this string hexString)
        {
            byte[] bytes = HexStringToByteArray(hexString);
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            return encoding.GetString(bytes);
        }

        public static byte[] GetBytes(this string str)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            return encoding.GetBytes(str);
        }

        public static string GetString(this byte[] bytes)
        {
            System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();
            return encoding.GetString(bytes);
        }

        #endregion

        public static string ToSentence(this string sent)
        {
            return string.Join(" ", 
                sent.Trim().Split(new char[] { ' ' })
                .Where(word => word !="")
                .Select(word => word.Substring(0, 1).ToUpper() + word.Substring(1).ToLower())
                );
        }

        /// <summary>
        /// remove left and right letters of a string 
        /// </summary>
        /// <param name="value"></param>
        /// <param name="left">number of letter on left</param>
        /// <param name="right">number of letter on right</param>
        /// <returns></returns>
        public static string Trim(this string value, int left, int right)
        {
            if (value == null)
                throw new ArgumentException("string cannot be null");

            int len = value.Length;

            if (left > len - 1)
                throw new ArgumentException("left is out of range");

            if (right > len - 1)
                throw new ArgumentException("left is out of range");

            if (left + right > len - 1)
                throw new ArgumentException("left+right is out of range"); 

            return value.Substring(left, len - left - right);
        }


        public static string ToSimpleString(this IDictionary dict)
        {
            StringBuilder builder = new StringBuilder("{");
            int i = 0;
            foreach (var key in dict.Keys)
            {
                object val = dict[key];
                builder
                    .Append(key)
                    .Append(":")
                    .Append(ToSimpleString(val));

                if (i++ < dict.Count - 1)
                    builder.Append(",");
            }

            builder.Append("}");
            return builder.ToString();
        }

        public static string ToSimpleString(this IEnumerable source)
        {
            StringBuilder builder = new StringBuilder("[");
            int i = 0;
            foreach (var item in source)
            {
                if (i++ > 0)
                    builder.Append(",");

                builder.Append(ToSimpleString(item));
            }

            builder.Append("]");
            return builder.ToString();
        }


        public static string ToSimpleString(this DataSet set)
        {
            if (set.Tables.Count == 1)
                return ToSimpleString(set.Tables[0]);

            StringBuilder builder = new StringBuilder("{");

            int i = 0;
            foreach (DataTable table in set.Tables)
            {
                if (i++ > 0)
                    builder.Append(",");

                builder.Append(table.TableName).Append(":");
                builder.Append(ToSimpleString(table));
            }

            builder.Append("}");
            return builder.ToString();
        }

        public static string ToSimpleString(this DataTable table)
        {
            StringBuilder builder = new StringBuilder();
            int i = 0;
            int n = table.Rows.Count;

            if (n > 1)
                builder.Append("[");

            foreach (DataRow row in table.Rows)
            {
                builder.Append(ToSimpleString(row));
                if (i++ < n - 1)
                    builder.Append(",");
            }

            if (n > 1)
                builder.Append("]");

            return builder.ToString();
        }

        public static string ToSimpleString(this DataRow row)
        {
            StringBuilder builder = new StringBuilder();
            int i = 0;

            var columns = row.Table.Columns;
            int m = columns.Count;
            builder.Append("{");
            for (int k = 0; k < m; k++)
            {
                string key = columns[k].ColumnName;
                object val = row[k];

                builder
                    .Append(key)
                    .Append(":")
                    .Append(ToSimpleString(val));

                if (i++ < m - 1)
                    builder.Append(",");
            }

            builder.Append("}");

            return builder.ToString();
        }

        public static string ToSimpleString(this Enum obj)
        {
            return obj.ToString().Replace(", ", "|");
        }

        public static string ToSimpleString(this object val)
        {
            if (val == null)
                return "null";

            if (val is DBNull)
                return "NULL";
            else if (val is string)
                return "\"" + val + "\"";
            else if (val is DateTime)
                return "\"" + val + "\"";
            else if (val is Tie.VAL)
                return (val as Tie.VAL).ToSimpleString();
            else if (val is IDictionary)
                return ToSimpleString((IDictionary)val);
            else if (val is IEnumerable)
                return ToSimpleString((IEnumerable)val);


            return val.ToString();
        }


    }
}
