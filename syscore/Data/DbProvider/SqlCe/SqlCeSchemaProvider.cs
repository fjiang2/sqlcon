﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Sys.Data
{
    class SqlCeSchemaProvider : DbSchemaProvider
    {
        public const string SQLCE_DATABASE_NAME = "SqlCeDatabase";

        public SqlCeSchemaProvider(ConnectionProvider provider)
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
                return tnames.FirstOrDefault(row => row.Name.ToUpper() == tname.Name.ToUpper() && row.SchemaName.ToUpper() == tname.SchemaName.ToUpper()) != null;

            }
            catch (Exception)
            {
            }

            return false;
        }

        public override DatabaseName[] GetDatabaseNames()
        {
            return new DatabaseName[] { new DatabaseName(provider, SQLCE_DATABASE_NAME) };
        }

        public override TableName[] GetTableNames(DatabaseName dname)
        {
            var table = dname.FillDataTable($"SELECT TABLE_SCHEMA AS SchemaName, TABLE_NAME as TableName FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE='TABLE' ORDER BY TABLE_SCHEMA,TABLE_NAME");
            if (table != null)
            {
                return table
                    .AsEnumerable()
                    .Select(row => new TableName(dname, row.Field<string>("SchemaName"), row.Field<string>("TableName")))
                    .ToArray();
            }

            return new TableName[] { };
        }

        public override TableName[] GetViewNames(DatabaseName dname)
        {
            var table = dname.FillDataTable($"SELECT TABLE_SCHEMA AS SchemaName, TABLE_NAME as TableName FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_TYPE = 'VIEW' ORDER BY TABLE_SCHEMA, TABLE_NAME");

            if (table != null)
                return table.AsEnumerable()
                .Select(row => new TableName(dname, row.Field<string>(0), row.Field<string>(1)) { Type = TableNameType.View })
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
            string SQL = $@"
SELECT 
	C.COLUMN_NAME AS ColumnName,
	DATA_TYPE AS DataType,
    CASE WHEN character_maximum_length IS NULL THEN CAST(0 AS smallint) 
		WHEN character_maximum_length > 4000 THEN CAST(-1 AS smallint) 
		ELSE CAST(character_maximum_length AS smallint) 
		END AS Length,
    CASE WHEN is_nullable = 'YES' THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS Nullable,
	CASE WHEN NUMERIC_PRECISION IS NOT NULL THEN CAST(NUMERIC_PRECISION AS tinyint) ELSE CAST(0 AS tinyint) END AS precision,
	CASE WHEN NUMERIC_SCALE IS NOT NULL THEN CAST(NUMERIC_SCALE AS tinyint) ELSE CAST(0 AS tinyint) END AS scale,
	CASE WHEN I.PRIMARY_KEY IS NOT NULL THEN CAST(I.PRIMARY_KEY AS BIT) ELSE CAST(0 AS BIT) END AS IsPrimary,
	CAST(0 AS BIT) AS IsIdentity,
	CAST(0 AS BIT) AS IsComputed,
	NULL AS definition,
	I.INDEX_NAME AS PKContraintName,
	NULL AS PK_Schema,
	NULL AS PK_Table,
	NULL AS PK_Column,
	NULL AS FKContraintName
FROM INFORMATION_SCHEMA.COLUMNS C
LEFT JOIN INFORMATION_SCHEMA.INDEXES I ON I.TABLE_NAME=C.TABLE_NAME AND I.COLUMN_NAME = C.COLUMN_NAME
WHERE C.TABLE_NAME='{tname.Name}'
";
            return new SqlCmd(tname.Provider, SQL).FillDataTable();
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
