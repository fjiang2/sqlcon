using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Sys.Data;
using Sys.Stdio;
using Tie;

namespace Sys
{
    class ConnectionConfiguration : IConnectionConfiguration
    {
        private string home;
        private VAL machines;
        public ConnectionConfiguration(string home, VAL machines)
        {
            this.home = home;
            this.machines = machines;
        }

        public string Home => this.home;

        public string DefaultServerPath
        {
            get
            {
                var provider = GetProvider(Home);
                return string.Format("{0}\\{1}", provider.ServerName, provider.DefaultDatabaseName.Name);
            }
        }

        private static string PeelOleDb(string connectionString)
        {
            if (connectionString.ToLower().IndexOf("sqloledb") >= 0)
            {
                var x1 = new OleDbConnectionStringBuilder(connectionString);
                var x2 = new SqlConnectionStringBuilder();
                x2.DataSource = x1.DataSource;
                x2.InitialCatalog = (string)x1["Initial Catalog"];
                x2.UserID = (string)x1["User Id"];
                x2.Password = (string)x1["Password"];
                return x2.ConnectionString;
            }

            return connectionString;
        }

        private List<ConnectionProvider> providers = null;
        public List<ConnectionProvider> Providers
        {
            get
            {
                if (providers == null)
                    providers = GetConnectionProviders();

                return providers;
            }
        }

        private List<ConnectionProvider> GetConnectionProviders()
        {
            List<ConnectionProvider> pvds = new List<ConnectionProvider>();

            if (machines.Undefined)
                return pvds;

            foreach (var pair in machines)
            {
                if (pair[0].IsNull || pair[1].IsNull)
                {
                    string text = pair[0].ToSimpleString();
                    cerr.WriteLine($"warning: undefined connection string at servers.{text}");
                    continue;
                }

                string serverName = pair[0].Str;
                string connectionString = PeelOleDb(pair[1].Str);
                try
                {
                    ConnectionProvider provider = ConnectionProviderManager.Register(serverName, connectionString);
                    pvds.Add(provider);
                }
                catch (Exception ex)
                {
                    cerr.WriteLine(ex.Message);
                }
            }

            return pvds;
        }

        public ConnectionProvider GetProvider(string path)
        {
            string[] x = path.Split('\\');
            if (x.Length < 3)
            {
                cerr.WriteLine($"invalid server path: {path}, correct format is server\\database");
                return null;
            }

            return GetProvider(x[1], x[2]);
        }


        private ConnectionProvider GetProvider(string serverName, string databaseName)
        {
            var provider = Providers.Find(x => x.Name == serverName);
            if (provider != null)
            {
                if (databaseName == "~")
                    databaseName = provider.DefaultDatabaseName.Name;

                return ConnectionProviderManager.CloneConnectionProvider(provider, serverName, databaseName);
            }
            else
            {
                cerr.WriteLine($"invalid server path: \\{serverName}\\{databaseName}");
                return null;
            }
        }

      
    }
}
