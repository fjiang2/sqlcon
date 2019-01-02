using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Sys.Data;

namespace Sys.Data.Manager
{
    class DataTableDpoClass : ITable
    {

        DataTable table;
        ClassTableName tableName;

        ColumnCollection _columns;
        IdentityKeys _identity;
        ComputedColumns _computedColumns;

        public DataTableDpoClass(DataTable table)
        {

            DatabaseName dname = new DatabaseName(ConnectionProviderManager.DefaultProvider, "MEM");

            this.table = table;

            this.tableName = new ClassTableName(new TableName(dname, TableName.dbo, table.TableName));



            this._columns = new ColumnCollection(this);
            foreach (DataColumn c in table.Columns)
            {
                this._columns.Add(new DtColumn(c));
            }

            this._identity = new IdentityKeys(this._columns);
            this._computedColumns = new ComputedColumns(this._columns);

            this._columns.UpdatePrimary(this.PrimaryKeys);
            this._columns.UpdateForeign(this.ForeignKeys);
        }

        public TableName TableName 
        {
            get
            {
                return tableName;
            }
            
        }
        
        public int TableID
        {
            get
            {
                return 0;
            }
        }

        public IIdentityKeys Identity 
        {
            get
            {
                return _identity;
            }
        }

        public IPrimaryKeys PrimaryKeys
        {
            get
            {
                return new PrimaryKeys(new string[]{});
            }
        }

        public IForeignKeys ForeignKeys
        {
            get
            {
                return new ForeignKeys(new ForeignKey[] {});
            }
        }

        public ColumnCollection Columns 
        {
            get
            {
                return this._columns;
            }
        }

    }


    class DtColumn : IColumn
    {
        DataColumn column;
        public DtColumn(DataColumn column)
        { 
            this.column = column;
        }

        public string ColumnName 
        { 
            get 
            { 
                return column.ColumnName; 
            } 
        }

        public string DataType 
        { 
            get 
            { 
                return column.DataType.ToCType().ToString().ToLower(); 
            } 
        }

        public short Length 
        { 
            get 
            { 
                return (short)column.MaxLength; 
            } 
        }

        public bool Nullable 
        { 
            get 
            { 
                return column.AllowDBNull; 
            } 
        }

        public byte Precision 
        { 
            get 
            { 
                return 0; 
            } 
        }

        public byte Scale 
        { 
            get 
            { 
                return 0; 
            } 
        }

        public string Definition
        {
            get
            {
                return null; 
            }
        }
        public bool IsIdentity { get { return column.AutoIncrement; } }


        public bool IsPrimary
        {
            get
            {
                return false;
            }
        }

        public bool IsComputed 
        { 
            get 
            { 
                return false; 
            } 
        }


        public int ColumnID
        {
            get 
            { 
                return -1; 
            }
        }

        public IForeignKey ForeignKey 
        {
            get
            {
                return null;
            }
        }

        public void SetForeignKey(IForeignKey value)
        {

        }
        
        public CType CType 
        { 
            get
            {
                return column.DataType.ToCType();
            } 
        }

    }
}

