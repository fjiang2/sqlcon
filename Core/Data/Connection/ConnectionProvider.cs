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
using System.Data.Common;
using System.Data.SqlClient;
using System.Data.OleDb;
using System.IO;
using Tie;

namespace Sys.Data
{
    public class ConnectionProvider : IValizable, IComparable<ConnectionProvider>, IComparable
    {
        internal const int DEFAULT_HANDLE = 0;
        internal const int USER_HANDLE_BASE = DEFAULT_HANDLE + 1000;
        
        private DbConnectionStringBuilder ConnectionBuilder;


        internal ConnectionProvider(int handle, string name, ConnectionProviderType type, string connectionString)
        {
            this.Handle = handle;
            this.Name = name;
            this.Type = type;

            SetDbConnectionString(connectionString);
        }

    
        public string Name { get; private set; }

        internal int Handle { get; private set;}
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


        public bool CheckConnection()
        {
            switch (Type)
            {
                case ConnectionProviderType.XmlFile:
                    if (DataSource.StartsWith("file://"))
                    {
                        string file = DataSource.Substring(7);
                        return File.Exists(file);
                    }
                    else if (DataSource.StartsWith("http://"))
                    {
                        return HttpRequest.Exists(new Uri(DataSource));
                    }
                    else
                        return false;

                default:
                    return !InvalidSqlClause("EXEC sp_databases");
            }
        }

        private bool InvalidSqlClause(string sql)
        {
            SqlConnection conn = new SqlConnection(ConnectionString);
            try
            {
                conn.Open();
                SqlCommand cmd = new SqlCommand(sql, conn);
                cmd.ExecuteScalar();
            }
            catch (Exception)
            {
                return true;
            }
            finally
            {
                conn.Close();
            }

            return false;
        }

        private int version = -1;
        public int Version
        {
            get
            {
                if (version != -1)
                    return version;

                SqlConnection conn = new SqlConnection(ConnectionString);
                try
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand("SELECT @@version", conn);
                    string text = (string)cmd.ExecuteScalar();
                    string[] items = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    version = int.Parse(items[3]);
                }
                catch (Exception)
                {
                    version = 0;
                }
                finally
                {
                    conn.Close();
                }

                return version;
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
            return this.Handle.Equals(pvd.Handle) ;
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
       
        internal DbProviderType DpType
        {
            get
            {
                switch (Type)
                {
                    case ConnectionProviderType.SqlServer:
                        return DbProviderType.SqlDb;

                    case ConnectionProviderType.SqlServerCe:
                        return DbProviderType.SqlCe;

                    case ConnectionProviderType.XmlFile:
                        return DbProviderType.XmlDb;

                    default:
                        return DbProviderType.OleDb;
                }
            }
        }

        public DbConnection NewDbConnection
        {
            get
            {
                switch (DpType)
                {
                    case DbProviderType.SqlDb:
                        return new SqlConnection(ConnectionString);

                    case DbProviderType.OleDb:
                        return new OleDbConnection(ConnectionString);

                    case DbProviderType.XmlDb:
                        return new XmlDbConnection(this);

                }

                throw new NotImplementedException();
            }
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


        private SchemaProvider schema = null;
        public SchemaProvider Schema
        {
            get
            {
                if (schema == null)
                {
                    switch (Type)
                    {
                        case ConnectionProviderType.SqlServer:
                            schema = new SqlSchemaProvider(this);
                            break;

                        case ConnectionProviderType.XmlFile:
                            schema = new XmlDbSchemaProvider(this);
                            break;
                    }
                }

                return schema;
            }
        }
    }
}
