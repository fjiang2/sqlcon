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
using System.Text;
using System.Data;
using Tie;
using System.Reflection;

namespace Sys.Data
{
    public static class Conversion
    {
        public static VAL ToVAL(this DataRow dataRow)
        {
            VAL val = new VAL();
            foreach (DataColumn dataColumn in dataRow.Table.Columns)
            {
                val[dataColumn.ColumnName] = VAL.Boxing(dataRow[dataColumn.ColumnName]);
            }
            return val;

        }

        public static VAL ToVAL(this DataRow dataRow, VAL val)
        {
            foreach (DataColumn dataColumn in dataRow.Table.Columns)
            {
                val[dataColumn.ColumnName] = VAL.Boxing(dataRow[dataColumn.ColumnName]);
            }
            return val;

        }

        public static DataRow ToDataRow(this VAL val, DataRow dataRow)
        {
            foreach (DataColumn dataColumn in dataRow.Table.Columns)
            {
                if (val[dataColumn.ColumnName].Defined)
                {
                    object v = VAL.UnBoxing(val[dataColumn.ColumnName]);

                    DataColumnAssign(dataRow, dataColumn.ColumnName, v);
                }
            }

            return dataRow;
        }

        public static DataTable ToDataTable(this VAL val)
        {

            DataTable dataTable = new DataTable();

            for (int i = 0; i < val.Size; i++)
            {
                VAL field = val[i];
                VAL key = field[0];
                VAL value = field[1];
                Type ty;
                if (value.Value != null)
                    ty = value.Value.GetType();
                else
                    ty = typeof(string);

                DataColumn dataColumn = new DataColumn(key.Str, ty);
                dataTable.Columns.Add(dataColumn);
            }

            DataRow dataRow = dataTable.NewRow();
            ToDataRow(val, dataRow);
            dataTable.Rows.Add(dataRow);
            return dataTable;
        }









        public static void DataColumnAssign(DataRow dataRow, string columnName, object value)
        {
            if (value == null)
            {
                if (dataRow[columnName] != System.DBNull.Value)  //suppress RowChanged event handler invoke
                    dataRow[columnName] = System.DBNull.Value;
            }
            else
            {
                DataColumn dataColumn = dataRow.Table.Columns[columnName];
                if (dataColumn.DataType == value.GetType())
                {
                    if (!dataRow[columnName].Equals(value))     //suppress RowChanged event handler invoke
                        dataRow[columnName] = value;
                }
            }
        }

     


        public static object VAL2Class(VAL val, object instance)
        {
            FieldInfo[] fields = instance.GetType().GetFields();
            foreach (FieldInfo fieldInfo in fields)
            {

                if (fieldInfo.IsStatic)
                    continue;

                try
                {
                    VAL p = val[fieldInfo.Name];
                    if (p.Defined)
                    {
                     if(fieldInfo.FieldType == typeof(VAL))
                        fieldInfo.SetValue(instance, p);
                     else
                        fieldInfo.SetValue(instance, p.HostValue);
                    }
                }
                catch (ArgumentException)
                {
                }
            }

            PropertyInfo[] properties = instance.GetType().GetProperties();
            foreach (PropertyInfo propertyInfo in properties)
            {
                try
                {
                    VAL p = val[propertyInfo.Name];
                    if (p.Defined && propertyInfo.CanWrite)
                    {
                        if(propertyInfo.PropertyType == typeof(VAL))
                            propertyInfo.SetValue(instance, p, null);
                        else
                            propertyInfo.SetValue(instance, p.HostValue, null);
                    }
                }
                catch (ArgumentException)
                {
                }
            }

            return instance;
        }

        public static VAL Class2VAL(object instance, VAL val)
        {
            val["ClassName"] = new VAL(instance.GetType().FullName); 

            FieldInfo[] fields = instance.GetType().GetFields();
            foreach (FieldInfo fieldInfo in fields)
            {
                if (fieldInfo.IsStatic)
                    continue;

                Attribute[] attributes = CustomAttributeProvider.GetAttributes<NonValizedAttribute>(fieldInfo);
                if (attributes.Length != 0)
                    continue;

                attributes = CustomAttributeProvider.GetAttributes<AssociationAttribute>(fieldInfo);
                if (attributes.Length != 0)
                    continue;

                val[fieldInfo.Name] = VAL.Boxing(fieldInfo.GetValue(instance));
            }

            PropertyInfo[] properties = instance.GetType().GetProperties();
            foreach (PropertyInfo propertyInfo in properties)
            {
                ValizableAttribute[] attributes = CustomAttributeProvider.GetAttributes<ValizableAttribute>(propertyInfo);
                if (attributes.Length !=0 && propertyInfo.CanRead)
                    val[propertyInfo.Name] = VAL.Boxing(propertyInfo.GetValue(instance, null));
                else
                    continue;
            }

            return val;
        }

        public static VAL Class2VAL(object instance)
        {
            VAL val = new VAL();

            return Class2VAL(instance,val);
        }


     


    

    }
}
