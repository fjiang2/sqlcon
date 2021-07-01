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
using System.Runtime.InteropServices;
using System.Data;
using System.Data.SqlClient;

namespace Sys.Data
{
    public class SqlCeServer
    {
        private const int SQL_NEED_DATA = 99;
        private const int SQL_SUCCESS = 0;


        public static string[] GetDatabases(string serverName, bool integratedSecurity, string userName, string password)
        {
            string connectionString = "initial catalog=master; Data Source=" + serverName + ";" + (integratedSecurity ? "integrated security=SSPI;" : "user id=" + userName + "; password=" + password + ";") + "pooling=false";

            SqlDataAdapter adapter = new SqlDataAdapter("SELECT name FROM dbo.sysdatabases ORDER BY name", connectionString);
            DataTable dataTable = new DataTable();

            adapter.Fill(dataTable);

            int j = dataTable.Rows.Count;

            string[] databases = new string[j];

            for (int i = 0; i < j; i++)
            {
                databases[i] = (string)dataTable.Rows[i]["name"];
            }

            dataTable.Dispose();
            adapter.Dispose();

            return databases;

        }

   

        public static bool IsGoodConnectionString()
        {
            SqlConnection conn = (SqlConnection)ConnectionProviderManager.DefaultProvider.NewDbConnection;
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand("EXEC sp_databases", conn);
                cmd.ExecuteScalar();
            }
            catch (Exception)
            {
                return false;
            }
            finally
            {
                conn.Close();
            }

            return true;
        }


    }
}
