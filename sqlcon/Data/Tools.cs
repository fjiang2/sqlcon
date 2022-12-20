using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys;
using Sys.Data;
using System.Text.RegularExpressions;
using System.Data;
using Sys.Stdio;

namespace sqlcon
{
    static class Tools
    {
        public static void FindName(this ConnectionProvider provider, DatabaseName[] dnames, string match)
        {
            switch (provider.Type)
            {
                case ConnectionProviderType.SqlServer:
                    FindNameOnSqlServer(provider, match);
                    return;

                case ConnectionProviderType.DbFile:
                    FindNameOnDbFile(provider, dnames, match);
                    return;
            }

            cout.WriteLine("command find is not supported on the current database server type.");
        }

        private static void FindNameOnSqlServer(ConnectionProvider provider, string match)
        {
            bool found = false;

            string sql = @"
SELECT
    s.name as SchemaName,
	t.name AS TableName
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
ORDER BY s.name, t.name";
            var dt = new SqlCmd(provider, sql).FillDataTable();
            Search(match, dt, "TableName");
            if (dt.Rows.Count != 0)
            {
                found = true;
                cout.WriteLine(ConsoleColor.Cyan, "Table Names");
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
ORDER BY s.name, c.name, c.column_id
";
            dt = new SqlCmd(provider, sql).FillDataTable();
            Search(match, dt, "ColumnName");
            if (dt.Rows.Count != 0)
            {
                found = true;
                cout.WriteLine(ConsoleColor.Cyan, "Table Columns");
                dt.ToConsole();
            };


            sql = @"SELECT  SCHEMA_NAME(schema_id) SchemaName, name AS ViewName FROM sys.views ORDER BY name";
            dt = new SqlCmd(provider, sql).FillDataTable();
            Search(match, dt, "ViewName");
            if (dt.Rows.Count != 0)
            {
                found = true;
                cout.WriteLine(ConsoleColor.Cyan, "View Names");
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

            dt = new SqlCmd(provider, sql).FillDataTable();
            Search(match, dt, "ColumnName");
            if (dt.Rows.Count != 0)
            {
                found = true;
                cout.WriteLine(ConsoleColor.Cyan, "View Columns");
                dt.ToConsole();
            }

            if (!found)
                cout.WriteLine("nothing is found");
        }


        private static void FindNameOnDbFile(ConnectionProvider provider, DatabaseName[] dnames, string match)
        {
            const string NAME_SPACE = "NameSpace";
            const string DATABASE_NAME = "Database";
            const string SCHEMA_NAME = "Schema";
            const string TABLE_NAME = "Table";
            const string COLUMN_NAME = "Column";

            var schema = provider.Schema;
            Regex regex = match.WildcardRegex();

            DataTable dt = new DataTable();
            dt.Columns.Add(DATABASE_NAME, typeof(string));
            dt.Columns.Add(NAME_SPACE, typeof(string));
            dt.Columns.Add(SCHEMA_NAME, typeof(string));
            dt.Columns.Add(TABLE_NAME, typeof(string));
            dt.Columns.Add(COLUMN_NAME, typeof(string));
            dt.Columns.Add("DataType", typeof(string));
            dt.Columns.Add("Length", typeof(int));
            dt.Columns.Add("Nullable", typeof(string));

            foreach (DatabaseName dname in dnames)
            {
                TableName[] tnames = schema.GetTableNames(dname);
                if (regex.IsMatch(dname.Name))
                {
                    var newRow = dt.NewRow();
                    newRow[DATABASE_NAME] = dname.Name;
                    newRow[NAME_SPACE] = dname.NameSpace;
                    dt.Rows.Add(newRow);
                }

                foreach (var tname in tnames)
                {
                    bool found = false;
                    var newRow = dt.NewRow();

                    newRow[DATABASE_NAME] = dname.Name;
                    newRow[NAME_SPACE] = dname.NameSpace;

                    if (regex.IsMatch(tname.SchemaName))
                    {
                        found = true;
                        newRow[SCHEMA_NAME] = tname.ShortName;
                    }

                    if (regex.IsMatch(tname.ShortName))
                    {
                        found = true;
                        newRow[TABLE_NAME] = tname.ShortName;
                    }

                    if (found)
                        dt.Rows.Add(newRow);

                    TableSchema tschema = new TableSchema(tname);
                    foreach (var column in tschema.Columns)
                    {
                        if (regex.IsMatch(column.ColumnName))
                        {
                            newRow = dt.NewRow();
                            newRow[DATABASE_NAME] = dname.Name;
                            newRow[SCHEMA_NAME] = tname.SchemaName;
                            newRow[TABLE_NAME] = tname.Name;
                            newRow[COLUMN_NAME] = column.ColumnName;
                            newRow["DataType"] = column.DataType.ToString();
                            if (column.Length != -1)
                                newRow["Length"] = column.Length;

                            newRow["Nullable"] = column.Nullable ? "NULL" : "NOT NULL";
                            newRow[NAME_SPACE] = dname.NameSpace;
                            dt.Rows.Add(newRow);
                        }
                    }
                }
            }

            if (dt.Rows.Count != 0)
            {
                //cout.WriteLine(ConsoleColor.Cyan, "Table Columns");
                dt.ToConsole(vertical: false, more: false, outputDbNull: false);
            }
            else
            {
                cout.WriteLine("nothing is found");
            }
        }


        private static DataTable Search(string pattern, DataTable table, string columnName)
        {
            Regex regex = pattern.WildcardRegex();
            foreach (DataRow row in table.Rows)
            {
                if (!regex.IsMatch(row[columnName].ToString()))
                    row.Delete();
            }

            table.AcceptChanges();
            return table;
        }

        public static long GetTableRowCount(this TableName tname, Locator locator = null)
        {
            TableReader tableReader;
            if (locator != null)
                tableReader = new TableReader(tname, locator);
            else
                tableReader = new TableReader(tname);

            return tableReader.Count;
        }


        public static int ForceLongToInteger(long cnt)
        {
            int count = 0;
            if (cnt < int.MaxValue)
                count = (int)cnt;
            else
            {
                count = int.MaxValue;
                cerr.WriteLine($"total count={cnt}, too many rows, progress bar may not be accurate");
            }

            return count;
        }
    }
}
