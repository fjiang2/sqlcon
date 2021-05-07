using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Sys.Collections
{
    /// <summary>
    /// Find the differences among collections
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public sealed class DifferenceList<T>
    {
        private const string _KEY = "KEY";
        private const string _VALUE = "VALUE";

        private DataTable dt;
        private bool isDirty = false;

        /// <summary>
        /// Use GetHashCode() as default key. 
        /// It can be used on default contructor
        /// </summary>
        public Func<T, int> PrimaryKey { get; set; } = item => item.GetHashCode();

        /// <summary>
        /// Use Equals() as default comparer
        /// It can be used on default contructor
        /// </summary>
        public Func<T, T, bool> Compare { get; set; } = (a, b) => a.Equals(b);

        private Action<T> onItemAdded;
        private Action<T> onItemDeleted;
        private Action<T, T> onItemModified;

        /// <summary>
        /// Func property PrimaryKey and Compare can be used here
        /// </summary>
        public DifferenceList()
        {
            dt = new DataTable
            {
                TableName = nameof(DifferenceList<T>)
            };

            DataColumn _key = new DataColumn(_KEY, typeof(int));
            DataColumn _value = new DataColumn(_VALUE, typeof(T));
            dt.Columns.Add(_key);
            dt.Columns.Add(_value);

            dt.PrimaryKey = new DataColumn[] { _key };
        }

        /// <summary>
        /// Add new items as comparison base. 
        /// typeof(T) must override int GetHashCode() and bool Equals() 
        /// Don't use Func property PrimaryKey and Compare here
        /// </summary>
        /// <param name="items"></param>
        public DifferenceList(IEnumerable<T> items)
            : this()
        {
            foreach (T item in items)
            {
                // Must use default PrimaryKey = item=>item.GetHashCode(), 
                // becuase user defined PrimaryKey is not ready in this code line.
                int key = PrimaryKey(item);         

                DataRow newRow = dt.NewRow();
                newRow[_KEY] = key;
                newRow[_VALUE] = item;
                dt.Rows.Add(newRow);

                dt.AcceptChanges();
            }
        }

        /// <summary>
        /// Compare and find differences
        /// </summary>
        /// <param name="items"></param>
        /// <returns>Original values before modified</returns>
        public T[] Differ(IEnumerable<T> items)
        {
            Dictionary<int, DataRow> dict = dt.Select().ToDictionary(row => (int)row[_KEY], row => row);
            List<int> keys = new List<int>();
            List<T> before = new List<T>();
            foreach (T item in items)
            {
                int key = PrimaryKey(item);

                keys.Add(key);

                if (!dict.ContainsKey(key))
                {
                    DataRow newRow = dt.NewRow();
                    newRow[_KEY] = key;
                    newRow[_VALUE] = item;
                    dt.Rows.Add(newRow);
                    onItemAdded?.Invoke(item);

                    isDirty = true;
                }
                else
                {
                    DataRow row = dict[key];
                    T old = (T)row[_VALUE];

                    if (!Compare(old, item))
                    {
                        row[_VALUE] = item;
                        isDirty = true;

                        before.Add(old);
                        onItemModified?.Invoke(old, item);
                    }
                }
            }

            foreach (DataRow row in dict.Values)
            {
                int key = (int)row[_KEY];
                if (keys.IndexOf(key) < 0)
                {
                    T item = (T)row[_VALUE];
                    onItemDeleted?.Invoke(item);
                    row.Delete();
                    isDirty = true;
                }
            }

            return before.ToArray();
        }

        public bool HasChanges => isDirty;

        public void Clear()
        {
            dt.Clear();
            isDirty = false;
        }

        public void Commit()
        {
            dt.AcceptChanges();
            isDirty = false;
        }

        public int Count => dt.Rows.Count;



        public T[] Added => GetItems(row => row.RowState == DataRowState.Added);
        public T[] BeforeModifying => GetItems(row => row.RowState == DataRowState.Modified, DataRowVersion.Original);
        public T[] Modified => GetItems(row => row.RowState == DataRowState.Modified);
        public T[] AddedOrModified => GetItems(row => row.RowState == DataRowState.Added || row.RowState == DataRowState.Modified);
        public T[] Deleted => GetItems(row => row.RowState == DataRowState.Deleted, DataRowVersion.Original);
        public T[] Unchanged => GetItems(row => row.RowState == DataRowState.Unchanged);
        public T[] Detached => GetItems(row => row.RowState == DataRowState.Detached);

        private T[] GetItems(Func<DataRow, bool> match, DataRowVersion version)
        {
            List<T> list = new List<T>();
            foreach (DataRow row in dt.Rows)
            {
                if (match(row))
                    list.Add((T)row[_VALUE, version]);
            }

            return list.ToArray();
        }

        private T[] GetItems(Func<DataRow, bool> match)
        {
            return dt.Select()
                .Where(row => match(row))
                .Select(row => (T)row[_VALUE])
                .ToArray();
        }





        public IDictionary<int, T> ToDictionary()
        {
            Dictionary<int, T> dict = new Dictionary<int, T>();
            foreach (DataRow row in dt.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                    continue;

                int key = (int)row[_KEY];
                if (!dict.ContainsKey(key))
                    dict.Add(key, (T)row[_VALUE]);
            }

            return dict;
        }

        public T[] ToArray() => GetItems(row => true);

        public bool Equals(IEnumerable<int> keys)
        {
            if (keys == null)
                return false;

            var dict = ToDictionary();

            if (keys.Count() != dict.Count)
                return false;

            foreach (int key in keys)
            {
                if (!dict.ContainsKey(key))
                    return false;
            }

            return true;
        }


        public bool Exists(T item)
        {
            int key = PrimaryKey(item);
            return Exists(key);
        }

        public bool Exists(int key)
        {
            foreach (DataRow row in dt.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                    continue;

                if (key == (int)row[_KEY])
                    return true;
            }

            return false;
        }

        public T Find(int key)
        {
            foreach (DataRow row in dt.Rows)
            {
                if (row.RowState == DataRowState.Deleted)
                    continue;

                if (key == (int)row[_KEY])
                    return (T)row[_VALUE];
            }

            return default(T);
        }

        public void OnItemAdded(Action<T> callback)
        {
            onItemAdded += callback;
        }

        public void OnItemModified(Action<T, T> callback)
        {
            onItemModified += callback;
        }

        public void OnItemDeleted(Action<T> callback)
        {
            onItemDeleted += callback;
        }

        public override string ToString()
        {
            return $"Count={Count}";
        }
    }
}

