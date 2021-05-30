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





        public static T GetField<T>(this DataRow row, string columnName, T defaultValue = default(T))
        {
            if (!row.Table.Columns.Contains(columnName))
                return defaultValue;

            return IsNull<T>(row[columnName], defaultValue);
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


        public static List<T> ToList<T>(this DataRow row, Func<int, object, T> func)
        {
            List<T> list = new List<T>();
            int i = 0;
            foreach (var obj in row.ItemArray)
            {
                list.Add(func(i, obj));
                i++;
            }

            return list;
        }

        public static List<T> ToList<T>(this DataRow row, Func<object, T> func)
        {
            List<T> list = new List<T>();
            foreach (var obj in row.ItemArray)
            {
                list.Add(func(obj));
            }

            return list;
        }



        public static List<T> ToList<T>(this DataTable dt, Func<int, DataRow, bool> where, Func<int, DataRow, T> select)
        {
            List<T> list = new List<T>();
            int i = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (where(i, row))
                    list.Add(select(i, row));
                i++;
            }

            return list;
        }

        public static List<T> ToList<T>(this DataTable dt, Func<DataRow, bool> where, Func<DataRow, T> select)
        {
            return dt.ToList((_, row) => where(row), (_, row) => select(row));
        }

        public static List<T> ToList<T>(this DataTable dt, Func<int, DataRow, T> select)
        {
            return dt.ToList((rowId, row) => true, (rowId, row) => select(rowId, row));
        }

        public static List<T> ToList<T>(this DataTable dt, Func<DataRow, T> select)
        {
            return dt.ToList((rowId, row) => true, (rowId, row) => select(row));
        }

        public static IEnumerable<DataLine> Where(this DataTable dt, Func<DataLine, bool> where)
        {
            return dt.ToLines().Where(where);
        }

        public static IEnumerable<DataLine> ToLines(this DataTable dt)
        {
            int i = 0;
            foreach (DataRow row in dt.Rows)
            {
                yield return new DataLine { Line = i, Row = row };
                i++;
            }
        }

        public static void Insert(this DataTable dt, Action<DataRow> insert)
        {
            DataRow row = dt.NewRow();
            insert(row);
            dt.Rows.Add(row);

            dt.AcceptChanges();
        }

        public static int Update(this DataTable dt, Action<DataRow> update)
        {
            return Update(dt, (_, row) => true, (_, row) => update(row));
        }

        public static int Update(this DataTable dt, Func<DataRow, bool> where, Action<DataRow> update)
        {
            return Update(dt, (_, row) => where(row), (_, row) => update(row));
        }

        public static int Update(this DataTable dt, Action<int, DataRow> update)
        {
            return Update(dt, (_, row) => true, update);
        }

        public static int Update(this DataTable dt, Func<int, DataRow, bool> where, Action<int, DataRow> update)
        {
            int count = 0;
            int i = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (where(i, row))
                {
                    update(i, row);
                    count++;
                }
                i++;
            }

            return count;
        }

        public static int Delete(this DataTable dt, Func<DataRow, bool> where)
        {
            return Delete(dt, (_, row) => where(row));
        }

        public static int Delete(this DataTable dt, Func<int, DataRow, bool> where)
        {
            int count = 0;
            int i = 0;
            foreach (DataRow row in dt.Rows)
            {
                if (where(i, row))
                {
                    row.Delete();
                    count++;
                }
                i++;
            }

            dt.AcceptChanges();
            return count;
        }


    }

    public class DataLine
    {
        public int Line { get; set; }
        public DataRow Row { get; set; }
    }

}
