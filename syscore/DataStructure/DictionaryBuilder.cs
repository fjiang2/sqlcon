using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Sys
{
    public class DictionaryBuilder
    {
        private Dictionary<string, object> dict = new Dictionary<string, object>();

        public DictionaryBuilder()
        {
        }

        public DictionaryBuilder Add(string key, object value)
        {
            if (this.dict.ContainsKey(key))
                this.dict[key] = value;
            else
                this.dict.Add(key, value);

            return this;
        }

        public DictionaryBuilder AddRange(IDictionary<string, object> dict)
        {
            if (dict == null)
                throw new ArgumentNullException();

            foreach (var kvp in dict)
            {
                Add(kvp.Key, kvp.Value);
            }

            return this;
        }

        public DictionaryBuilder AddRange(object obj)
        {
            if (obj == null)
                throw new ArgumentNullException();

            foreach (var propertyInfo in obj.GetType().GetProperties())
            {
                Add(propertyInfo.Name, propertyInfo.GetValue(obj));
            }

            return this;
        }

        public void AddRange(DataRow row)
        {
            foreach (DataColumn column in row.Table.Columns)
            {
                Add(column.ColumnName, row[column]);
            }
        }

        public IDictionary<string, object> ToDictionary() => dict;

        public void Clear()
        {
            dict.Clear();
        }

        public override string ToString()
        {
            return dict.ToString();
        }
    }
}
