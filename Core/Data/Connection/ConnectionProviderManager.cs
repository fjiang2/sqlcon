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
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.OleDb;
using Tie;

namespace Sys.Data
{
    
    public class ConnectionProviderManager 
    {
        private static ConnectionProviderManager instance = null;

        private Dictionary<int, ConnectionProvider> providers = new Dictionary<int, ConnectionProvider>();

        private ConnectionProviderManager()
        { 
        }

        public static ConnectionProviderManager Instance
        {
            get
            {
                if (ConnectionProviderManager.instance == null)
                    ConnectionProviderManager.instance = new ConnectionProviderManager();
                
                return ConnectionProviderManager.instance;
            }

        }

        private void Add(ConnectionProvider provider)
        {
            if (providers.ContainsKey(provider.Handle))
            {
                providers.Remove(provider.Handle);
            }

            providers.Add(provider.Handle, provider);

        }

     
        private void Remove(ConnectionProvider provider)
        {
             providers.Remove(provider.Handle);
        }


        public override string ToString()
        {
            return string.Format("Registered Providers = #{0}", providers.Count);
        }

        

        public IEnumerable<ConnectionProvider> Providers
        {
            get { return this.providers.Values; }
        }

        public ConnectionProvider GetProvider(int handle)
        {
            if (providers.ContainsKey(handle))
                return providers[handle];

            return ConnectionProviderManager.DefaultProvider;
        }

        public ConnectionProvider GetProvider(string name)
        {
            foreach (var provider in this.providers.Values)
            {
                if (provider.Name == name)
                    return provider;
            }

            return ConnectionProviderManager.DefaultProvider;

        }




//-----------------------------------------------------------------------------------------------------------------------------------
 
        #region Default Data Provider

        public static ConnectionProvider RegisterDefaultProvider(string connectionString)
        {
            return RegisterDefaultProvider(connectionString, null);
        }

        public static ConnectionProvider RegisterDefaultProvider(string connectionString, string sysDatabase)
        {
            SqlConnectionStringBuilder builder = new SqlConnectionStringBuilder(connectionString);

            Const.CONNECTION_STRING = connectionString;
            Const.DB_APPLICATION = builder.InitialCatalog;
            if (sysDatabase == null)
                Const.DB_SYSTEM = builder.InitialCatalog;
            else
                Const.DB_SYSTEM = sysDatabase;

            defaultProvider = new SqlDbConnectionProvider(ConnectionProvider.DEFAULT_HANDLE, "Default",  connectionString);
            Instance.Add(ConnectionProviderManager.DefaultProvider);

            return ConnectionProviderManager.DefaultProvider;
        }

      
        private static ConnectionProvider defaultProvider = null;
        public static ConnectionProvider DefaultProvider
        {
            get 
            {
                if (defaultProvider == null)
                    defaultProvider = new SqlDbConnectionProvider(ConnectionProvider.DEFAULT_HANDLE, "Default", Const.CONNECTION_STRING);

                return defaultProvider; 
            }
        }

        #endregion


        /// <summary>
        /// provider's handle is assigned during runtime
        /// </summary>
        private static int PROVIDER = ConnectionProvider.USER_HANDLE_BASE;
            
        /// <summary>
        /// 
        /// </summary>
        /// <param name="name">used to display and search</param>
        /// <param name="type"></param>
        /// <param name="connectionString"></param>
        /// <returns></returns>
        internal static ConnectionProvider RegisterOleDb(string name, ConnectionProviderType type, string connectionString)
        {
            ConnectionProvider pvd = new OleDbConnectionProvider(++PROVIDER, name, type, connectionString);
            Instance.Add(pvd);
            return pvd;
        }


        public static void Unregister(ConnectionProvider provider)
        {
            Instance.Remove(provider);
        }


        public static ConnectionProvider Register(string serverName, string connectionString)
        {
            
            var conn = connectionString.ToLower();

            ConnectionProvider pvd;

            if (conn.IndexOf("provider=xmlfile") >= 0)
            {
                pvd = new XmlDbConnectionProvider(++PROVIDER, serverName, connectionString);
            }
            else if (conn.IndexOf("provider=riadb") >= 0)
            {
                pvd = new RiaDbConnectionProvider(++PROVIDER, serverName, connectionString);
            }
            else if (conn.IndexOf("provider=sqloledb") >= 0)
            {
                pvd = new OleDbConnectionProvider(++PROVIDER, serverName, connectionString);
            }
            else
                pvd = new SqlDbConnectionProvider(++PROVIDER, serverName, connectionString);

            Instance.Add(pvd);
            return pvd;
        }

        public static ConnectionProvider CloneConnectionProvider(ConnectionProvider provider, string serverName, string databaseName)
        {
            provider.InitialCatalog = databaseName;
            var pvd = ConnectionProviderManager.Register(serverName, provider.ConnectionString);
            return pvd;
        }
    }
}

