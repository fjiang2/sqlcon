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
    public static class DataExtension
    {

        public static T IsNull<T>(this object value, T defaultValue)
        {
            if (value is T)
                return (T)value;

            if (value == null || value == DBNull.Value)
                return defaultValue;

            throw new Exception($"{value} is not type of {typeof(T)}");
        }


        #region  DataTable.Rows[][x] -> Array[x]

        public static T[] ToArray<T>(this DataTable dataTable, string columnName)
        {
            return ToArray<T>(dataTable, row => (T)row[columnName]);
        }

        public static T[] ToArray<T>(this DataTable dataTable, int columnIndex = 0)
        {
            return ToArray<T>(dataTable, row => (T)row[columnIndex]);
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


        public static Dictionary<TKey, TValue> ToDictionary<TKey, TValue>(this DataTable dataTable, Func<DataRow, TKey> keySelector, Func<DataRow, TValue> valueSelector)
        {
            Dictionary<TKey, TValue> dict = new Dictionary<TKey, TValue>();

            foreach (DataRow row in dataTable.Rows)
            {
                TKey key = keySelector(row);
                TValue value = valueSelector(row);
                if (dict.ContainsKey(key))
                    dict[key] = value;
                else
                    dict.Add(key, value);
            }

            return dict;
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




        #region ToINumerable<T>

        public static DataTable ToTable<T>(this IEnumerable<T> records) where T : class, IDPObject, new()
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





        public static T GetField<T>(this DataRow row, string columnName)
        {
            if (!row.Table.Columns.Contains(columnName))
                return default(T);

            return IsNull<T>(row[columnName], default(T));
        }

        public static void SetField(this DataRow row, string columnName, object value)
        {
            if (row.Table.Columns.Contains(columnName))
            {
                if (value == null)
                    row[columnName] = DBNull.Value;
                else
                    row[columnName] = value;
            }
        }




    }


}
