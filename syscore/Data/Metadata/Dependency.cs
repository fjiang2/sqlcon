using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data
{


    class Dependency
    {
        private DependencyInfo[] rows;

        public Dependency(DatabaseName dname)
        {
            this.rows = dname
               .Provider
               .Schema
               .GetDependencySchema(dname);
        }

        public ForeignKeys ByForeignKeys(TableName tname)
        {
            var L = rows.Where(row => row.PkTable.Equals(tname)).ToArray();
            List<ForeignKey> keys = new List<ForeignKey>();
            foreach (var l in L)
            {
                keys.Add(new ForeignKey
                {
                    TableName = l.FkTable,
                    FK_Column = l.FkColumn,
                    PK_Schema = tname.SchemaName,
                    PK_Table = tname.Name,
                    PK_Column = l.PkColumn
                });
            }

            return new ForeignKeys(keys.ToArray());
        }

        private DependencyInfo[] GetFkRows(TableName pk)
            => rows.Where(row => row.PkTable.Equals(pk)).ToArray();

        private DependencyInfo[] GetPkRows(TableName fk)
            => rows.Where(row => row.FkTable.Equals(fk)).ToArray();

        public TableName[] GetDependencyTableNames(DatabaseName databaseName)
        {
            var dict = rows.GroupBy(
                    row => row.FkTable,
                    (Key, rows) => new
                    {
                        FkTable = Key,
                        PkTables = rows.Select(row => row.PkTable).ToArray()
                    })
                .ToDictionary(row => row.FkTable, row => row.PkTables);


            TableName[] names = databaseName.GetTableNames();

            List<TableName> history = new List<TableName>();

            foreach (var tname in names)
            {
                if (history.IndexOf(tname) < 0)
                    Iterate(tname, dict, history);
            }

            return history.ToArray();
        }

        private static void Iterate(TableName tableName, Dictionary<TableName, TableName[]> dict, List<TableName> history)
        {
            if (!dict.ContainsKey(tableName))
            {
                if (history.IndexOf(tableName) < 0)
                {
                    history.Add(tableName);
                }
            }
            else
            {
                foreach (var name in dict[tableName])
                    Iterate(name, dict, history);

                if (history.IndexOf(tableName) < 0)
                {
                    history.Add(tableName);
                }
            }
        }


        public string DROP_TABLE(TableName tname, bool ifExists)
        {
            StringBuilder builder = new StringBuilder();
            var fkrows = GetFkRows(tname);
            foreach (var row in fkrows)
            {
                DROP_TABLE(row, GetFkRows(row.FkTable), ifExists, builder);
                builder.AppendLine(dropTemplate(row.FkTable, ifExists));
            }

            return builder.ToString();
        }

        private void DROP_TABLE(DependencyInfo pkrow, DependencyInfo[] fkrows, bool ifExists, StringBuilder builder)
        {
            if (fkrows.Length == 0)
                return;

            List<string> completed = new List<string>();
            foreach (var row in fkrows)
            {
                DependencyInfo[] getFkRows = GetFkRows(row.FkTable);

                string stamp = $"{row.FkTable}=>{row.PkTable}";
                if (completed.IndexOf(stamp) < 0)   //don't allow to same fk=>pk many times
                {
                    DROP_TABLE(row, getFkRows, ifExists, builder);
                    builder.AppendLine(dropTemplate(row.FkTable, ifExists));
                    completed.Add(stamp);
                }
            }

            return;
        }

        public string DELETE(TableName tname)
        {
            StringBuilder builder = new StringBuilder();
            var fkrows = GetFkRows(tname);
            foreach (var row in fkrows)
            {
                string locator = $"[{row.FkColumn}] = @{row.PkColumn}";
                DELETE(row, GetFkRows(row.FkTable), locator, builder);
                builder.AppendLine($"DELETE FROM {row.FkTable.FormalName} WHERE [{row.FkColumn}] = @{row.PkColumn}");
            }

            return builder.ToString();
        }


        private void DELETE(DependencyInfo pkrow, DependencyInfo[] fkrows, string locator, StringBuilder builder)
        {
            if (fkrows.Length == 0)
                return;

            foreach (var row in fkrows)
            {
                string sql = $"[{row.FkColumn}] IN (SELECT [{row.PkColumn}] FROM {pkrow.FkTable.FormalName} WHERE {locator})";
                DependencyInfo[] getFkRows = GetFkRows(row.FkTable);

                var columnInfo = row.FkTable.GetTableSchema().Columns[row.FkColumn];

                if (columnInfo.Nullable)
                {
                    builder.AppendLine(updateTemplate(row, locator));
                }
                else
                {
                    DELETE(row, getFkRows, sql, builder);
                    builder.AppendLine(deleteTemplate(row, locator));
                }
            }
        }

        private static string dropTemplate(TableName tableName, bool ifExists)
        {
            return new SqlTemplate(tableName.FormalName, DbAgentStyle.SqlServer).DropTable(ifExists);
        }

        private string deleteTemplate(DependencyInfo row, string locator)
        {
            return $"DELETE FROM {row.FkTable.FormalName} WHERE [{row.FkColumn}] IN (SELECT [{row.PkColumn}] FROM {row.PkTable.FormalName} WHERE {locator})";
        }

        private string updateTemplate(DependencyInfo row, string locator)
        {
            return $"UPDATE {row.FkTable.FormalName} SET {row.FkColumn} = NULL WHERE [{row.FkColumn}] IN (SELECT [{row.PkColumn}] FROM {row.PkTable.FormalName} WHERE {locator})";
        }

        private string selectTemplate(DependencyInfo pkrow, DependencyInfo fkrow)
        {
            return $"SELECT [{fkrow.PkColumn}] FROM {fkrow.PkTable.FormalName} WHERE [{fkrow.FkColumn}] = @{pkrow.PkColumn}";
        }




    }
}
