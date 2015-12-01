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
    public static class MetaDatabase
    {

        public static bool DatabaseExists(this DatabaseName databaseName)
        {
            try
            {
                switch (databaseName.Provider.DpType)
                {
                    case DbProviderType.SqlDb:
                        return SqlCmd.FillDataRow(databaseName.Provider, "SELECT * FROM sys.databases WHERE name = '{0}'", databaseName.Name) != null;
                    case DbProviderType.SqlCe:
                        return true;
                }
            }
            catch (Exception)
            {
            }
            return false;
        }

        public static void CreateDatabase(this DatabaseName databaseName)
        {
            SqlCmd.ExecuteNonQuery(databaseName.Provider, "CREATE DATABASE {0}", databaseName.Name);
        }

        public static bool TableExists(this TableName tname)
        {
            try
            {
                if (!DatabaseExists(tname.DatabaseName))
                    return false;

                switch (tname.Provider.DpType)
                {
                    case DbProviderType.SqlDb:
                        return SqlCmd.FillDataRow(tname.Provider, "USE [{0}] ; SELECT * FROM sys.Tables WHERE Name='{1}'", tname.DatabaseName.Name, tname.Name) != null;

                    case DbProviderType.SqlCe:
                        return SqlCmd.FillDataRow(tname.Provider, "SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='TABLE' AND TABLE_NAME='{0}'", tname.Name) != null;
                }
            }
            catch (Exception)
            {
            }

            return false;
        }


        public static string CurrentDatabaseName(DataProvider provider)
        {
            switch (provider.DpType)
            {
                case DbProviderType.SqlDb:
                    return (string)SqlCmd.ExecuteScalar(provider, "SELECT DB_NAME()");

                case DbProviderType.SqlCe:
                    return "Database" ;

                default:
                    throw new NotSupportedException();
            }
        }


        public static string[] GetDatabaseNames()
        {
             return GetDatabaseNames(DataProvider.DefaultProvider);
        }

        public static string[] GetDatabaseNames(DataProvider provider)
        {
            switch (provider.DpType)
            {
                case DbProviderType.SqlDb:
                    return SqlCmd.FillDataTable(provider, "SELECT Name FROM sys.databases ORDER BY Name").ToArray<string>("name");

                case DbProviderType.SqlCe:
                    return new string[] {"Database"};

                default:
                    throw new NotSupportedException();
            }
        }

        public static string[] GetTableNames(DatabaseName databaseName)
        {
            switch (databaseName.Provider.DpType)
            {
                case DbProviderType.OleDb:
                case DbProviderType.SqlDb:
                    return SqlCmd
                        .FillDataTable(databaseName.Provider, "USE [{0}] ; SELECT Name FROM sys.Tables ORDER BY Name", databaseName.Name)
                        .ToArray<string>("Name");

                case DbProviderType.SqlCe:
                    return SqlCmd
                            .FillDataTable(databaseName.Provider, "SELECT TABLE_NAME AS Name FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='TABLE'")
                            .ToArray<string>(0);
                default:
                    return new string[0];
            }

        }
    }
}
