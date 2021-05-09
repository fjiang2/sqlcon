using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Data;

namespace Sys.Data.Linq
{

    class BrokerOfDataContract2<TEntity> : IDataContractBroker<TEntity>
    {
        private readonly Type type;
        public ITableSchema Schema { get; }

        public BrokerOfDataContract2()
        {
            this.type = typeof(TEntity);
            this.Schema = type.GetTableSchemaFromType();
        }

        public ITableSchema GetSchmea(Type type)
        {
            return type.GetTableSchemaFromType();
        }

        public IDictionary<string, object> ToDictionary(TEntity entity)
        {
            return (entity as IEntityRow).ToDictionary();
        }


        public List<TEntity> ToList(DataTable dt)
        {
            return System.Data.DataTableExtensions.AsEnumerable(dt)
            .Select(row => (TEntity)Activator.CreateInstance(type, new object[] { row }))
            .ToList();
        }

    }
}
