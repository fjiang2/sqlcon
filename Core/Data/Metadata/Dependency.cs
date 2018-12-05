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
        class RowDef
        {
            public TableName fkTable { get; set; }
            public TableName pkTable { get; set; }
            public string pkColumn { get; set; }
            public string fkColumn { get; set; }

            public override string ToString() => $"{fkTable.ShortName}.{fkColumn} => {pkTable.ShortName}.{pkColumn}";
        }

        private RowDef[] rows;

        public Dependency(DatabaseName dname)
        {
            var dt = dname
               .Provider
               .Schema
               .GetDependencySchema(dname)
               .AsEnumerable();

            rows = dt.Select(
                   row => new RowDef
                   {
                       fkTable = new TableName(dname, (string)row["FK_SCHEMA"], (string)row["FK_Table"]),
                       pkTable = new TableName(dname, (string)row["PK_SCHEMA"], (string)row["PK_Table"]),
                       pkColumn = (string)row["PK_Column"],
                       fkColumn = (string)row["FK_Column"]
                   })
                   .ToArray();
        }

        public ForeignKeys ByForeignKeys(TableName tname)
        {
            var L = rows.Where(row => row.pkTable.Equals(tname)).ToArray();
            List<ForeignKey> keys = new List<ForeignKey>();
            foreach (var l in L)
            {
                keys.Add(new ForeignKey
                {
                    TableName = l.fkTable,
                    FK_Column = l.fkColumn,
                    PK_Schema = tname.SchemaName,
                    PK_Table = tname.Name,
                    PK_Column = l.pkColumn
                });
            }

            return new ForeignKeys(keys.ToArray());
        }

        private RowDef[] GetFkRows(TableName pk)
            => rows.Where(row => row.pkTable.Equals(pk)).ToArray();

        private RowDef[] GetPkRows(TableName fk)
            => rows.Where(row => row.fkTable.Equals(fk)).ToArray();

        public TableName[] GetDependencyTableNames(DatabaseName databaseName)
        {
            var dict = rows.GroupBy(
                    row => row.fkTable,
                    (Key, rows) => new
                    {
                        FkTable = Key,
                        PkTables = rows.Select(row => row.pkTable).ToArray()
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
                DROP_TABLE(row, GetFkRows(row.fkTable), ifExists, builder);
                builder.AppendLine(dropTemplate(row.fkTable, ifExists));
            }

            return builder.ToString();
        }


        public string DELETE(TableName tname)
        {
            StringBuilder builder = new StringBuilder();
            var fkrows = GetFkRows(tname);
            foreach (var row in fkrows)
            {
                //string locator = string.Format("SELECT [{0}] FROM {1} WHERE {2}=@{2}", "{0}", row.pkTable.FormalName, row.pkColumn);
                string locator = string.Format("[{0}]=@{1}", row.fkColumn, row.pkColumn);
                DELETE(row, GetFkRows(row.fkTable), locator, builder);
                builder
                    .AppendFormat("DELETE FROM {0} WHERE [{1}] = @{2}", row.fkTable.FormalName, row.fkColumn, row.pkColumn)
                    .AppendLine();
            }

            return builder.ToString();
        }

        private static string dropTemplate(TableName tableName, bool ifExists)
        {
            return TableClause.DROP_TABLE(tableName, ifExists);
        }

        private string deleteTemplate(RowDef row, string locator)
        {
            return string.Format("DELETE FROM {0} WHERE [{1}] IN (SELECT [{2}] FROM {3} WHERE {4})",
                row.fkTable.FormalName,
                row.fkColumn,
                row.pkColumn,
                row.pkTable.FormalName,
                locator);
        }

        private string selectTemplate(RowDef pkrow, RowDef fkrow)
        {
            return string.Format("SELECT [{0}] FROM {1} WHERE [{2}] = @{3}", fkrow.pkColumn, fkrow.pkTable.FormalName, fkrow.fkColumn, pkrow.pkColumn);
        }


        private void DROP_TABLE(RowDef pkrow, RowDef[] fkrows, bool ifExists, StringBuilder builder)
        {
            if (fkrows.Length == 0)
                return;

            List<string> completed = new List<string>();
            foreach (var row in fkrows)
            {
                RowDef[] getFkRows = GetFkRows(row.fkTable);

                string stamp = $"{row.fkTable}=>{row.pkTable}";
                if (completed.IndexOf(stamp) < 0)   //don't allow to same fk=>pk many times
                {
                    DROP_TABLE(row, getFkRows, ifExists, builder);
                    builder.AppendLine(dropTemplate(row.fkTable, ifExists));
                    completed.Add(stamp);
                }
            }
        }


        private void DELETE(RowDef pkrow, RowDef[] fkrows, string locator, StringBuilder builder)
        {
            if (fkrows.Length == 0)
                return;

            foreach (var row in fkrows)
            {
                string sql = string.Format("[{0}] IN (SELECT [{1}] FROM {2} WHERE {3})", row.fkColumn, row.pkColumn, pkrow.fkTable.FormalName, locator);
                RowDef[] getFkRows = GetFkRows(row.fkTable);
                DELETE(row, getFkRows, sql, builder);

                builder.AppendLine(deleteTemplate(row, locator));
            }


        }
    }
}
