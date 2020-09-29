using System;
using System.Reflection;

namespace Sys.Data.Linq
{
    static class TableSchemaExtension
    {
        public static ITableSchema GetTableSchemaFromExtensionType(this Type extension)
        {
            string schemaName = extension.GetStaticField("SchemaName", "dbo");
            string tableName = extension.GetStaticField("TableName", string.Empty);
            string[] keys = extension.GetStaticField("Keys", new string[] { });
            string[] identity = extension.GetStaticField("Identity", new string[] { });

            string[] items = tableName.Split('.');
            if (items.Length >= 2)
            {
                schemaName = items[0];
                tableName = items[1];
            }

            return new TableSchema
            {
                SchemaName = schemaName,
                TableName = tableName,
                PrimaryKeys = keys,
                IdentityKeys = identity,
            };
        }

        private static T GetStaticField<T>(this Type type, string name, T defaultValue = default(T))
        {
            var fieldInfo = type.GetField(name, BindingFlags.Public | BindingFlags.Static);
            if (fieldInfo != null)
                return (T)fieldInfo.GetValue(null);
            else
                return defaultValue;
        }
    }
}
