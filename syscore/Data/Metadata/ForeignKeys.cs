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

namespace Sys.Data
{



    class ForeignKeys : IForeignKeys
    {
        private IForeignKey[] keys;

        public ForeignKeys(IForeignKey[] columns)
        {
            this.keys = columns;
        }

        public ForeignKeys(TableName tableName, ColumnCollection columns)
        {
            var fkeys = columns.Where(column => (column as ColumnSchema).IsForeignKey).ToArray();
            this.keys = new ForeignKey[fkeys.Length];

            int i = 0;
            foreach (var fk in fkeys)
                this.keys[i++] = new ForeignKey(tableName, fk);
        }

        public IForeignKey[] Keys
        {
            get
            {
                return this.keys;
            }
        }

        public int Length { get { return this.keys.Length; } }

        public override string ToString()
        {
            return string.Join<IForeignKey>(" + ", keys);
        }

    }


    class ForeignKey : IForeignKey
    {
        public TableName TableName { get; set; }

        public string FK_Schema { get { return this.TableName.SchemaName; } }
        public string FK_Table { get { return this.TableName.Name; } }
        public string FK_Column { get; set; }

        public string PK_Schema { get; set; }
        public string PK_Table { get; set; }
        public string PK_Column { get; set; }

        public string Constraint_Name { get; set; }



        public ForeignKey()
        {
        }

        public ForeignKey(TableName tableName, IColumn column)
        {
            this.TableName = tableName;

            ColumnSchema schema = (ColumnSchema)column;

            this.FK_Column = column.ColumnName;

            this.PK_Schema = schema.PK_Schema;
            this.PK_Table = schema.PK_Table;
            this.PK_Column = schema.PK_Column;
            this.Constraint_Name = schema.FkContraintName;
        }




        internal static string GetAttribute(IForeignKey key, Type pkTableType)
        {
            return GetAttribute(key, pkTableType.FullName);
        }

        internal static string GetAttribute(IForeignKey key, string dpoClassFullName)
        {
            return string.Format("[{0}(typeof({1}), {1}._{2})]",
                typeof(ForeignKeyAttribute).Name.Replace("Attribute", ""),
                dpoClassFullName,
                key.PK_Column);
        }

        public override bool Equals(object obj)
        {
            ForeignKey key = obj as ForeignKey;
            if (key == null)
                return false;

            return this.PK_Table.Equals(key.PK_Table)
                && this.PK_Column.Equals(key.PK_Column)
                && this.FK_Table.Equals(key.FK_Table)
                && this.FK_Column.Equals(key.FK_Column)
                ;
        }

        public override int GetHashCode()
        {
            if (Constraint_Name != null)
                return Constraint_Name.GetHashCode();
            else
                return base.GetHashCode();
        }

        public override string ToString()
        {
            return string.Format("TABLE {0} CONSTRAINT {1} FOREIGN KEY({2}) REFERENCES {3}({4})", FK_Table, Constraint_Name, FK_Column, PK_Table, PK_Column);
        }
    }
}
