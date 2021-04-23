using System;
using System.Collections.Generic;
using System.Data;

namespace Sys.Data.Linq
{
    interface IDataContractBroker<TEntity>
    {
        ITableSchema Schema { get; }
        ITableSchema GetSchmea(Type type);
        IDictionary<string, object> ToDictionary(TEntity entity);
        List<TEntity> ToList(DataTable dt);
    }
}