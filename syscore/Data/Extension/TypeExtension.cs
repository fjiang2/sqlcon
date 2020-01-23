﻿using System;
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
                ds.Tables.Add(dt);
            }

            return ds;
        }

        public static DataTable CreateDataTable(this Type clss)
        {
            DataTable dt = new DataTable
            {
                TableName = clss.Name,
            };

            foreach (var propertyInfo in clss.GetProperties())
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
    }
}
