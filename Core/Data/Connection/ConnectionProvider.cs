﻿//--------------------------------------------------------------------------------------------------//
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
    public abstract class ConnectionProvider : IValizable, IComparable<ConnectionProvider>, IComparable
    {
        internal const int DEFAULT_HANDLE = 0;
        internal const int USER_HANDLE_BASE = DEFAULT_HANDLE + 1000;

        private DbConnectionStringBuilder ConnectionBuilder;


        protected ConnectionProvider(string name, ConnectionProviderType type, string connectionString)
        {
            this.Name = name;
            this.Type = type;

            SetDbConnectionString(connectionString);
        }


        public string Name { get; private set; }

        internal int Handle { get; set; } = DEFAULT_HANDLE;
        internal ConnectionProviderType Type { get; private set; }

        internal string ConnectionString
        {
            get { return this.ConnectionBuilder.ConnectionString; }
        }


        private void SetDbConnectionString(string connectionString)
        {
            if (Type == ConnectionProviderType.SqlServer)
                this.ConnectionBuilder = new SqlConnectionStringBuilder(connectionString);
            else if (Type == ConnectionProviderType.OleDbServer)
                this.ConnectionBuilder = new OleDbConnectionStringBuilder(connectionString);
            else
            {
                this.ConnectionBuilder = new DbConnectionStringBuilder();
                this.ConnectionBuilder.ConnectionString = connectionString;
            }
        }


        public string InitialCatalog
        {
            get { return (string)ConnectionBuilder["Initial Catalog"]; }
            set { ConnectionBuilder["Initial Catalog"] = value; }
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
            return string.Format("{0}\\{1}", this.Name, this.InitialCatalog);
        }

        public static explicit operator int(ConnectionProvider provider)
        {
            return provider.Handle;
        }

        public ConnectionProvider(VAL val)
        {
            SetVAL(val);
        }

        public void SetVAL(VAL val)
        {
            this.Handle = val["handle"].Intcon;
            this.Name = val["name"].Str;
            this.Type = (ConnectionProviderType)val["type"].Intcon;
            SetDbConnectionString(val["connection"].Str);
        }


        public VAL GetVAL()
        {
            VAL val = new VAL();
            val["handle"] = new VAL(this.Handle);
            val["name"] = new VAL(this.Name);
            val["type"] = new VAL((int)this.Type);
            val["connection"] = new VAL(this.ConnectionString);

            return val;
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


        private DatabaseName _defaultDatabaseName = null;
        public DatabaseName DefaultDatabaseName
        {
            get
            {
                if (_defaultDatabaseName == null)
                    _defaultDatabaseName = new DatabaseName(this, InitialCatalog);

                return _defaultDatabaseName;
            }
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

        internal abstract DbProviderType DpType { get; }

        internal abstract DbConnection NewDbConnection { get; }

        internal abstract string CurrentDatabaseName();

        internal abstract DbProvider CreateDbProvider(string script);


        public static ConnectionProvider CreateProvider(string serverName, string connectionString)
        {
            DbConnectionStringBuilder conn = new DbConnectionStringBuilder();
            conn.ConnectionString = connectionString.ToLower();

            string providerName = "sqldb";
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
                case "file/db/xml":
                    return new FileDbConnectionProvider(serverName, connectionString, DbFileType.XmlDb);

                case "file/dataset/json":
                    return new FileDbConnectionProvider(serverName, connectionString, DbFileType.JsonDataSet);

                case "file/datalake/json":
                    return new FileDbConnectionProvider(serverName, connectionString, DbFileType.JsonDataLake);

                case "file/dataset/xml":
                    return new FileDbConnectionProvider(serverName, connectionString, DbFileType.XmlDataSet);

                case "file/datalake/xml":
                    return new FileDbConnectionProvider(serverName, connectionString, DbFileType.XmlDataLake);

                case "file/assembly":
                    return new FileDbConnectionProvider(serverName, connectionString, DbFileType.Assembly);

                case "file/c#":
                    return new FileDbConnectionProvider(serverName, connectionString, DbFileType.CSharp);

                case "riadb":                   //Remote Invoke Agent
                    pvd = new RiaDbConnectionProvider(serverName, connectionString);
                    break;

                case "Microsoft.ACE.OLEDB.12.0": //Excel 2010
                case "Microsoft.Jet.OLEDB.4.0":  //Excel 2007 or Access
                case "MySqlProv":                //MySql
                case "MSDAORA":                  //Oracle
                case "sqloledb":
                    pvd = new OleDbConnectionProvider(serverName, connectionString);
                    break;

                case "sqldb":                   //Sql Server
                    pvd = new SqlDbConnectionProvider(serverName, connectionString);
                    break;
            }

            return pvd;
        }
    }
}
