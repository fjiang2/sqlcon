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
using System.IO;
using Sys.Data;
using System.Reflection;
using System.Linq;
using Tie;

namespace Sys.Data
{


    public class TableSchema : ITable
    {
        protected TableName tableName;

        public TableSchema(TableName tname)
        {
            this.tableName = tname;
        }

        public override string ToString()
        {
            return string.Format("{0}..[{1}], Id={2}", tableName.DatabaseName.Name, tableName.Name, TableID);
        }

        protected virtual void LoadSchema()
        {
            DataTable schema;
            schema = tableName.TableSchema();

            this._columns = new ColumnCollection(this);

            foreach (DataRow row in schema.Rows)
            {
                this._columns.Add(new ColumnSchema(row));
            }

        }

        protected ColumnCollection _columns = null;
        public ColumnCollection Columns
        {
            get
            {
                if (this._columns == null)
                    LoadSchema();

                return this._columns;
            }
        }


        #region PrimaryKey/ForeignKey/Identity/Computed column

        public IPrimaryKeys PrimaryKeys
        {
            get { return new PrimaryKeys(this.Columns); }
        }

        public IForeignKeys ForeignKeys
        {
            get { return new ForeignKeys(tableName, this.Columns); }
        }


        public IForeignKeys ByForeignKeys
        {
            get
            {
                return new Dependency(tableName.DatabaseName).ByForeignKeys(tableName);
            }
        }

        public IIdentityKeys Identity
        {
            get
            {
                if (tableName.Provider.Version >= 2005)
                    return new IdentityKeys(this.Columns);
                else
                    return new IdentityKeys();
            }
        }



        public ComputedColumns ComputedColumns
        {
            get { return new ComputedColumns(this.Columns); }
        }

        #endregion

        internal int _tableID = -1;
        public int TableID
        {
            get
            {
                return this._tableID;
            }
        }


        public int ColumnId(string columnName)
        {
            int[] L = this.Columns.Where(column => column.ColumnName == columnName).Select(column => column.ColumnID).ToArray();
            if (L.Length == 0)
                return -1;
            else
                return L[0];
        }


        public DataRow NewRow()
        {
            return EmptyDataTable.NewRow();
        }

        private DataTable _thisDataTable = null;
        public DataTable EmptyDataTable
        {
            get
            {
                if (_thisDataTable == null)
                {
                    _thisDataTable = DataExtension.FillDataTable(tableName.Provider, "SELECT TOP 1 * FROM {0}", tableName);
                    _thisDataTable.Rows.Clear();
                }

                return _thisDataTable;
            }
        }

        public bool Exists
        {
            get
            {
                return tableName.Exists();
            }
        }


        public TableName TableName
        {
            get { return this.tableName; }
        }


        //------------------------------------------------------------------------------------
        public Locator DefaultLocator()
        {
            return new Locator(this.PrimaryKeys);
        }

    }


}
