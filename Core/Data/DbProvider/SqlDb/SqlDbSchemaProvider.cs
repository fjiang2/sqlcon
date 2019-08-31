using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Sys.Data
{
    class SqlDbSchemaProvider : DbSchemaProvider
    {
        public SqlDbSchemaProvider(ConnectionProvider provider)
            : base(provider)
        {
        }

        public override bool Exists(DatabaseName dname)
        {
            try
            {
                string SQL = sp_databases(provider);
                var dnames = provider.FillDataTable(SQL).ToArray<string>("DATABASE_NAME");
                return dnames.FirstOrDefault(row => row.ToLower().Equals(dname.Name.ToLower())) != null;
            }
            catch (Exception)
            {
            }
            return false;
        }


        public override bool Exists(TableName tname)
        {
            try
            {
                if (!Exists(tname.DatabaseName))
                    return false;

                var tnames = GetTableNames(tname.DatabaseName);
                return tnames.FirstOrDefault(row => row.Name.ToUpper() == tname.Name.ToUpper() && row.SchemaName.ToUpper() == tname.SchemaName.ToUpper()) != null;

            }
            catch (Exception)
            {
            }

            return false;
        }

        private static string sp_databases(ConnectionProvider provider)
        {
            string SQL;
            if (provider.Version >= 2005)
                //Used for SQL Server 2008+, state==6 means offline database
                SQL = "SELECT Name as DATABASE_NAME FROM sys.databases WHERE State<>6 ORDER BY Name";
            else
                SQL = "EXEC sp_databases";

            return SQL;
        }

        public override DatabaseName[] GetDatabaseNames()
        {
            string SQL = sp_databases(provider);

            string[] dnames;
            switch (provider.DpType)
            {
                case DbProviderType.SqlDb:
                case DbProviderType.RiaDb:
                    dnames = provider.FillDataTable(SQL).ToArray<string>("DATABASE_NAME");
                    List<string> L = new List<string>();
                    foreach (var dname in dnames)
                    {
                        if (!__sys_tables.Contains(dname))  // && !dname.StartsWith("AzureStorageEmulator"))
                            L.Add(dname);
                    }

                    dnames = L.ToArray();
                    break;

                case DbProviderType.SqlCe:
                    dnames = new string[] { "Database" };
                    break;

                default:
                    throw new NotSupportedException();
            }

            return dnames.Select(dname => new DatabaseName(provider, dname)).ToArray();
        }

        public override TableName[] GetTableNames(DatabaseName dname)
        {
            if (dname.Provider.Version >= 2005)
            {
                var table = dname.FillDataTable($"USE [{dname.Name}] ; SELECT SCHEMA_NAME(schema_id) AS SchemaName, name as TableName FROM sys.Tables ORDER BY SchemaName,Name");
                if (table != null)
                {
                    return table
                        .AsEnumerable()
                        .Select(row => new TableName(dname, row.Field<string>("SchemaName"), row.Field<string>("TableName")))
                        .ToArray();
                }
            }
            else
            {

                var table = dname.FillDataTable($"USE [{dname.Name}] ; EXEC sp_tables");
                if (table != null)
                {
                    return table
                        .AsEnumerable()
                        .Where(row => row.Field<string>("TABLE_TYPE") == "TABLE")
                        .Select(row => new TableName(dname, row.Field<string>("TABLE_OWNER"), row.Field<string>("TABLE_NAME")))
                        .ToArray();
                }
            }

            return new TableName[] { };
        }

        public override TableName[] GetViewNames(DatabaseName dname)
        {
            if (dname.Provider.Version >= 2005)
            {
                var table = dname.FillDataTable($"USE [{dname.Name}] ; SELECT  SCHEMA_NAME(schema_id) SchemaName, name FROM sys.views ORDER BY name");

                if (table != null)
                    return table.AsEnumerable()
                    .Select(row => new TableName(dname, row.Field<string>(0), row.Field<string>(1)) { IsViewName = true })
                    .ToArray();
            }
            else
            {
                var table = dname.FillDataTable($"USE [{dname.Name}] ; EXEC sp_tables");
                if (table != null)
                {
                    return table
                        .AsEnumerable()
                        .Where(row => row.Field<string>("TABLE_TYPE") == "VIEW")
                        .Select(row => new TableName(dname, row.Field<string>("TABLE_OWNER"), row.Field<string>("TABLE_NAME")) { IsViewName = true })
                        .ToArray();
                }
            }

            return new TableName[] { };
        }


        public override DataTable GetTableSchema(TableName tname)
        {
            return InformationSchema.SqlTableSchema(tname);
        }

        public override DataTable GetDatabaseSchema(DatabaseName dname)
        {
            //return GetServerSchema(dname.ServerName).Tables[dname.Name];

            return InformationSchema.SqlServerSchema(dname.ServerName, new DatabaseName[] { dname }).Tables[dname.Name];
        }

        public override DataSet GetServerSchema(ServerName sname)
        {
            return InformationSchema.SqlServerSchema(sname, sname.GetDatabaseNames());
        }

        public override DependencyInfo[] GetDependencySchema(DatabaseName dname)
        {
            const string sql = @"
SELECT  
		FK.TABLE_SCHEMA AS FK_SCHEMA,
		FK.TABLE_NAME AS FK_Table,
		PK.TABLE_SCHEMA AS PK_SCHEMA,
        PK.TABLE_NAME AS PK_Table,
        PT.COLUMN_NAME AS PK_Column,
        CU.COLUMN_NAME AS FK_Column
  FROM  INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C
        INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
        INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
        INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME
        INNER JOIN ( SELECT i1.TABLE_NAME ,
                            i2.COLUMN_NAME
                     FROM   INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1
                            INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME
                     WHERE  i1.CONSTRAINT_TYPE = 'PRIMARY KEY'
                   ) PT ON PT.TABLE_NAME = PK.TABLE_NAME
 WHERE FK.TABLE_NAME <> PK.TABLE_NAME
";

            var dt = new SqlCmd(dname.Provider, sql).FillDataTable();

            DependencyInfo[] rows = dt.AsEnumerable().Select(
                row => new DependencyInfo
                {
                    FkTable = new TableName(dname, (string)row["FK_SCHEMA"], (string)row["FK_Table"]),
                    PkTable = new TableName(dname, (string)row["PK_SCHEMA"], (string)row["PK_Table"]),
                    PkColumn = (string)row["PK_Column"],
                    FkColumn = (string)row["FK_Column"]
                })
                .ToArray();

            return rows;
        }
    }
}
