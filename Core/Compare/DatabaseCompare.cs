using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sys.Data.Comparison
{
    public class DatabaseCompare
    {

        public static string Difference(string connFrom, string connTo, string dbNameFrom, string dbNameTo)
        {
            var pvd1 = DataProviderManager.Register("Source", DataProviderType.SqlServer, connFrom);
            var pvd2 = DataProviderManager.Register("Sink", DataProviderType.SqlServer, connTo);

            return Difference(pvd1, pvd2, dbNameFrom, dbNameTo);
        }

        public static string Difference(DataProvider from, DataProvider to, string dbNameFrom, string dbNameTo)
        {
            
            DatabaseName dname1 = new DatabaseName(from, dbNameFrom);
            DatabaseName dname2 = new DatabaseName(to, dbNameTo);

            string[] names = MetaDatabase.GetTableNames(dname1);

            StringBuilder builder = new StringBuilder();
            foreach (string tableName in names)
            {
                TableName tname1 = new TableName(dname1, tableName);
                TableName tname2 = new TableName(dname2, tableName);

                string[] primaryKeys = InformationSchema.PrimaryKeySchema(tname1).ToArray<string>(0);
                if (primaryKeys.Length == 0)
                    continue;

                if (MetaDatabase.TableExists(tname2))
                {
                    builder.Append(TableCompare.Difference(tname1, tname2, tableName, primaryKeys));
                }
                else
                {
                    builder.Append(TableCompare.Rows(tableName, from));
                }

                builder.AppendLine();
            }

            return builder.ToString();
        }
    }
}
