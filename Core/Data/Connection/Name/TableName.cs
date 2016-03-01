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

namespace Sys.Data
{
    public class TableName : IComparable<TableName>, IComparable, IDataPath
    {
        public const string dbo = "dbo";

        protected DatabaseName baseName;
        protected string schema = dbo;
        protected string tableName;

        public TableName(ConnectionProvider provider, string fullTableName)
        {
            //tableName may have format like [db.dbo.tableName], [db..tableName], or [tableName]
            string[] t = fullTableName.Split(new char[] { '.' });

            string databaseName = "";
            this.tableName = "";
            if (t.Length > 1)
            {
                databaseName = t[0];
                this.schema = t[1];
                this.tableName = t[2];
            }
            else
                this.tableName = fullTableName;

            databaseName = databaseName.Replace("[", "").Replace("]", "");
            this.schema = this.schema.Replace("[", "").Replace("]", "");
            this.tableName = this.tableName.Replace("[", "").Replace("]", "");

            if (databaseName == "")
                databaseName = provider.CurrentDatabaseName();

            this.baseName = new DatabaseName(provider, databaseName);
        }

      

        public TableName(DatabaseName databaseName, string schemaName, string tableName)
        {
            this.baseName = databaseName;
            this.schema = schemaName;
            this.tableName = tableName;
        }


        public int CompareTo(object obj)
        {
            return CompareTo((TableName)obj);
        }

        public int CompareTo(TableName n)
        {
            if (this.baseName.CompareTo(n.baseName) == 0)
                return this.tableName.CompareTo(n.tableName);

            return this.baseName.CompareTo(n.baseName);
        }

        public override bool Equals(object obj)
        {
            TableName name = (TableName)obj;
            return FullName.ToLower().Equals(name.FullName.ToLower()) && this.baseName.Equals(name.baseName);
        }

        public override int GetHashCode()
        {
            return FullName.GetHashCode()*100 + this.baseName.GetHashCode();
        }
    
        public string Name
        {
            get { return this.tableName; }
        }

        public string Path
        {
            get
            {
                return string.Format("{0}.{1}", this.schema, this.tableName);
            }
        }

        public string SchemaName
        {
            get { return this.schema; }
        }
        public DatabaseName DatabaseName
        {
            get { return this.baseName; }
        }

        public string FormalName
        {
            get 
            {
                if (this.schema != dbo)
                    return string.Format("[{0}].[{1}]", this.schema, this.tableName); 
                else
                    return string.Format("[{0}]", this.tableName);
            }
        }

        public string ShortName
        {
            get 
            {
                if (this.schema != dbo)
                {
                    return string.Format("{0}.{1}", this.schema, this.tableName);
                }
                else
                    return this.tableName;
            }
        }

        public string FullName
        {
            get
            {
                if (this.baseName.Name != "")
                {
                    //Visual Studio 2010 Windows Form Design Mode, does not support format [database]..[table]
                    if (baseName.Provider.DpType != DbProviderType.SqlCe)
                        return string.Format("{0}.{1}.[{2}]", this.baseName.Name, this.schema, this.tableName);
                    else
                        return string.Format("[{0}]", this.tableName);
                }
                else
                    return this.tableName;
            }
        }

            
        public override string ToString()
        {
            return FullName;
        }


        
        public int Id
        {
            get
            {
                return this.GetTableSchema().TableID;
            }
        }



        public int ColumnId(string columnName)
        {
            if (Id == -1)
                return -1;

            return this.GetTableSchema().ColumnId(columnName);
        }


        public bool IsViewName { get; set; }

        public ConnectionProvider Provider
        {
            get { return this.baseName.Provider; }
        }

        public bool Exists()
        {
            return Provider.Schema.Exists(this);
        }


        public DataTable TableSchema()
        {
            return Provider.Schema.GetTableSchema(this);
        }


        public string GenerateScript(bool if_drop = false)
        {
            TableSchema schema = new TableSchema(this);
            var script = new TableClause(schema);

            StringBuilder builder = new StringBuilder();
            if (if_drop)
                builder.Append(script.IF_EXISTS_DROP_TABLE())
                    .AppendLine(TableClause.GO);

            return script.GenerateScript();
        }
    }
}
