﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Data.SqlServerCe;
using System.Data;
using System.Data.Common;

namespace Sys.Data
{
    class SqlCeConnectionProvider : ConnectionProvider
    {

        public SqlCeConnectionProvider(string name, string connectionString)
            : base(name, ConnectionProviderType.SqlServerCe, connectionString)
        {
        }

        private int version = -1;
        public override int Version
        {
            get
            {
                if (version != -1)
                    return version;

                if (this.Type == ConnectionProviderType.SqlServer)
                {
                    SqlCeConnection conn = new SqlCeConnection(ConnectionString);
                    try
                    {
                        conn.Open();
                        SqlCeCommand cmd = new SqlCeCommand("SELECT @@version", conn);
                        string text = (string)cmd.ExecuteScalar();
                        if (text.StartsWith("Microsoft SQL Azure"))
                            return version = 2016;

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
                }

                return version;
            }
        }

        public override bool CheckConnection()
        {
            return true;
        }

        private bool InvalidSqlClause(string sql)
        {
            string connString = ConnectionString;
            SqlCeConnection conn = new SqlCeConnection(connString);
            try
            {
                conn.Open();
                SqlCeCommand cmd = new SqlCeCommand(sql, conn);
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


        protected override DbSchemaProvider GetSchema()
        {
            return new SqlCeSchemaProvider(this);
        }


        internal override DbProviderType DpType
        {
            get
            {
                return DbProviderType.SqlCe;
            }
        }

        internal override DbConnection NewDbConnection
        {
            get
            {
                return new SqlCeConnection(ConnectionString);
            }
        }

        internal override string CurrentDatabaseName()
        {
            return SqlCeSchemaProvider.SQLCE_DATABASE_NAME;
        }

        internal override DbProvider CreateDbProvider(string script)
        {
            return new SqlCeProvider(script, this);
        }

    }
}
