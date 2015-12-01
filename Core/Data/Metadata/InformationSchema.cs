using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;


namespace Sys.Data
{
    static class InformationSchema
    {

        private static string SQL_SCHEMA = @"
SELECT 
	{0} 
    c.name AS ColumnName,
    ty.name AS DataType,
    c.max_length AS Length,
    c.is_nullable AS Nullable,
    c.precision,
    c.scale,
    CASE WHEN p.CONSTRAINT_NAME IS NOT NULL THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS IsPrimary,
    c.is_identity AS IsIdentity,
    c.is_computed AS IsComputed,
    d.definition,
	p.CONSTRAINT_NAME AS PKContraintName,
	f.PK_Schema,
	f.PK_Table,
	f.PK_Column,
	f.Constraint_Name AS FKContraintName
    FROM sys.tables t 
        INNER JOIN sys.columns c ON t.object_id = c.object_id 
        INNER JOIN sys.types ty ON ty.system_type_id =c.system_type_id AND ty.name<>'sysname' AND ty.is_user_defined = 0
        LEFT JOIN sys.Computed_columns d ON t.object_id = d.object_id AND c.name = d.name
		LEFT JOIN (SELECT pk.TABLE_NAME, k.COLUMN_NAME, pk.CONSTRAINT_NAME
					FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS pk 
						INNER JOIN INFORMATION_SCHEMA.KEY_COLUMN_USAGE k ON  k.TABLE_NAME = pk.TABLE_NAME AND k.CONSTRAINT_NAME = pk.CONSTRAINT_NAME
						WHERE pk.CONSTRAINT_TYPE = 'PRIMARY KEY'
						) p	ON p.TABLE_NAME = t.name  AND p.COLUMN_NAME = c.name
		LEFT JOIN (SELECT   FK.TABLE_SCHEMA AS FK_Schema,
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
				   ) f ON f.FK_Table = t.name AND f.FK_Column = c.name
{1}
ORDER BY t.name, c.column_id
";

        public static DataTable SqlTableSchema(TableName tableName)
        {
            DataTable dt1;
            string SQL = string.Format(SQL_SCHEMA, "", "WHERE t.name='{0}'");
            dt1 = Use(tableName, SQL);

            return dt1;
        }

      
        private static DataTable Use(this TableName tableName, string script)
        {
            StringBuilder builder = new StringBuilder();

            if (tableName.Provider.DpType != DbProviderType.SqlCe)
            {
                builder.AppendFormat("USE [{0}] ", tableName.DatabaseName.Name).AppendLine();
            }

            builder.AppendFormat(script, tableName.Name);

            return DataExtension.FillDataTable(tableName.Provider, builder.ToString());

        }


        private static string SqlDatabaseSchema(DatabaseName dname)
        {
            StringBuilder builder = new StringBuilder();
            string s = @"SCHEMA_NAME(t.schema_id) AS SchemaName,t.name AS TableName,";
            builder.AppendFormat("USE [{0}] ", dname.Name).AppendLine();
            builder.AppendLine(string.Format(SQL_SCHEMA, s, ""));

            return builder.ToString();
        }

        public static DataSet SqlServerSchema(ServerName sname, IEnumerable<DatabaseName> dnames)
        {
            StringBuilder builder = new StringBuilder();
            foreach (DatabaseName dname in dnames)
            {
                builder.AppendLine(SqlDatabaseSchema(dname));
            }

            DataSet ds = new SqlCmd(sname.Provider, builder.ToString()).FillDataSet();
            ds.DataSetName = sname.Path;
            int i = 0;
            foreach (DatabaseName dname in dnames)
            {
                ds.Tables[i++].TableName = dname.Name;
            }

            return ds;
        }

        public static DataTable XmlTableSchema(TableName tableName, DataTable schema)
        {
            DataTable dt = new DataTable();
            foreach (DataColumn column in schema.Columns)
            {
                if (column.ColumnName != "SchemaName" && column.ColumnName != "TableName")
                    dt.Columns.Add(new DataColumn(column.ColumnName, column.DataType));
            }

            var rows = schema.AsEnumerable().Where(row => row.Field<string>("SchemaName") == tableName.SchemaName && row.Field<string>("TableName").ToLower() == tableName.Name.ToLower());
            foreach (var row in rows)
            {
                DataRow newRow = dt.NewRow();
                foreach (DataColumn column in dt.Columns)
                {
                    newRow[column] = row[column.ColumnName];
                }

                dt.Rows.Add(newRow);
            }

            dt.AcceptChanges();
            return dt;
        }


        public static TableName[] XmlTableNames(this DatabaseName databaseName, DataTable schema)
        {
            var rows = schema.AsEnumerable()
                .Select(row=> new { schema =  row.Field<string>("SchemaName"), name = row.Field<string>("TableName")})
                .Distinct();

            List<TableName> tnames = new List<TableName>();
            foreach (var row in rows)
            {
                TableName tname = new TableName(databaseName, row.schema, row.name);
                tnames.Add(tname);
            }

            return tnames.ToArray();
        }

    }
}
