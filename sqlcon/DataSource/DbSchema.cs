using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using Sys.Data;

namespace sqlcon
{
    static class DbSchema
    {

        private static DataTable Use(this TableName tableName, string script)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("USE [{0}] ", tableName.DatabaseName.Name).AppendLine();

            builder.AppendFormat(script, tableName.Name);

            return DataExtension.FillDataTable(tableName.Provider, builder.ToString());

        }

        private static DataTable Use(this DatabaseName databaseName, string script)
        {
            StringBuilder builder = new StringBuilder();

            builder.AppendFormat("USE [{0}] ", databaseName.Name).AppendLine();

            builder.Append(script);

            return DataExtension.FillDataTable(databaseName.Provider, builder.ToString());

        }

        private const string SQL_FK_QUERY = @"
        SELECT  
                FK.TABLE_SCHEMA AS FK_Schema,
                FK.TABLE_NAME AS FK_Table,
                CU.COLUMN_NAME AS FK_Column,
                PK.TABLE_SCHEMA AS PK_Schema,
                PK.TABLE_NAME AS PK_Table,
                PT.COLUMN_NAME AS PK_Column,
                C.CONSTRAINT_NAME AS Constraint_Name 
        FROM    INFORMATION_SCHEMA.REFERENTIAL_CONSTRAINTS C
                INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS FK ON C.CONSTRAINT_NAME = FK.CONSTRAINT_NAME
                INNER JOIN INFORMATION_SCHEMA.TABLE_CONSTRAINTS PK ON C.UNIQUE_CONSTRAINT_NAME = PK.CONSTRAINT_NAME
                INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE CU ON C.CONSTRAINT_NAME = CU.CONSTRAINT_NAME
                INNER JOIN ( SELECT i1.TABLE_NAME ,
                                    i2.COLUMN_NAME
                                FROM   INFORMATION_SCHEMA.TABLE_CONSTRAINTS i1
                                    INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE i2 ON i1.CONSTRAINT_NAME = i2.CONSTRAINT_NAME
                                WHERE  i1.CONSTRAINT_TYPE = 'PRIMARY KEY'
                            ) PT ON PT.TABLE_NAME = PK.TABLE_NAME
        ";

        public static DataTable ForeignKeySchema(this TableName tname)
        {
            string WHERE = string.Format("WHERE FK.TABLE_SCHEMA='{0}' AND FK.TABLE_NAME='{1}'", tname.SchemaName, tname.Name);
            return Use(tname, SQL_FK_QUERY + WHERE);
        }

        public static DataTable DependenySchema(this TableName tname)
        {
            string WHERE = string.Format("WHERE PK.TABLE_SCHEMA='{0}' AND PK.TABLE_NAME='{1}'", tname.SchemaName, tname.Name);
            return Use(tname, SQL_FK_QUERY + WHERE);
        }

        public static DataTable PrimaryKeySchema(this TableName tableName)
        {
            string SQL = @"
            SELECT c.COLUMN_NAME, pk.CONSTRAINT_NAME
                FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk, 
                     INFORMATION_SCHEMA.KEY_COLUMN_USAGE c 
                WHERE pk.TABLE_NAME = '{0}' 
                      AND CONSTRAINT_TYPE = 'PRIMARY KEY' 
                      AND c.TABLE_NAME = pk.TABLE_NAME 
                      AND c.CONSTRAINT_NAME = pk.CONSTRAINT_NAME
            ";


            return Use(tableName, SQL);

        }

        public static DataTable IdentityKeySchema(this TableName tableName)
        {
            string SQL = @"
            SELECT 
	            t.name AS TableName,
	            c.name AS ColumnName
            FROM sys.tables t 
	            JOIN sys.columns c ON t.object_id = c.object_id 
            WHERE t.name = '{0}' AND 
	                c.is_identity = 1
            ORDER BY t.name
            ";
            return Use(tableName, SQL);

        }

        public static DataTable StorageSchema(this TableName tableName)
        {
            string SQL = string.Format("Exec sp_spaceused N'{0}'", tableName.ShortName);
            return Use(tableName, SQL);
        }

        public static DataTable StorageSchema(this DatabaseName dname)
        {
            string SQL = "Exec sp_spaceused";
            return Use(dname, SQL);
        }

        public static DataTable AllView(this DatabaseName databaseName)
        {
            string SQL = @"
            SELECT SCHEMA_NAME(schema_id) AS schema_name
		            ,name AS view_name
		            ,OBJECTPROPERTYEX(OBJECT_ID,'IsIndexed') AS IsIndexed
		            ,OBJECTPROPERTYEX(OBJECT_ID,'IsIndexable') AS IsIndexable
            FROM sys.views
            ";
            return Use(databaseName, SQL);

        }

        public static DataTable ViewSchema(this TableName tableName)
        {
            string SQL = @"
                SELECT TABLE_SCHEMA, COLUMN_NAME, DATA_TYPE, IS_NULLABLE
                FROM INFORMATION_SCHEMA.COLUMNS
                WHERE TABLE_NAME='{0}'
            ";
            return Use(tableName, SQL);

        }

        public static DataTable AllProc(this DatabaseName databaseName)
        {
            string SQL = @"
        SELECT Routine_Name , DATA_TYPE, ROUTINE_TYPE
          FROM {0}.information_schema.routines 
         WHERE Routine_Name IN (SELECT name FROM dbo.sysobjects)
        ";
            
            return Use(databaseName, string.Format(SQL, databaseName.Name));

        }

        public static DataTable AllIndices(this DatabaseName databaseName)
        {
            string SQL = @"
       		   SELECT 
                 TableName = t.name,
                 IndexName = ind.name,
                 IndexId = ind.index_id,
                 ColumnId = ic.index_column_id,
                 ColumnName = col.name,
				 [Clustered] = ind.type_desc
            FROM sys.indexes ind 
				INNER JOIN sys.index_columns ic ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id 
				INNER JOIN sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id 
				INNER JOIN sys.tables t ON ind.object_id = t.object_id 
            WHERE 
                ind.is_primary_key = 0 
                AND ind.is_unique = 0 
                AND ind.is_unique_constraint = 0 
                AND t.is_ms_shipped = 0 
            ORDER BY 
                 t.name, ind.name, ind.index_id, ic.index_column_id 
          ";

            return Use(databaseName, SQL);
        }
        

        public static DataTable IndexSchema(this TableName tableName)
        {
            string SQL = @"
       		   SELECT 
                 TableName = t.name,
                 IndexName = ind.name,
                 IndexId = ind.index_id,
                 ColumnId = ic.index_column_id,
                 ColumnName = col.name,
				 [Clustered] = ind.type_desc
            FROM sys.indexes ind 
				INNER JOIN sys.index_columns ic ON  ind.object_id = ic.object_id and ind.index_id = ic.index_id 
				INNER JOIN sys.columns col ON ic.object_id = col.object_id and ic.column_id = col.column_id 
				INNER JOIN sys.tables t ON ind.object_id = t.object_id 
            WHERE 
                t.name = '{0}'
                AND ind.is_primary_key = 0 
                AND ind.is_unique = 0 
                AND ind.is_unique_constraint = 0 
                AND t.is_ms_shipped = 0 
            ORDER BY 
                 t.name, ind.name, ind.index_id, ic.index_column_id 
          ";

            return Use(tableName, SQL);
        }
    }
}
