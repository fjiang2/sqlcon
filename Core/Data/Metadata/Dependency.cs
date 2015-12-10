using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

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
        }

		class SelectDef
		{
		  public string SELECT {get; set;}
		  public bool isSelect {get; set;}
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

        public string DELETE(TableName tname)
        {
            StringBuilder builder = new StringBuilder();
			var fkrows = GetFkRows(tname);
            var pkrows = GetPkRows(tname);
            if (fkrows.Length > 0)
			{
				var select = new SelectDef { SELECT = fkrows[0].pkColumn, isSelect=false};
				DELETE(fkrows, select, builder);
			}
            //sql = deleteTemplate(tname, string.Format("{0}=@{0}", "ID");
            //builder.AppendLine(sql);
            return builder.ToString();
        }

        private string deleteTemplate(RowDef row, SelectDef def)
        {
			if(def.isSelect)
				return string.Format("DELETE FROM {0} WHERE [{1}] IN ({2})", row.fkTable.FormalName, row.fkColumn, def.SELECT); 
			else
				return string.Format("DELETE FROM {0} WHERE [{1}] = @{2}", row.fkTable.FormalName, row.fkColumn, def.SELECT); 
        }

		private SelectDef selectTemplate(RowDef row, SelectDef def)
        {
			string select;
			if(def.isSelect)
				select = string.Format("SELECT [{0}] FROM {1} WHERE [{2}] IN ({3})", row.pkColumn, row.fkTable.FormalName, row.fkColumn, def.SELECT); 
			else
				select = string.Format("SELECT [{0}] FROM {1} WHERE [{2}] =@{3}", row.pkColumn, row.fkTable.FormalName, row.fkColumn, def.SELECT);

            return new SelectDef { SELECT = select, isSelect = true };
        }

		
        private void DELETE(RowDef[] fkrows, SelectDef select, StringBuilder builder)
        {
            if (fkrows.Length == 0)
                return;

            foreach (var row in fkrows)
            {
                DELETE(GetFkRows(row.fkTable), selectTemplate(row, select), builder);
				string sql = deleteTemplate(row, select);
                builder.AppendLine(sql);
            }


        }
    }
}
