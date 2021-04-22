using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Data;

namespace Sys.Data.Linq
{
    class DataContract2<TEntity> : IDataContract<TEntity> where TEntity : IEntityRow, new()
    {
        private readonly Type type;
        public ITableSchema Schema { get; }

        public DataContract2()
        {
            this.type = typeof(TEntity);
            this.Schema = type.GetTableSchemaFromType();
        }

        public IDictionary<string, object> ToDictionary(TEntity entity)
        {
            return entity.ToDictionary();
        }

        //public TEntity FromDictionary(IDictionary<string, object> dict)
        //{
        //    var obj = new TEntity();
        //    obj.FromDictionary(dict);
        //    return obj;
        //}

        public List<TEntity> ToList(DataTable dt)
        {
            return System.Data.DataTableExtensions.AsEnumerable(dt)
            .Select(row =>
            {
                var obj = new TEntity();
                obj.FillObject(row);
                return obj;
            })
            .ToList();
        }

    }
}
