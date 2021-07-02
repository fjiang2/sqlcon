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
using System.Data.Common;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using Sys.Networking;
using Tie;

namespace Sys.Data
{
    public abstract class ConnectionProvider : IComparable<ConnectionProvider>, IComparable
    {
        internal const int DEFAULT_HANDLE = 0;
        internal const int USER_HANDLE_BASE = DEFAULT_HANDLE + 1000;

        protected DbConnectionStringBuilder ConnectionBuilder;
        private DatabaseName _defaultDatabaseName = null;

        protected ConnectionProvider(string name, ConnectionProviderType type, DbConnectionStringBuilder connectionBuilder)
        {
            this.Name = name;
            this.Type = type;
            this.ConnectionBuilder = connectionBuilder;
            _defaultDatabaseName = new DatabaseName(this, InitialCatalog);
        }


        public string Name { get; private set; }

        internal int Handle { get; set; } = DEFAULT_HANDLE;
        public ConnectionProviderType Type { get; private set; }

        public string ConnectionString
        {
            get { return this.ConnectionBuilder.ConnectionString; }
        }

        public virtual string InitialCatalog
        {
            get
            {
                return (string)ConnectionBuilder["Initial Catalog"];
            }
            set
            {
                ConnectionBuilder["Initial Catalog"] = value;
            }
        }


        public string DataSource
        {
            get { return (string)ConnectionBuilder["Data Source"]; }
            set { ConnectionBuilder["Data Source"] = value; }

        }

        public string UserId
        {
            get
            {
                if (ConnectionBuilder.ContainsKey("User Id"))
                    return (string)ConnectionBuilder["User Id"];
                else
                    return null;
            }
            set { ConnectionBuilder["User Id"] = value; }
        }

        public string Password
        {
            get
            {
                if (ConnectionBuilder.ContainsKey("Password"))
                    return (string)ConnectionBuilder["Password"];
                else
                    return null;
            }
            set { ConnectionBuilder["Password"] = value; }
        }

        public override bool Equals(object obj)
        {
            ConnectionProvider pvd = (ConnectionProvider)obj;
            return this.Handle.Equals(pvd.Handle);
        }

        public override int GetHashCode()
        {
            return this.Handle.GetHashCode();
        }


        public int CompareTo(object obj)
        {
            return CompareTo((ConnectionProvider)obj);
        }

        public int CompareTo(ConnectionProvider provider)
        {
            return Handle.CompareTo(provider.Handle);
        }


        public override string ToString()
        {
            return string.Format("Handle={0}, Name={1}\\{2}, DataSource={3}", this.Handle, this.Name, this.InitialCatalog, this.DataSource);
        }

        public string ToSimpleString()
        {
            return $"Provider={Type}, DataSource={DataSource}, InitialCatalog={InitialCatalog}";
        }

        public static explicit operator int(ConnectionProvider provider)
        {
            return provider.Handle;
        }

        private static Dictionary<string, ServerName> _serverNames = new Dictionary<string, ServerName>();
        public ServerName ServerName
        {
            get
            {
                string key = this.DataSource;
                key = this.Name;
                if (!_serverNames.ContainsKey(key))
                {
                    _serverNames.Add(key, new ServerName(this, Name));
                }

                var sname = _serverNames[key];
                return sname;
            }
        }


        public DatabaseName DefaultDatabaseName
        {
            get => _defaultDatabaseName;
        }


        private DbSchemaProvider schema = null;
        public DbSchemaProvider Schema
        {
            get
            {
                if (schema == null)
                    schema = GetSchema();

                return schema;
            }
        }

        public virtual int Version => 2005;

        public bool IsReadOnly => DpType == DbProviderType.FileDb;

        public abstract bool CheckConnection();



        protected abstract DbSchemaProvider GetSchema();

        public abstract SchemaName DefaultTableSchemaName { get; }

        public abstract DbProviderType DpType { get; }

