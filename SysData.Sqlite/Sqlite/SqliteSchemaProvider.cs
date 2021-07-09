using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Sys.Data
{

    /*

    SELECT * FROM sqlite_master

    PRAGMA table_info(MessageBacklog)

    */
    /// <summary>
    /// 
    /// </summary>
    class SqliteSchemaProvider : DbSchemaProvider
    {
        public const string SQLITE_DATABASE_NAME = "SqliteDatabase";

        public SqliteSchemaProvider(ConnectionProvider provider)
            : base(provider)
        {
        }

        public override bool Exists(DatabaseName dname)
        {
            return true;
        }


        public override bool Exists(TableName tname)
        {
            try
            {
                var tnames = GetTableNames(tname.DatabaseName);
                return tnames.FirstOrDefault(row => row.Name.ToUpper() == tname.Name.ToUpper() && row.SchemaName?.ToUpper() == tname.SchemaName?.ToUpper()) != null;

            }
            catch (Exception)
            {
            }

            return false;
        }

        public override DatabaseName[] GetDatabaseNames()
        {
            return new DatabaseName[] { new DatabaseName(provider, SQLITE_DATABASE_NAME) };
        }

        public override TableName[] GetTableNames(DatabaseName dname)
        {
            var table = new SqlCmd(dname.Provider, $"SELECT NULL AS SchemaName, NAME as TableName FROM sqlite_master WHERE TYPE='table' AND NOT (name LIKE 'sqlite_%') ORDER BY NAME")
                .FillDataTable();

            if (table != null)
            {
                return table
                    .AsEnumerable()
                    .Select(row => new TableName(dname, row["SchemaName"].IsNull(string.Empty), row.Field<string>("TableName")))
                    .ToArray();
            }

            return new TableName[] { };
        }

        public override TableName[] GetViewNames(DatabaseName dname)
        {
            var table = new SqlCmd(dname.Provider, $"SELECT NULL AS SchemaName, NAME as TableName FROM sqlite_master WHERE TYPE='view' AND NOT (name LIKE 'sqlite_%') ORDER BY NAME")
                .FillDataTable();

            if (table != null)
                return table.AsEnumerable()
                .Select(row => new TableName(dname, row["SchemaName"].IsNull(string.Empty), row.Field<string>("TableName")) { Type = TableNameType.View })
                .ToArray();

            return new TableName[] { };
        }

        public override TableName[] GetProcedureNames(DatabaseName dname)
        {
            return new TableName[] { };
        }

        public override string GetProcedure(TableName pname)
        {
            return string.Empty;
        }

        public override DataTable GetTableSchema(TableName tname)
        {
            string SQL = string.Format(SQL_SCHEMA, string.Empty, $"WHERE C.TABLE_NAME = '{tname.Name}'");
            return new SqlCmd(tname.Provider, SQL).FillDataTable();
        }

        public override DataTable GetDatabaseSchema(DatabaseName dname)
        {
            return InformationSchema.LoadDatabaseSchema(dname.ServerName, new DatabaseName[] { dname }, CreateSQLOfDatabaseSchema)
                .Tables[dname.Name];
        }

        public override DataSet GetServerSchema(ServerName sname)
        {
            return InformationSchema.LoadDatabaseSchema(sname, sname.GetDatabaseNames(), CreateSQLOfDatabaseSchema);
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
                    FkTable = new TableName(dname, row["FK_SCHEMA"].IsNull(string.Empty), (string)row["FK_Table"]),
                    PkTable = new TableName(dname, row["PK_SCHEMA"].IsNull(string.Empty), (string)row["PK_Table"]),
                    PkColumn = (string)row["PK_Column"],
                    FkColumn = (string)row["FK_Column"]
                })
                .ToArray();

            return rows;
        }


        private static string CreateSQLOfDatabaseSchema(DatabaseName dname)
        {
            StringBuilder builder = new StringBuilder();
            string header = @"C.TABLE_SCHEMA AS SchemaName, C.TABLE_NAME AS TableName,";
            //builder.AppendFormat("USE [{0}] ", dname.Name).AppendLine();
            builder.AppendLine(string.Format(SQL_SCHEMA, header, string.Empty));

            return builder.ToString();
        }

       private static string SQL_SCHEMA = @"
SELECT 
    {0}
	C.COLUMN_NAME AS ColumnName,
	DATA_TYPE AS DataType,
    CASE WHEN CHARACTER_OCTET_LENGTH IS NULL THEN CAST(0 AS smallint) 
		WHEN CHARACTER_OCTET_LENGTH > 4000 THEN CAST(-1 AS smallint) 
		ELSE CAST(CHARACTER_OCTET_LENGTH AS smallint) 
		END AS Length,
    CASE WHEN is_nullable = 'YES' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS Nullable,
	CASE WHEN NUMERIC_PRECISION IS NOT NULL THEN CAST(NUMERIC_PRECISION AS tinyint) ELSE CAST(0 AS tinyint) END AS precision,
	CASE WHEN NUMERIC_SCALE IS NOT NULL THEN CAST(NUMERIC_SCALE AS tinyint) ELSE CAST(0 AS tinyint) END AS scale,
	CASE WHEN I.PRIMARY_KEY IS NOT NULL THEN CAST(I.PRIMARY_KEY AS BIT) ELSE CAST(0 AS BIT) END AS IsPrimary,
	CAST(0 AS BIT) AS IsIdentity,
	CAST(0 AS BIT) AS IsComputed,
	NULL AS definition,
	I.INDEX_NAME AS PKContraintName,
	X.PK_Schema,
	X.PK_Table,
	X.PK_Column,
	X.FKContraintName
FROM INFORMATION_SCHEMA.COLUMNS C
	LEFT JOIN INFORMATION_SCHEMA.INDEXES I ON I.TABLE_NAME=C.TABLE_NAME AND I.COLUMN_NAME = C.COLUMN_NAME
	LEFT JOIN (
		SELECT 
			U.TABLE_NAME,
			U.COLUMN_NAME AS COLUMN_NAME,
			R.UNIQUE_CONSTRAINT_NAME AS PKContraintName,
			I.TABLE_SCHEMA AS PK_Schema,
			I.TABLE_NAME  AS PK_Table,
			I.COLUMN_NAME AS PK_Column,
			R.CONSTRAINT_NAME AS FKContraintName 
		FROM INFORMATION_SCHEMA.KEY_COLUMN_USAGE U 
			INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS T ON T.CONSTRAINT_NAME = U.CONSTRAINT_NAME
		   	INNER JOIN INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS R ON R.CONSTRAINT_TABLE_NAME=T.TABLE_NAME AND R.CONSTRAINT_NAME=T.CONSTRAINT_NAME
			INNER JOIN INFORMATION_SCHEMA.INDEXES I ON I.INDEX_NAME = R.UNIQUE_CONSTRAINT_NAME
		WHERE T.CONSTRAINT_TYPE='FOREIGN KEY'
	) X ON X.COLUMN_NAME = C.COLUMN_NAME AND X.TABLE_NAME=C.TABLE_NAME
{1}
";
    }
}
