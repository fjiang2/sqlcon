using System;
using System.Reflection;
using Tie;

namespace Sys.Data.Linq
{
    static class TableSchemaExtension
    {
        private const string EXTENSION = "Extension";

        public static ITableSchema GetTableSchema(this Type type, out Type extension)
        {
            extension = HostType.GetType(type.FullName + EXTENSION);
            return extension.GetTableSchemaFromExtensionType();
        }

        private static ITableSchema GetTableSchemaFromExtensionType(this Type extension)
        {
            string schemaName = extension.GetStaticField(nameof(ITableSchema.SchemaName), TableName.dbo);
            string tableName = extension.GetStaticField(nameof(ITableSchema.TableName), string.Empty);
            string[] keys = extension.GetStaticField("Keys", new string[] { });
            string[] identity = extension.GetStaticField("Identity", new string[] { });
            IAssociation[] associations = extension.GetStaticField(nameof(ITableSchema.Associations), new IAssociation[] { });

            return new TableSchema
            {
                SchemaName = schemaName,
                TableName = tableName,
                PrimaryKeys = keys,
                IdentityKeys = identity,
                Associations = associations,
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
            return $"[{schema.SchemaName}].[{schema.TableName}]";
        }
    }
}
