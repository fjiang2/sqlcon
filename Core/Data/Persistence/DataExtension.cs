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
using System.Data.OleDb;
using Sys;

namespace Sys.Data
{
    public static class DataExtension
    {


        #region IsNull

        public static T IsNull<T>(this DataRow dataRow, string columnName, T defaultValue)
        {
            if (dataRow == null)
                return defaultValue;

            return dataRow[columnName] != System.DBNull.Value ? (T)dataRow[columnName] : defaultValue;
        }

        public static T IsNull<T>(this object value, T defaultValue)
        {
            if (value == null)
                return defaultValue;

            return value != System.DBNull.Value ? (T)value : defaultValue;
        }

        #endregion


        public static T Cell<T>(this DataTable dataTable, int row, int column, T defaultValue)
        {
            return dataTable.Rows[row][column].IsNull<T>(defaultValue);
        }

        public static T Cell<T>(this DataTable dataTable, int row, int column)
        {
            return (T)dataTable.Rows[row][column];
        }



        #region  DataTable.Rows[][x] -> Array[x]


        public static F[] ToArray<F>(this DataTable dataTable, string columnName)
        {
            F[] values = new F[dataTable.Rows.Count];
            int i = 0;
            foreach (DataRow row in dataTable.Rows)
            {
                values[i++] = (F)row[columnName];
            }
            return values;
        }

        public static F[] ToArray<F>(this DataTable dataTable, int columnIndex)
        {
            F[] values = new F[dataTable.Rows.Count];
            int i = 0;
            foreach (DataRow row in dataTable.Rows)
            {
                values[i++] = (F)row[columnIndex];
            }
            return values;
        }

        public static T[] ToArray<T>(this DataTable dataTable, Func<DataRow, T> func)
        {
            T[] values = new T[dataTable.Rows.Count];
            
            int i = 0;
            foreach (DataRow row in dataTable.Rows)
            {
                values[i++] = func(row);
            }
            
            return values;
        }


        //public static Dictionary<T1, T2> ToDictionary<T1, T2>(this DataTable dataTable, Func<DataRow, T1> keySelector, Func<DataRow, T2> elementSelector)
        //{
        //    Dictionary<T1, T2> dict = new Dictionary<T1, T2>();

        //    foreach (DataRow row in dataTable.Rows)
        //    {
        //        dict.Add(keySelector(row), elementSelector(row));
        //    }

        //    return dict;
        //}
        


        #endregion



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




        public static TableName TableName(this Type dpoType)
        {
            TableAttribute[] A = dpoType.GetAttributes<TableAttribute>();
            if (A.Length > 0)
                return A[0].TableName;
            else
                return null;
        }

        internal static Level Level(this Type dpoType)
        {
            TableAttribute[] A = dpoType.GetAttributes<TableAttribute>();
            if (A.Length > 0)
                return A[0].Level;

            throw new MessageException("Table Level is not defined");
        }

        internal static string FieldName(this string columnName)
        {
            string fieldName = columnName;
            if (columnName.IndexOf("#") != -1
                || columnName.IndexOf(" ") != -1
                || columnName.IndexOf("/") != -1
                || !Char.IsLetter(columnName[0]))
            {
                fieldName = columnName.Replace("#", "_").Replace(" ", "_").Replace("/", "_");

                if (!Char.IsLetter(columnName[0]))
                    fieldName = "_" + fieldName;
            }

            return fieldName;

        }


        internal static string Attribute(this IColumn column)
        {
            string attr = "";
            switch (column.CType)
            {
                case CType.VarBinary:
                case CType.Binary:
                    attr = string.Format(", Length = {0}", column.AdjuestedLength());
                    break;

                case CType.Char:
                case CType.VarChar:
                case CType.NChar:
                case CType.NVarChar:
                    int len = column.AdjuestedLength();
                    if(len != -1)
                        attr = string.Format(", Length = {0}", len);
                    break;


                //case CType.Numeric:
                case CType.Decimal:
                    attr = string.Format(", Precision = {0}, Scale = {1}", column.Precision, column.Scale);
                    break;


            }

            string attribute = string.Format("[Column(_{0}, CType.{1}", column.ColumnName.FieldName(), column.CType);

            if (column.Nullable)
                attribute += ", Nullable = true";   //see: bool Nullable = false; in class DataColumnAttribute

            if (column.IsIdentity)
                attribute += ", Identity = true";

            if (column.IsPrimary)
                attribute += ", Primary = true";

            if (column.IsComputed)
                attribute += ", Computed = true";

            if (attr != "")
                attribute += attr;

            attribute += ")]";

            return attribute;
        }


