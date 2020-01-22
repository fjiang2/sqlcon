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
using System.Data;

namespace Sys.Data
{
    public class DatabaseName : IComparable<DatabaseName>, IComparable, IDataPath
    {
        private ConnectionProvider provider;
        private string name;
        public string NameSpace { get; set; }

        public DatabaseName(ConnectionProvider provider, string databaseName)
        {
            this.provider = provider;
            this.name = databaseName;
        }

        public string Name
        {
            get { return this.name; }
        }

        public string Path
        {
            get { return this.name; }
        }

        public string FullPath => $"{ServerName.FullPath}\\{Path}";

        public ServerName ServerName
        {
            get { return Provider.ServerName; }
        }

        public ConnectionProvider Provider
        {
            get
            {
                this.provider.InitialCatalog = name;
                return this.provider; 
            }
        }

        public int CompareTo(object obj)
        {
            return CompareTo((DatabaseName)obj);
        }

        public int CompareTo(DatabaseName n)
        {
            if (this.ServerName.CompareTo(n.ServerName) == 0)
               return this.name.CompareTo(n.name);

            return this.ServerName.CompareTo(n.ServerName);
        }


        public override int GetHashCode()
        {
            return name.GetHashCode() + this.ServerName.GetHashCode() * 324819;
        }

        public override bool Equals(object obj)
        {
            DatabaseName dname = (DatabaseName)obj;
            return this.name.ToLower().Equals(dname.name.ToLower()) && this.ServerName.Equals(dname.ServerName);
        }

        public bool Exists()
        {
            return Provider.Schema.Exists(this);
        }

        public DataTable DatabaseSchema()
        {
            return Provider.Schema.GetDatabaseSchema(this);
        }


        public TableName[] GetTableNames()
        {
            return Provider.Schema.GetTableNames(this);
        }


        public TableName[] GetViewNames()
        {
            return Provider.Schema.GetViewNames(this);
        }


        public TableName[] GetDependencyTableNames()
        {
            var dependencies = Provider
                .Schema.GetDependencySchema(this);

            var dict = dependencies.GroupBy(
                    row => row.FkTable,
                    (Key, rows) => new
                    {
                        FkTable = Key,
                        PkTables = rows.Select(row => row.PkTable).ToArray()
                    })
                .ToDictionary(row => row.FkTable, row => row.PkTables);


            TableName[] names = this.GetTableNames();

            List<TableName> history = new List<TableName>();

            foreach (var tname in names)
            {
                if (history.IndexOf(tname) < 0)
                    Iterate(tname, dict, history);
            }

            return history.ToArray();
        }

        private static void Iterate(TableName tableName, Dictionary<TableName, TableName[]> dict, List<TableName> history)
        {
            if (!dict.ContainsKey(tableName))
            {
                if (history.IndexOf(tableName) < 0)
                {
                    history.Add(tableName);
                }
            }
            else
            {
                foreach (var name in dict[tableName])
                    Iterate(name, dict, history);

                if (history.IndexOf(tableName) < 0)
                {
                    history.Add(tableName);
                }
            }
        }

        public string GenerateClause()
        {
            return new DatabaseClause(this).GenerateClause();
        }
        public override string ToString()
        {
            return string.Format("{0}\\{1}", this.ServerName, this.name);
        }
    }
}
