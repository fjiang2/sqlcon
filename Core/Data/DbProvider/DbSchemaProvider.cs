using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

namespace Sys.Data
{
    public abstract class DbSchemaProvider
    {
        protected ConnectionProvider provider;

        protected DbSchemaProvider(ConnectionProvider provider)
        {
            this.provider = provider;
        }

        public virtual bool Exists(DatabaseName dname)
        {
            return GetDatabaseNames().FirstOrDefault(row => row.Equals(dname)) != null;
        }

        public virtual bool Exists(TableName tname)
        {
            DatabaseName dname = tname.DatabaseName;
            if (!Exists(dname))
                return false;

            return GetTableNames(dname).FirstOrDefault(row => row.Equals(tname)) != null;
        }


        public ServerName ServerName
        {
            get { return this.provider.ServerName; }
        }

        public abstract DatabaseName[] GetDatabaseNames();

        public abstract TableName[] GetTableNames(DatabaseName dname);

        public abstract TableName[] GetViewNames(DatabaseName dname);

        public abstract DataSet GetServerSchema(ServerName sname);
        public abstract DataTable GetDatabaseSchema(DatabaseName dname);
        public abstract DataTable GetTableSchema(TableName tname);
        public abstract DataTable GetDependencySchema(DatabaseName dname);
    }
}
