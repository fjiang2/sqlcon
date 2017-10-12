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
using System.Data;
using System.Data.Common;
using System.Text;
using System.Reflection;

namespace Sys.Data
{

    public class ColumnAdapter 
    {

        public event ValueChangedHandler ValueChanged;
        
        private object originValue;
        protected object value;
        private string alias;

        private DataField field;

        /// Constructor
        public ColumnAdapter(DataField field)
        {
            this.field = field;
            this.alias = field.Name;

            this.originValue = System.DBNull.Value;
            this.value = System.DBNull.Value;
        }

        public DataField Field
        {
            get { return this.field; }
        }

        public override string ToString()
        {
            return string.Format("{0}={1}", field.Name, value);
        }

        public string Alias
        {
            set { this.alias = value; }
        }

     
        public void UpdateOriginValue(DataRow dataRow)
        {
            this.originValue = dataRow[alias];
        }

        public object Value
        {
            get { return this.value; }
        }


        public void UpdateValue(object val)
        {
            this.value = val;
            this.originValue = this.value;
        }

        public void UpdateValue(DataRow dataRow)
        {
            UpdateValue(dataRow[alias]);
        }

        public void UpdateDataRow(DataRow dataRow)
        {
            dataRow[alias] = value;
        }


        public void UpdateValue(PersistentObject dpo)
        {
            Type type = dpo.GetType();
            FieldInfo fieldInfo = type.GetField(field.Name);

            object val = fieldInfo.GetValue(dpo);
            Type ty = fieldInfo.FieldType;
            UpdateValue(val);
        }

        public void UpdateDpo(PersistentObject dpo)
        {
            Type type = dpo.GetType();
            FieldInfo fieldInfo = type.GetField(field.Name);
            fieldInfo.SetValue(dpo, this.value);
        }

      
        public virtual void AddParameter(SqlCmd sqlCmd)
        {
            DbParameter param = sqlCmd.DbProvider.AddParameter(field.ParameterName, field.DataType);
            if (value is DateTime)
            {
                DateTime SqlMinValue = new DateTime(1900, 1, 1);
                DateTime SqlMaxValue = new DateTime(9999, 12, 31);

                if ((DateTime)value < SqlMinValue)
                    value = SqlMinValue;

                else if ((DateTime)value > SqlMaxValue)
                    value = SqlMaxValue;
            }

            param.Value = value;
            param.Direction = ParameterDirection.Input;
        }


        public virtual DbParameter AddIdentityParameter(SqlCmd sqlCmd)
        {
            DbParameter param = sqlCmd.DbProvider.AddParameter(field.ParameterName, field.DataType);
            param.Value = value;

            if (value == System.DBNull.Value)
                param.Value = 0;

            param.Direction = ParameterDirection.Output;
            return param;
        }

        public bool IsValueChanged
        {
            get
            {
                if (originValue is byte[] && value is byte[])
                {
                    byte[] b1 = (byte[])originValue;
                    byte[] b2 = (byte[])value;
                    if (b1.Length != b2.Length)
                        return true;

                    for (int i = 0; i < b1.Length; i++)
                    {
                        if (b1[i] != b2[i])
                            return true;
                    }

                    return false;
                }

                string x1 = originValue.ToString().Trim();
                string x2 = value.ToString().Trim();

                return !x1.Equals(x2);
            }
        }

     

        protected Type dataType
        {
            get { return this.field.DataType; }
        }

        protected object Convert(object obj)
        {
            return InternalDataExtension.Convert(obj, dataType);
        }

        public void OnVauleChanged()
        {
            if (ValueChanged != null)
                ValueChanged(this, new ValueChangedEventArgs(field.Name, originValue, value));

            originValue = value;
        }


        public virtual void Fill()
        {
        }

        public virtual void Collect()
        {
        }

    


    }
}