        public abstract DbConnection NewDbConnection { get; }

        public abstract string CurrentDatabaseName();

        public abstract DbProvider CreateDbProvider(string script);

        public const string PROVIDER_FILE_DB_XML = "file/db/xml";

        private const string PROVIDER_FILE_DATASET_JSON = "file/dataset/json";
        private const string PROVIDER_FILE_DATASET_XML = "file/dataset/xml";
        private const string PROVIDER_FILE_DATALAKE_JSON = "file/datalake/json";
        private const string PROVIDER_FILE_DATALAKE_XML = "file/datalake/xml";

        private const string PROVIDER_FILE_ASSEMBLY = "file/assembly";
        private const string PROVIDER_FILE_CSHARP = "file/c#";

        private const string PROVIDER_REMOTE_INVOKE_AGENT = "riadb";
        private const string PROVIDER_SQL_OLE_DB = "sqloledb";
        private const string PROVIDER_SQL_DB = "sqldb";


        private static Dictionary<string, Func<string, string, ConnectionProvider>> providers = new Dictionary<string, Func<string, string, ConnectionProvider>>();
        public static void Register(string providerName, Func<string, string, ConnectionProvider> provider)
        {
            if (providers.ContainsKey(providerName))
            {
                providers.Remove(providerName);
            }

            providers.Add(providerName, provider);
        }

        public static ConnectionProvider CreateProvider(string serverName, string connectionString)
        {
            DbConnectionStringBuilder conn = new DbConnectionStringBuilder();
            conn.ConnectionString = connectionString.ToLower();

            string providerName = PROVIDER_SQL_DB;
            object value;
            if (conn.TryGetValue("provider", out value))
            {
                if (value is string)
                    providerName = (string)value;
            }

            ConnectionProvider pvd = null;

            switch (providerName)
            {
                case "xmlfile":
                case PROVIDER_FILE_DB_XML:
                    return new FileDbConnectionProvider(serverName, connectionString, DbFileType.XmlDb);

                case PROVIDER_FILE_DATASET_JSON:
                    return new FileDbConnectionProvider(serverName, connectionString, DbFileType.JsonDataSet);

                case PROVIDER_FILE_DATALAKE_JSON:
                    return new FileDbConnectionProvider(serverName, connectionString, DbFileType.JsonDataLake);

                case PROVIDER_FILE_DATASET_XML:
                    return new FileDbConnectionProvider(serverName, connectionString, DbFileType.XmlDataSet);

                case PROVIDER_FILE_DATALAKE_XML:
                    return new FileDbConnectionProvider(serverName, connectionString, DbFileType.XmlDataLake);

                case PROVIDER_FILE_ASSEMBLY:
                    return new FileDbConnectionProvider(serverName, connectionString, DbFileType.Assembly);

                case PROVIDER_FILE_CSHARP:
                    return new FileDbConnectionProvider(serverName, connectionString, DbFileType.CSharp);

                case PROVIDER_REMOTE_INVOKE_AGENT:                   //Remote Invoke Agent
                    pvd = new RiaDbConnectionProvider(serverName, connectionString);
                    break;

                case "Microsoft.ACE.OLEDB.12.0": //Excel 2010
                case "Microsoft.Jet.OLEDB.4.0":  //Excel 2007 or Access
                case "MySqlProv":                //MySql
                case "MSDAORA":                  //Oracle
                case PROVIDER_SQL_OLE_DB:
                    pvd = new OleDbConnectionProvider(serverName, connectionString);
                    break;

                case PROVIDER_SQL_DB:                   //Sql Server
                    pvd = new SqlDbConnectionProvider(serverName, connectionString);
                    break;
            }

            if (pvd == null)
            {
                //pvd = new SqlCeConnectionProvider(serverName, connectionString.Replace("provider=sqlce;", ""));
                if (providers.ContainsKey(providerName))
                    return providers[providerName](serverName, connectionString);
            }


            return pvd;
        }
    }
}