        internal static bool Oversize(this IColumn column, object value)
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
        internal static int AdjuestedLength(this IColumn column)
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




        public static void SqlDelete(this TableName tableName, string where, params object[] args)
        {
            string sql = string.Format("DELETE FROM {0} WHERE {1}", tableName, string.Format(where, args));
            DataExtension.ExecuteNonQuery(tableName.Provider, sql);
        }

        internal static object Convert(object obj, Type type)
        {
            if (obj == null)
                return System.DBNull.Value;

            string s = obj.ToString().Trim();

            if (type == typeof(string))
                return s;

            if (s == "")
                return System.DBNull.Value;


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





    

        #region ToINumerable<T>

        public static DataTable ToTable<T>(this IEnumerable<T> records) where T : class,  IDPObject, new()
        {
            DPList<T> list = new DPList<T>(records);
            return list.Table;
        }

        public static DPList<T> ToDPList<T>(this IEnumerable<T> collection) where T : class, IDPObject, new()
        {
            return new DPList<T>(collection);
        }

        public static DPList<T> ToDPList<T>(this TableReader<T> reader) where T : class, IDPObject, new()
        {
            return new DPList<T>(reader);
        }

        public static DPCollection<T> ToDPCollection<T>(this DPList<T> list) where T : class, IDPObject, new()
        {
            return new DPCollection<T>(list.Table);
        }


        #endregion


       

        internal static string SqlParameterName(this string name)
        {
            return "@" + name.Replace(" ", "").Replace("#", "");
        }


        #region Metadata Table pool
        private static DataPool<TableName, TableSchema> pool = new DataPool<TableName, TableSchema>(20);
        
        //return TableId ==-1 , ColumnId == -1
        internal static TableSchema GetTableSchema(this TableName tname)
        {
            return pool.GetItem(tname);
        }
        #endregion


        public static object GetValue(this DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
                return null;

            object value = row[columnName];

            if (value == System.DBNull.Value)
            {
                return null;
            }

            return value;
        }

        public static void SetValue(this DataRow row, string columnName, object value)
        {
            if (row.Table.Columns.Contains(columnName))
            {
                if (value == null)
                    row[columnName] = System.DBNull.Value;
                else
                    row[columnName] = value;
            }
        }

        /// <summary>
        /// Delete records
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="where"></param>
        public static void Delete<T>(this SqlExpr where) where T : class, IDPObject, new()
        {
            TableName tableName = typeof(T).TableName();
            DataExtension.ExecuteScalar(tableName.Provider,
                    "DELETE FROM {0} WHERE {1}",
                    tableName.FullName,
                    where);
        }


        //--------------------------------------------------------------------------------------
        public static object ExecuteScalar(this ConnectionProvider provider, string script, params object[] args)
        {
            SqlCmd cmd = new SqlCmd(provider, string.Format(script, args));
            return cmd.ExecuteScalar();
        }

        public static int ExecuteNonQuery(this ConnectionProvider provider, string script, params object[] args)
        {
            SqlCmd cmd = new SqlCmd(provider, string.Format(script, args));
            return cmd.ExecuteNonQuery();
        }

        public static DataTable FillDataTable(this string script, params object[] args)
        {
            return FillDataTable(ConnectionProviderManager.DefaultProvider, script, args);
        }

        public static DataTable FillDataTable<T>(this string script, params object[] args) where T : class, IDPObject, new()
        {
            TableName tableName = typeof(T).TableName();
            return FillDataTable(tableName.Provider, script, args);
        }

        public static DataTable FillDataTable(this ConnectionProvider provider, string script, params object[] args)
        {
            SqlCmd cmd = new SqlCmd(provider, string.Format(script, args));
            return cmd.FillDataTable();
        }

        public static DataRow FillDataRow(this ConnectionProvider provider, string script, params object[] args)
        {
            SqlCmd cmd = new SqlCmd(provider, string.Format(script, args));
            return cmd.FillDataRow();
        }


    }
}
