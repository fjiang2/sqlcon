using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;
using System.Data;
using Tie;

namespace Sys.Data.Linq
{
    class BrokerOfDataContract1<TEntity> : IDataContractBroker<TEntity>
    {
        private const string EXTENSION = "Extension";

        private readonly Type type;
        private readonly Type extension;
        private readonly MethodInfo functionToDictionary;
        public ITableSchema Schema { get; }

        public BrokerOfDataContract1()
        {
            this.type = typeof(TEntity);
            this.extension = HostType.GetType(type.FullName + EXTENSION);

            this.Schema = extension.GetTableSchemaFromType();
            this.functionToDictionary = extension.GetMethod(nameof(ToDictionary), BindingFlags.Public | BindingFlags.Static);
        }

        public ITableSchema GetSchmea(Type type)
        {
            var extension = HostType.GetType(type.FullName + EXTENSION);
            return extension.GetTableSchemaFromType();
        }

        private object Invoke(string name, object[] parameters)
        {
            var methodInfo = extension.GetMethod(name, BindingFlags.Public | BindingFlags.Static);
            if (methodInfo != null)
                return methodInfo.Invoke(null, parameters);

            return null;
        }

        private static T Invoke<T>(MethodInfo methodInfo, params object[] parameters)
        {
            if (methodInfo != null)
                return (T)methodInfo.Invoke(null, parameters);

            return default(T);
        }

        public IDictionary<string, object> ToDictionary(TEntity entity)
        {
            return Invoke<IDictionary<string, object>>(functionToDictionary, entity);
        }

        public List<TEntity> ToList(DataTable dt)
        {
            object obj = Invoke($"To{type.Name}Collection", new object[] { dt });
            return (List<TEntity>)obj;
        }

    }
}
