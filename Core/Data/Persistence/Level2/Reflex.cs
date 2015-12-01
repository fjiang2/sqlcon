//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        DPO(Data Persistent Object)                                                               //
//                                                                                                  //
//          Copyright(c) Datum Connect Inc.                                                         //
//                                                                                                  //
// This source code is subject to terms and conditions of the Datum Connect Software License. A     //
// copy of the license can be found in the License.html file at the root of this distribution. If   //
// you cannot locate the  Datum Connect Software License, please send an email to                   //
// datconn@gmail.com. By using this source code in any fashion, you are agreeing to be bound        //
// by the terms of the Datum Connect Software License.                                              //
//                                                                                                  //
// You must not remove this notice, or any other, from this software.                               //
//                                                                                                  //
//                                                                                                  //
//--------------------------------------------------------------------------------------------------//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Data;

namespace Sys.Data
{
    class Reflex
    {

        public static PropertyInfo[] GetColumnProperties(object instance)
        {
            return GetColumnProperties(instance.GetType());
        }

        public static PropertyInfo[] GetColumnProperties(Type type)
        {
             PropertyInfo[] properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);    //ignore public const fields

            List<PropertyInfo> list = new List<PropertyInfo>();
             foreach (PropertyInfo propertyInfo in properties)
             {
                 if (GetColumnAttribute(propertyInfo) != null)
                     list.Add(propertyInfo);
             }

             return list.ToArray();
        }


        public static ColumnAttribute GetColumnAttribute(DataRow dataRow, PropertyInfo propertyInfo)
        {

            ColumnAttribute attribute = GetColumnAttribute(propertyInfo);

            if (attribute != null && dataRow.Table.Columns.Contains(attribute.ColumnName))
            {
                if (dataRow.Table.Columns[attribute.ColumnName].DataType == propertyInfo.PropertyType)
                    return attribute;

                if (propertyInfo.PropertyType.IsGenericType
                  && propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>)
                  && propertyInfo.PropertyType.GetGenericArguments()[0] == dataRow.Table.Columns[attribute.ColumnName].DataType)
                    return attribute;
            }


            return null;
        }


        public static ColumnAttribute GetColumnAttribute(PropertyInfo propertyInfo)
        {
            ColumnAttribute[] attributes = CustomAttributeProvider.GetAttributes<ColumnAttribute>(propertyInfo);

            foreach (ColumnAttribute attribute in attributes)
            {
                if (attribute.ColumnName == String.Empty)
                    attribute.ColumnName = propertyInfo.Name;

                if (attribute.CType == CType.Auto)
                    attribute.CType = propertyInfo.PropertyType.ToCType();

                return attribute;
            }

            if (attributes.Length == 0)
            {
                Attribute[] non = CustomAttributeProvider.GetAttributes<NonPersistentAttribute>(propertyInfo);

                if (non.Length > 0)
                    return null;

                non = CustomAttributeProvider.GetAttributes<AssociationAttribute>(propertyInfo);

                if (non.Length > 0)
                    return null;

                //no other attribute defined.
                if (propertyInfo.CanRead && propertyInfo.CanWrite)
                {
                    try
                    {
                        return new ColumnAttribute(propertyInfo.Name, propertyInfo.PropertyType.ToCType());
                    }
                    catch (Exception)
                    {
                        return null;    //some type is not supported
                    }
                }

            }

            return null;
        }

        public static AssociationAttribute GetAssociationAttribute(PropertyInfo propertyInfo)
        {
            AssociationAttribute[] attributes = CustomAttributeProvider.GetAttributes<AssociationAttribute>(propertyInfo);

            foreach (AssociationAttribute attribute in attributes)
                return attribute;

            return null;
        }

        public static void FillInstance(object instance, DataRow dataRow, bool defaultValueUsed)
        {

            foreach (PropertyInfo propertyInfo in Reflex.GetColumnProperties(instance))
            {
                FillField(instance, propertyInfo, dataRow, defaultValueUsed);
            }
        }


        public static object FillField(object instance, PropertyInfo propertyInfo, DataRow dataRow, bool defaultValueUsed)
        {
            ColumnAttribute a = Reflex.GetColumnAttribute(dataRow, propertyInfo);
            if (a != null)
            {
                object value = dataRow[a.ColumnName];

                if (value == System.DBNull.Value)
                {
                    if (a.DefaultValue != null)
                    {
                        value = a.DefaultValue;
                    }
                    else if (defaultValueUsed)
                    {
                        Type dataType = dataRow.Table.Columns[a.ColumnName].DataType;
                        value = DefaultRowValue.SystemDefaultValue(dataType);
                    }
                    else
                        value = null;
                }

                propertyInfo.SetValue(instance, value, null);
            }

            return a;
        }


        public static DataTable GetEmptyDataTable<T>() where T : class, IDPObject
        {
            return GetEmptyDataTable(typeof(T));
        }


        /// <summary>
        /// Create Empty System.Data.DataTable from ColumnAttribute of DPO fields
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        private static DataTable GetEmptyDataTable(Type type)
        {
            DataTable dt = new DataTable();
            foreach (PropertyInfo propertyInfo in Reflex.GetColumnProperties(type))
            {
                ColumnAttribute a = Reflex.GetColumnAttribute(propertyInfo);
                if (a == null)
                    continue;

                Type ty = propertyInfo.PropertyType.InnullableType();
                DataColumn column = new DataColumn(a.ColumnName, ty);
                column.AllowDBNull = a.Nullable;
                column.AutoIncrement = a.Identity;
                column.Caption = a.Caption;

                //may need complicated logic for differentg type
                if (ty == typeof(string))
                    column.MaxLength = a.Length;    

                dt.Columns.Add(column);
            }

            return dt;
        }

    }
}
