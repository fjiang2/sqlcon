using System;
using System.Collections.Generic;
using System.Data;

namespace Sys.Data.Linq
{
    interface IDataContract<TEntity>
    {
        ITableSchema Schema { get; }
        ITableSchema GetSchmea(Type type);
        //TEntity FromDictionary(IDictionary<string, object> dict);
        IDictionary<string, object> ToDictionary(TEntity entity);
        List<TEntity> ToList(DataTable dt);
    }
}