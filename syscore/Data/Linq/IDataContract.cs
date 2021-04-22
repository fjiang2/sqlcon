using System.Collections.Generic;
using System.Data;

namespace Sys.Data.Linq
{
    interface IDataContract<TEntity>
    {
        ITableSchema Schema { get; }
        //TEntity FromDictionary(IDictionary<string, object> dict);
        IDictionary<string, object> ToDictionary(TEntity entity);
        List<TEntity> ToList(DataTable dt);
    }
}