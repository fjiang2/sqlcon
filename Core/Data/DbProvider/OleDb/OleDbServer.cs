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
using System.Data.OleDb;

namespace Sys.Data
{
    public class OleDbServer
    {
        private const string Excel2010 = "Provider=Microsoft.ACE.OLEDB.12.0;Data Source=@XLS;Extended Properties=\"Excel 12.0 Xml;HDR=@HDR\"";
        private const string Excel2007 = "Provider=Microsoft.Jet.OLEDB.4.0;Data Source=@XLS;Extended Properties=\"Excel 8.0;HDR=@HDR;\"";
        private const string MySQL      = "Provider=MySqlProv;Data Source=@ServerName; User id=@UserName; Password=@Password";
        private const string Oracle     = "Provider=MSDAORA;Data Source= @Database;UserId=@UserName;Password=@Password;";
        private const string Access     = "Provider=Microsoft.Jet.OLEDB.4.0; Data Source=@mdb; Jet OLEDB:Database Password=@Password";


        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">used to display and search</param>
        /// <param name="type"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        private static ConnectionProvider RegisterOleDb(string name, ConnectionProviderType type, string connectionString)
        {
            ConnectionProvider pvd = new OleDbConnectionProvider(name, type, connectionString);
            ConnectionProviderManager.Register(pvd);
            return pvd;
        }

        public static ConnectionProvider RegisterExcel2007(string xlsName, bool HasHeader = false)
        {
            return RegisterOleDb(xlsName, ConnectionProviderType.Excel2007, 
                Excel2007
                    .Replace("@XLS", xlsName)
                    .Replace("@HDR", HasHeader ? "Yes" : "No")
                );
        }

        public static ConnectionProvider RegisterExcel2010(string xlsName, bool HasHeader = false)
        {
            return RegisterOleDb(xlsName, ConnectionProviderType.Excel2010, 
                Excel2010
                    .Replace("@XLS", xlsName)
                    .Replace("@HDR", HasHeader ? "Yes" : "No")
                );
        }

        public static ConnectionProvider RegisterMySQL(string serverName, string userName, string password)
        {
            return RegisterOleDb(
                serverName, 
                ConnectionProviderType.MySQL,
                MySQL
                    .Replace("@ServerName", serverName)
                    .Replace("@UserName", userName)
                    .Replace("@Password", password)
                );
        }

        public static ConnectionProvider RegisterOracle(string database, string userName, string password)
        {
            return RegisterOleDb(
                database,
                ConnectionProviderType.Oracle,
                Oracle
                    .Replace("@Database", database)
                    .Replace("@UserName", userName)
                    .Replace("@Password", password)
                );
        }


     
    }
}
