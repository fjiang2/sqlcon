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

namespace Sys.Data
{
    public class TableName : IComparable<TableName>, IComparable, IDataPath
    {
        protected const string dbo = Sys.Data.SchemaName.dbo;

        protected DatabaseName baseName;
        protected string schema = dbo;
        protected string tableName;

        public TableName(ConnectionProvider provider, string fullTableName)
        {
            //tableName may have format like [db.dbo.tableName], [db..tableName], or [tableName]
            string[] t = fullTableName.Split(new char[] { '.' });

            string databaseName = "";
            this.tableName = "";
            if (t.Length > 2)
            {
                databaseName = t[0];
                this.schema = t[1];
                this.tableName = t[2];
            }
            else if (t.Length > 1)
            {
                this.schema = t[0];
                this.tableName = t[1];
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
            return FullName.GetHashCode() * 100 + this.baseName.GetHashCode();
        }

        public string Name
        {
            get { return this.tableName; }
        }

        public string Path
        {
            get => $"{this.schema}.{this.tableName}";
        }

        public string FullPath => $"{DatabaseName.FullPath}\\{Path}";

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
                string _schema = this.schema;
                if (_schema != dbo)
                {
                    _schema = $"[{schema}]";
                }

                if (this.baseName.Name != "")
                {
                    if (baseName.Provider.DataSource.ToLower().StartsWith("(localdb)"))
                    {
                        return $"[{baseName.Name}].{_schema}.[{tableName}]";
                    }
                    else if (baseName.Provider.DpType != DbProviderType.SqlCe) //Visual Studio 2010 Windows Form Design Mode, does not support format [database]..[table]
                    {
                        return $"[{baseName.Name}].{_schema}.[{tableName}]";
                    }
                    else
                    {
                        return $"[{tableName}]";
                    }
                }
                else if (schema != string.Empty)
                    return $"{_schema}.[{tableName}]";
                else
                    return this.tableName;
            }
        }


        public override string ToString()
        {
            return FullName;
        }

        public int Id => this.GetTableSchema().TableID;

        public int ColumnId(string columnName)
        {
            if (Id == -1)
                return -1;

            return this.GetTableSchema().ColumnId(columnName);
        }


        public TableNameType Type { get; set; } = TableNameType.Table;

        public ConnectionProvider Provider
        {
            get { return this.baseName.Provider; }
        }

        public bool Exists()
        {
            return Provider.Schema.Exists(this);
        }
    }

    public enum TableNameType
    {
        Table,
        View,
        Procedure,
        Function,
    }
}
