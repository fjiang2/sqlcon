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
using Sys.Data.Comparison;

namespace Sys.Data
{
    public static class Schema
    {

        public static bool Exists(this DatabaseName databaseName)
        {
            return databaseName.Provider.Schema.Exists(databaseName);
        }

        public static void CreateDatabase(this DatabaseName databaseName)
        {
            new SqlCmd(databaseName.Provider, "CREATE DATABASE {0}", databaseName.Name).ExecuteNonQuery();
        }

        public static bool Exists(this TableName tname)
        {
            return tname.Provider.Schema.Exists(tname);
        }

        public static DataTable TableSchema(this TableName tableName)
        {
            return tableName.Provider.Schema.GetTableSchema(tableName);
        }

        public static DataTable DatabaseSchema(this DatabaseName databaseName)
        {
            return databaseName.Provider.Schema.GetDatabaseSchema(databaseName);
        }

        public static DataSet ServerSchema(this ServerName serverName)
        {
            return serverName.Provider.Schema.GetServerSchema(serverName);
        }

        public static string CurrentDatabaseName(this ConnectionProvider provider)
        {
            switch (provider.DpType)
            {
                case DbProviderType.SqlDb:
                    {
                        return (string)DataExtension.ExecuteScalar(provider, "SELECT DB_NAME()");
                        //var connection = new SqlCmd(provider, string.Empty).DbProvider.DbConnection;
                        //return connection.Database.ToString();
                    }

                case DbProviderType.SqlCe:
                    return "Database";

                default:
                    throw new NotSupportedException();
            }
        }


        public static DatabaseName[] GetDatabaseNames(this ServerName serverName)
        {
            return serverName.Provider.Schema.GetDatabaseNames();
        }

        public static TableName[] GetTableNames(this DatabaseName databaseName)
        {
            return databaseName.Provider.Schema.GetTableNames(databaseName);
        }

        public static TableName[] GetViewNames(this DatabaseName databaseName)
        {
            return databaseName.Provider.Schema.GetViewNames(databaseName);
        }

        public static string GenerateScript(this DatabaseName databaseName)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(databaseName.GenerateDropTableScript());
            builder.Append(databaseName.GenerateScript_());
            return builder.ToString();
        }


        public static string IF_EXISTS_DROP_TABLE(this TableName tableName)
        {
            string drop =
@"IF OBJECT_ID('{0}') IS NOT NULL
  DROP TABLE {1}
";
            StringBuilder builder = new StringBuilder();
            builder.AppendFormat(drop, tableName.Name, tableName.FormalName);
            return builder.ToString();
        }

        public static string GenerateScript(this TableName tableName)
        {
            TableSchema schema1 = new TableSchema(tableName);

            string sql;
            var script = new TableScript(schema1);
            sql = script.CREATE_TABLE();

            StringBuilder builder = new StringBuilder(sql);
            //builder.AppendLine(TableScript.GO);

            var fk1 = schema1.ForeignKeys;
            if (fk1.Keys.Length > 0)
            {
                foreach (var fk in fk1.Keys)
                {
                    builder.AppendLine(script.ADD_FOREIGN_KEY(fk));
                    //builder.AppendLine(TableScript.GO);
                }

            }

            sql = builder.ToString();
            return sql;
        }

        private static string GenerateScript_(this DatabaseName databaseName)
        {
            StringBuilder builder = new StringBuilder();
            TableName[] history = GetDependencyTableNames(databaseName);

            foreach (var tableName in history)
            {
                Console.WriteLine("generate CREATE TABLE {0}", tableName.FormalName);
                try
                {
                    builder.AppendLine(tableName.GenerateScript());
                }
                catch (Exception ex)
                {
                    Console.WriteLine("failed to generate CREATE TABLE {0},{1}", tableName.FormalName, ex.Message);
                }
            }

            return builder.ToString();
        }

        public static string GenerateDropTableScript(this DatabaseName databaseName)
        {
            TableName[] history = GetDependencyTableNames(databaseName);
            StringBuilder builder = new StringBuilder();
            foreach (var tableName in history.Reverse())
            {
                builder.AppendLine(tableName.IF_EXISTS_DROP_TABLE())
                    .AppendLine(TableScript.GO);
            }

            return builder.ToString();
        }

        public static TableName[] GetDependencyTableNames(this DatabaseName databaseName)
        {
            var dt = databaseName.Provider
                .Schema.GetDependencySchema(databaseName)                
                .AsEnumerable();

            var dict = dt.GroupBy(
                    row => new TableName(databaseName, (string)row[0], (string)row[1]),
                    (Key, rows) => new { Fk = Key, Pk = rows.Select(row => new TableName(databaseName, (string)row[2], (string)row[3])).ToArray() })
                .ToDictionary(row => row.Fk, row => row.Pk);


            TableName[] names = databaseName.GetTableNames();

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
    }
}
