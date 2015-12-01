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
   

    public class MetaTable : ITable
    {
        protected TableName tableName;
        
        public MetaTable(TableName tname)
        {
            this.tableName = tname;
        }

        public override string ToString()
        {
            return string.Format("{0}..[{1}], Id={2}", tableName.DatabaseName.Name, tableName.Name, TableID);
        }

        protected virtual void LoadSchema()
        {
            DataTable schema = InformationSchema.TableSchema(tableName);

            this._columns = new ColumnCollection(this);

            foreach (DataRow row in schema.Rows)
            {
                this._columns.Add(new MetaColumn(row));
            }

            this._identity = new IdentityKeys(this._columns);
            this._computedColumns = new ComputedColumns(this._columns);

            this._columns.UpdatePrimary(this.PrimaryKeys);
            this._columns.UpdateForeign(this.ForeignKeys);
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


        #region Primary/Foreign Key

        private PrimaryKeys _primary = null;
        public IPrimaryKeys PrimaryKeys
        {
            get
            {
                if (this._primary == null)
                   this._primary = new PrimaryKeys(tableName);

                return this._primary;
            }
        }

        private ForeignKeys _foreign = null;
        public IForeignKeys ForeignKeys
        {
            get
            {
                if (this._foreign == null)
                    this._foreign = new ForeignKeys(tableName);

                return this._foreign;
            }
        }


        #endregion


        
        #region Identity/Computed column

        private IdentityKeys _identity = null;
        public IIdentityKeys Identity
        {
            get
            {
                if (this._columns == null)
                    LoadSchema();

                return this._identity;
            }
        }


        private ComputedColumns _computedColumns = null;
        public ComputedColumns ComputedColumns
        {
            get
            {
                if (this._columns == null)
                    LoadSchema();

                return this._computedColumns;
            }
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
                    _thisDataTable = SqlCmd.FillDataTable(tableName.Provider, "SELECT TOP 1 * FROM {0}", tableName);
                    _thisDataTable.Rows.Clear();
                }

                return _thisDataTable;
            }
        }
  
        public bool Exists
        {
            get
            {
                return MetaDatabase.TableExists(tableName);
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



    

        internal static string GenerateCREATE_TABLE(ITable metaTable)
        {
            string fields = string.Join(",\r\n", metaTable.Columns.Select(column => Sys.Data.MetaColumn.GetSQLField(column)));
            return CREATE_TABLE(fields, metaTable.PrimaryKeys);

        }



        public static string CREATE_TABLE(string fields, IPrimaryKeys primary)
        {

            string primaryKey = "";
            if (primary.Length > 0)
                primaryKey = string.Format("\tPRIMARY KEY({0})", string.Join(",", primary.Keys.Select(key => string.Format("[{0}]", key))));


            string SQL = @"
CREATE TABLE [dbo].[{0}]
(
{1}
{2}
) 
";
            return string.Format(SQL, "{0}", fields, primaryKey);
        }


     
    }


}
