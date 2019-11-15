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
using System.Data;
using System.Reflection;

namespace Sys.Data
{
    class RowObjectAdapter : RowAdapter
    {
        private PersistentObject obj;


        public RowObjectAdapter(PersistentObject obj)
            : this(obj, new Selector())
        {
            this.obj = obj;
        }

        public RowObjectAdapter(PersistentObject obj, Selector columnNames)
            : base( obj.TableName, obj.Locator, obj.NewRow)
        {
            this.obj = obj;
            this.transaction = obj.Transaction;
            Bind(columnNames);
            
        }

        private RowObjectAdapter Bind(Selector columnNames)
        {

            foreach (PropertyInfo propertyInfo in Reflex.GetColumnProperties(obj))
            {
                ColumnAttribute attribute = Reflex.GetColumnAttribute(propertyInfo);

                if (attribute != null && this.Row.Table.Columns.Contains(attribute.ColumnNameSaved))
                {
                    DataField field = this.fields.Add(attribute.ColumnNameSaved, attribute.Type);
                    ColumnAdapter column = new ColumnAdapter(field);
                    this.Bind(column);

                    column.Field.Identity = attribute.Identity;
                    column.Field.Primary = attribute.Primary;

                    if (attribute.Identity || attribute.Computed || ! columnNames.Exists(attribute.ColumnNameSaved))
                        column.Field.Saved = false;
                    else
                        column.Field.Saved = attribute.Saved;
                }
            }

            //in case of ColumnAttribute not setup Identity and Primary Keys
            fields.UpdatePrimaryIdentity(obj.Primary, obj.Identity);

            return this;
        }



        public void Apply()
        {
            foreach (PropertyInfo propertyInfo in Reflex.GetColumnProperties(obj))
            {
                ColumnAttribute a = Reflex.GetColumnAttribute(propertyInfo);
                if (a != null && this.Row.Table.Columns.Contains(a.ColumnNameSaved))
                {
                    if (propertyInfo.GetValue(obj, null) == null)
                        this.Row[a.ColumnNameSaved] = System.DBNull.Value;
                    else
                        this.Row[a.ColumnNameSaved] = propertyInfo.GetValue(obj, null);
                }

            }

            this.Fill();
        }




    }
}
