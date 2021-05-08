using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Reflection;

namespace Sys.Data
{
    public static class TypeExtension
    {
        public static DataSet CreateDataSet(this Assembly assembly)
        {
            return CreateDataSet(assembly, type => type.IsClass);
        }

        public static DataSet CreateDataSet(this Assembly assembly, Func<Type, bool> filter)
        {
            Type[] classes = assembly
                .GetTypes()
                .Where(filter)
                .ToArray();

            DataSet ds = new DataSet
            {
                DataSetName = assembly.GetName().Name,
            };

            foreach (Type clss in classes)
            {
                DataTable dt = CreateDataTable(clss);
                if (dt != null)
                    ds.Tables.Add(dt);
            }

            return ds;
        }

        public static DataTable CreateDataTable(this Type clss)
        {

            var properties = clss.GetProperties();
            if (properties.Length == 0)
                return null;

            DataTable dt = new DataTable
            {
                TableName = clss.Name,
            };

            foreach (var propertyInfo in properties)
            {
                Type type = propertyInfo.PropertyType;
                bool isNullable = Nullable.GetUnderlyingType(type) != null;
                if (isNullable)
                    type = Nullable.GetUnderlyingType(type);

                DataColumn column = new DataColumn(propertyInfo.Name, type)
                {
                    AllowDBNull = isNullable,
                    Unique = false,
                    AutoIncrement = false,
                };

                dt.Columns.Add(column);
            }

            return dt;
        }

        public static object SystemDefaultValue(this Type dataType)
        {
            if (!dataType.IsValueType)
                return System.DBNull.Value;

            switch (dataType.Name)
            {
                case "Int16":
                    return System.Convert.ToInt16(0);

                case "Int32":
                    return 0;

                case "String":
                    return "";

                case "Boolean":
                    return false;

                case "Double":
                    return 0.0;

                case "Decimal":
                    return new Decimal(0.0);

                case "DateTime":
                    return System.DateTime.MinValue;

            }

            throw new MessageException("Type {0} is not supported", dataType);

        }

    }
}
