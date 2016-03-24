using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Sys.Data
{
    public class DataContractTable<T> where T : IDataContractRow<T>, new()
    {
        private DataTable dt;

        public DataContractTable(DataTable dt)
        {
            this.dt = dt;
        }

        public DataTable Table
        {
            get { return this.dt; }
        }

        public List<T> ToList()
        {
            return dt.AsEnumerable()
            .Select(row => NewObject(row))
            .ToList();
        }

        public void ToTable(IEnumerable<T> items, DataTable dt)
        {
            foreach (var item in items)
            {
                var row = dt.NewRow();
                item.UpdateRow(row);
                dt.Rows.Add(row);
            }

            dt.AcceptChanges();
        }

        public T NewObject(DataRow row)
        {
            var item = new T();
            item.UpdateObject(row);
            return item;
        }


        public void ForEach(Action<T> action)
        {
            foreach (DataRow row in dt.Rows)
            {
                var item = NewObject(row);
                action(item);
                item.UpdateRow(row);
            }
        }

        public IEnumerable<TResult> ForEach<TResult>(Func<T, TResult> func)
        {
            List<TResult> list = new List<TResult>();
            foreach (DataRow row in dt.Rows)
            {
                var item = NewObject(row);
                TResult t = func(item);
                list.Add(t);
            }
            return list;
        }

    }
}
