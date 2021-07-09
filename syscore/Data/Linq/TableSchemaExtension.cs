using System;
using System.Reflection;

namespace Sys.Data.Linq
{
    static class TableSchemaExtension
    {
        public static ITableSchema GetTableSchemaFromType(this Type extension)
        {
            string schemaName = extension.GetStaticField(nameof(ITableSchema.SchemaName), SchemaName.dbo);
            string tableName = extension.GetStaticField(nameof(ITableSchema.TableName), string.Empty);
            string[] keys = extension.GetStaticField("Keys", new string[] { });
            string[] identity = extension.GetStaticField("Identity", new string[] { });
            IConstraint[] associations = extension.GetStaticField(nameof(ITableSchema.Constraints), new IConstraint[] { });

            return new TableSchema
            {
                SchemaName = schemaName,
                TableName = tableName,
                PrimaryKeys = keys,
                IdentityKeys = identity,
                Constraints = associations,
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

        public static string FormalTableName(this ITableSchema schema)
        {
            if (schema.SchemaName == SchemaName.dbo || schema.SchemaName == SchemaName.empty)
                return string.Format("[{0}]", schema.TableName);
            else
                return $"[{schema.SchemaName}].[{schema.TableName}]";
        }
    }
}
