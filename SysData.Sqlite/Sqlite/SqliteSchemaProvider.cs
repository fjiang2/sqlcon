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
        public const string SQLITE_DATABASE_NAME = "SQLiteDb";

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
            List<SchemaRow> rows = new List<SchemaRow>();
            LoadColumns(rows, tname);
            LoadForeignKeys(rows, tname);

            DataTable schemaTable = SchemaRowExtension.CreateTable();
            rows.ToDataTable(schemaTable);
            schemaTable.Columns.Remove(SchemaRowExtension._SCHEMANAME);
            schemaTable.Columns.Remove(SchemaRowExtension._TABLENAME);
            schemaTable.AcceptChanges();
            return schemaTable;
        }



        private static void LoadColumns(List<SchemaRow> rows, TableName tname)
        {
            string SQL = $"SELECT * FROM PRAGMA_TABLE_INFO('{tname.Name}')";
            var dt = new SqlCmd(tname.Provider, SQL).FillDataTable();
            foreach (DataRow row in dt.Rows)
            {
                string columnName = row.GetField<string>("name");
                SchemaRow _row = rows.Where(x => x.TableName == tname.Name && x.ColumnName == columnName).SingleOrDefault();
                if (_row == null)
                {
                    _row = new SchemaRow
                    {
                        SchemaName = SchemaName.empty,
                        TableName = tname.Name,
                        ColumnName = row.GetField<string>("name"),
                    };
                }

                _row.DataType = row.GetField<string>("type");
                _row.Length = 0;
                _row.Nullable = row.GetField<long>("notnull") == 0;
                _row.precision = 0;
                _row.scale = 0;

                _row.IsPrimary = row.GetField<long>("pk") == 1;
                _row.IsIdentity = false;
                _row.IsComputed = false;
                _row.definition = null;

                _row.PKContraintName = _row.IsPrimary ? $"PK_{tname.Name}" : null;

                ParseType(_row, _row.DataType);
                rows.Add(_row);
            }
        }

        private static void LoadForeignKeys(List<SchemaRow> rows, TableName tname)
        {
            string SQL = $"SELECT * FROM PRAGMA_FOREIGN_KEY_LIST('{tname.Name}')";
            var dt = new SqlCmd(tname.Provider, SQL).FillDataTable();
            foreach (DataRow row in dt.Rows)
            {
                string columnName = row.GetField<string>("from");
                SchemaRow _row = rows.Where(x => x.TableName == tname.Name && x.ColumnName == columnName).SingleOrDefault();

                _row.PK_Schema = SchemaName.empty;
                _row.PK_Table = row.GetField<string>("table");
                _row.PK_Column = row.GetField<string>("to");
                _row.FKContraintName = $"FK_{tname.Name}_{_row.PK_Table}";
            }
        }


        private static void ParseType(SchemaRow row, string dataType)
        {
            string type = dataType.ToLower();

            switch (type)
            {
                case "int":
                case "integer":
                    row.DataType = "int";
                    return;

                case "real":
                case "double":
                    row.DataType = "float";
                    return;

                case "blob":
                    row.DataType = "binary";
                    return;
            }

            if (type.IndexOf('(') > 0 && type.IndexOf(')') > 0)
            {
                string[] items = type.Split(new char[] { '(', ',', ')' }, StringSplitOptions.RemoveEmptyEntries);
                string _type = items[0];
                int a1 = int.Parse(items[1]);
                switch (_type)
                {
                    case "nvarchar":
                        row.DataType = "nvarchar";
                        row.Length = Convert.ToInt16(a1 * 2);
                        return;

                    case "varchar":
                        row.DataType = "varchar";
                        row.Length = Convert.ToInt16(a1);
                        return;

                    case "numeric":
                        row.DataType = "numeric";
                        row.precision = (byte)short.Parse(items[1]);
                        row.scale = (byte)short.Parse(items[2]);
                        return;

                    default:
                        throw new NotImplementedException();
                }
            }

            row.DataType = type;
            return;
        }

        public override DataTable GetDatabaseSchema(DatabaseName dname)
        {
            return LoadDatabaseSchema(dname.ServerName, new DatabaseName[] { dname })
                .Tables[dname.Name];
        }

        public override DataSet GetServerSchema(ServerName sname)
        {
            return LoadDatabaseSchema(sname, sname.GetDatabaseNames());
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


        private static DataTable LoadDatabaseSchema(DatabaseName dname)
        {
            List<SchemaRow> rows = new List<SchemaRow>();
            foreach (TableName tname in dname.GetTableNames())
            {
                LoadColumns(rows, tname);
                LoadForeignKeys(rows, tname);
            }

            DataTable schemaTable = SchemaRowExtension.CreateTable();
            rows.ToDataTable(schemaTable);
            schemaTable.AcceptChanges();
            return schemaTable;
        }

        public static DataSet LoadDatabaseSchema(ServerName sname, IEnumerable<DatabaseName> dnames)
        {
            DataSet ds = new DataSet();
            ds.DataSetName = sname.Path;

            foreach (DatabaseName dname in dnames)
            {
                DataTable dt = LoadDatabaseSchema(dname);
                dt.TableName = dname.Name;
                ds.Tables.Add(dt);
            }

            return ds;
        }

    }
}

