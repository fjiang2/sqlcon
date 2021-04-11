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
using System.Data;

namespace Sys.Data
{
    static class InternalDataExtension
    {

        #region DataRow CopyTo/Clone/EqualTo
        public static DataRow CopyTo(this DataRow src, DataRow dst)
        {
            foreach (DataColumn c in src.Table.Columns)
                dst[c.ColumnName] = src[c.ColumnName];

            return dst;
        }

        public static DataRow Clone(this DataRow src)
        {
            DataRow dst = src.Table.NewRow();
            foreach (DataColumn c in src.Table.Columns)
                dst[c.ColumnName] = src[c.ColumnName];

            return dst;
        }


        public static bool EqualTo(this DataRow r1, DataRow r2)
        {
            return EqualTo(r1, r2, new string[] { });
        }

        /// <summary>
        /// Compare 2 rows 
        /// </summary>
        /// <param name="r1"></param>
        /// <param name="r2"></param>
        /// <returns></returns>
        public static bool EqualTo(this DataRow r1, DataRow r2, string[] ignoredColumns)
        {
            if (r1.Table.Columns.Count != r2.Table.Columns.Count)
                return false;

            foreach (DataColumn column in r2.Table.Columns)
            {
                string x = column.ColumnName;

                if (Array.IndexOf(ignoredColumns, x) >= 0)
                    continue;

                if (!r1.Table.Columns.Contains(x))
                    return false;

                object v1 = r1[x];
                object v2 = r2[x];

                if ((v1 == null && v2 != null) || (v1 != null && v2 == null))
                    return false;
                else if (v1 == null && v2 == null)
                    continue;

                if (v1.GetType() != v2.GetType())
                    return false;

                if (v1 is byte[] && v2 is byte[])
                {
                    byte[] b1 = (byte[])v1;
                    byte[] b2 = (byte[])v2;
                    if (b1.Length != b2.Length)
                        return false;

                    for (int i = 0; i < b1.Length; i++)
                    {
                        if (b1[i] != b2[i])
                            return false;
                    }
                }
                else if (!v1.ToString().Equals(v2.ToString()))
                {
                    return false;
                }
            }

            return true;

        }

        #endregion


        public static Level Level(this Type dpoType)
        {
            TableAttribute[] A = dpoType.GetAttributes<TableAttribute>();
            if (A.Length > 0)
                return A[0].Level;

            throw new MessageException("Table Level is not defined");
        }



        public static bool Oversize(this IColumn column, object value)
        {
            if (!(value is string))
                return false;

            if (column.CType == CType.NText || column.CType == CType.Text)
                return false;

            string s = (string)value;

            if (column.Length == -1)
            {
                if (column.CType == CType.NVarChar || column.CType == CType.NChar)
                    return s.Length > 4000;
                else
                    return s.Length > 8000;
            }
            else
                return s.Length > column.AdjuestedLength();
        }


        /// <summary>
        /// Adjuested Length
        /// </summary>
        public static int AdjuestedLength(this IColumn column)
        {
            if (column.Length == -1)
                return -1;

            switch (column.CType)
            {
                case CType.NChar:
                case CType.NVarChar:
                    return column.Length / 2;
            }

            return column.Length;
        }



        public static object Convert(object obj, Type type)
        {
            if (obj == null)
                return DBNull.Value;

            string s = obj.ToString().Trim();

            if (type == typeof(string))
                return s;

            if (s == "")
                return DBNull.Value;


            string g = "";
            int i = 0;

            if (type == typeof(int) || type == typeof(short) || type == typeof(bool))
            {
                if (s[0] == '-') g = "-";
                for (i = 0; i < s.Length; i++)
                {
                    if (s[i] >= '0' && s[i] <= '9')
                        g += s[i];
                    if (s[i] == '.')
                        break;
                }

                int result = int.Parse(g);

                if (type == typeof(bool))
                    return result != 0;
                else
                    return result;
            }
            else if (type == typeof(decimal) || type == typeof(double))
            {
                bool dot = false;
                if (s[0] == '-') g = "-";
                for (i = 0; i < s.Length; i++)
                {
                    if (s[i] >= '0' && s[i] <= '9')
                        g += s[i];
                    else if (s[i] == '.' && !dot)
                    {
                        g += s[i];
                        dot = true;
                    }
                }

                if (type == typeof(double))
                    return double.Parse(g);
                else
                    return decimal.Parse(g);
            }

            else if (type == typeof(DateTime))
            {
                // "2/16/1992 12:15:12"
                return DateTime.Parse(s);
            }

            else
                throw new ApplicationException("Data Type in Convert Function is not defined.");

        }



        public static string SqlParameterName(this string name)
        {
            return "@" + name.Replace(" ", "").Replace("#", "");
        }


        #region Metadata Table pool
        private static DataPool<TableName, TableSchema> pool = new DataPool<TableName, TableSchema>(20);

        //return TableId ==-1 , ColumnId == -1
        public static TableSchema GetTableSchema(this TableName tname)
        {
            return pool.GetItem(tname);
        }
        #endregion


        /// <summary>
        /// Delete records
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        public static void Delete<T>(this SqlExpr where) where T : class, IDPObject, new()
        {
            TableName tableName = typeof(T).TableName();
            tableName.Provider.ExecuteScalar($"DELETE FROM {tableName} WHERE {where}");
        }

    }
}
