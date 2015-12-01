using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys;
using Sys.Data;
using System.Text.RegularExpressions;
using System.Data;

namespace sqlcon
{
    static class Tools
    {
        public static void FindName(this Side side, string match)
        {
            bool found = false;

            string sql = "SELECT name AS TableName FROM sys.tables";
            var dt = new SqlCmd(side.Provider, sql).FillDataTable();
            Search(match, dt, "TableName");
            if (dt.Rows.Count != 0)
            {
                found = true;
                stdio.DisplayTitle("Table Names");
                dt.ToConsole();
            };


            sql = @"
 SELECT 
	s.name as SchemaName,
	t.name as TableName,
    c.name AS ColumnName,
    ty.name AS DataType,
    c.max_length AS Length,
    CASE c.is_nullable WHEN 0 THEN 'NOT NULL' WHEN 1 THEN 'NULL' END AS Nullable
FROM sys.tables t 
     INNER JOIN sys.columns c ON t.object_id = c.object_id 
     INNER JOIN sys.types ty ON ty.system_type_id =c.system_type_id AND ty.name<>'sysname'
     LEFT JOIN sys.Computed_columns d ON t.object_id = d.object_id AND c.name = d.name
	 INNER JOIN sys.schemas s ON s.schema_id=t.schema_id
ORDER BY c.name, c.column_id
";
            dt = new SqlCmd(side.Provider, sql).FillDataTable();
            Search(match, dt, "ColumnName");
            if (dt.Rows.Count != 0)
            {
                found = true;
                stdio.DisplayTitle("Table Columns");
                dt.ToConsole();
            };


            sql = @"SELECT  SCHEMA_NAME(schema_id) SchemaName, name AS ViewName FROM sys.views ORDER BY name";
            dt = new SqlCmd(side.Provider, sql).FillDataTable();
            Search(match, dt, "ViewName");
            if (dt.Rows.Count != 0)
            {
                found = true;
                stdio.DisplayTitle("View Names");
                dt.ToConsole();
            }

            sql = @"
  SELECT 
	            VCU.TABLE_NAME AS ViewName, 
	            COL.COLUMN_NAME AS ColumnName,
	            COL.DATA_TYPE,
	            COL.IS_NULLABLE
            FROM INFORMATION_SCHEMA.VIEW_COLUMN_USAGE AS VCU
	            JOIN INFORMATION_SCHEMA.COLUMNS AS COL
	            ON  COL.TABLE_SCHEMA  = VCU.TABLE_SCHEMA
	            AND COL.TABLE_CATALOG = VCU.TABLE_CATALOG
	            AND COL.TABLE_NAME    = VCU.TABLE_NAME
	            AND COL.COLUMN_NAME   = VCU.COLUMN_NAME";

            dt = new SqlCmd(side.Provider, sql).FillDataTable();
            Search(match, dt, "ColumnName");
            if (dt.Rows.Count != 0)
            {
                found = true;
                stdio.DisplayTitle("View Columns");
                dt.ToConsole();
            }

            if (!found)
                stdio.WriteLine("nothing is found");
        }



        public static DataTable Search(string pattern, DataTable table, string columnName)
        {
            Regex regex = pattern.WildcardRegex();
            foreach (DataRow row in table.Rows)
            {
                if(!regex.IsMatch(row[columnName].ToString()))
                    row.Delete();
            }

            table.AcceptChanges();
            return table;
        }


    }



}
