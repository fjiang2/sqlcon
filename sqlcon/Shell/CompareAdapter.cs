using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys.Data;
using Sys.Data.Comparison;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;

namespace sqlcon
{

    class CompareAdapter  
    {

        public Side Side1 {get; private set;}
        public Side Side2 {get; private set;}

        public CompareAdapter(Side side1, Side side2)
        {
            this.Side1 = side1;
            this.Side2 = side2;
        }
     
        private static bool Exists(TableName tname)
        {
            if (!tname.Exists())
            {
                stdio.WriteLine("table not found : {0}", tname);
                return false;
            }

            return true;
        }

        private static bool Exists(DatabaseName dname)
        {
            if (!dname.Exists())
            {
                stdio.WriteLine("table not found : {0}", dname);
                return false;
            }

            return true;
        }


        public string Run(ActionType compareType, TableName[] N1, TableName[] N2, Configuration cfg, string[] exceptColumns)
        {
            DatabaseName dname1 = Side1.DatabaseName;
            DatabaseName dname2 = Side2.DatabaseName;

            stdio.WriteLine("server1: {0} default database:{1}", Side1.Provider.DataSource, dname1.Name);
            stdio.WriteLine("server2: {0} default database:{1}", Side2.Provider.DataSource, dname2.Name);

            if (!Exists(dname1) || !Exists(dname2))
                return string.Empty;

            StringBuilder builder = new StringBuilder();
            builder.AppendFormat("-- sqlcon:", Side1.Provider.DataSource, dname1.Name).AppendLine();
            builder.AppendFormat("-- compare server={0} db={1}", Side1.Provider.DataSource, dname1.Name).AppendLine();
            builder.AppendFormat("--         server={0} db={1} @ {2}", Side2.Provider.DataSource, dname2.Name, DateTime.Now).AppendLine();

            CancelableWork.CanCancel(cts =>
            {
                foreach (var tname1 in N1)
                {
                    if (cts.IsCancellationRequested)
                        return;

                    TableName tname2 = N2.Where(t => t.ShortName == tname1.ShortName).FirstOrDefault();
                    if (tname2 == null)
                    {
                        tname2 = new TableName(dname2, tname1.SchemaName, tname1.ShortName);
                    }

                    if (compareType == ActionType.CompareData && cfg.compareExcludedTables.FirstOrDefault(t => t == tname1.Name) != null)
                    {
                        stdio.WriteLine("{0} is excluded", tname1);
                        continue;
                    }

                    if (tname2.Exists())
                        builder.Append(CompareTable(compareType, CompareSideType.compare, tname1, tname2, cfg.PK, exceptColumns));
                    else
                    {
                        if (compareType == ActionType.CompareSchema)
                        {
                            string sql = tname1.GenerateCluase();
                            stdio.WriteLine(sql);
                            builder.Append(sql);
                        }
                        else
                        {
                            stdio.WriteLine("{0} doesn't exist", tname2);
                        }
                    }
                }
                
            });

            return builder.ToString();
        }

        private string CompareDatabaseSchema(CompareSideType sideType, DatabaseName db1, DatabaseName db2)
        {
            stdio.WriteLine("{0} database schema {1} => {2}", sideType, db1.Name, db2.Name);
            return Compare.DatabaseSchemaDifference(sideType, db1, db2);
        }

        private string CompareDatabaseData(CompareSideType sideType, DatabaseName db1, DatabaseName db2, string[] excludedtables)
        {
            stdio.WriteLine("compare database data {0} => {1}", db1.Name, db2.Name);
            if (excludedtables != null && excludedtables.Length > 0)
                stdio.WriteLine("ignore tables: {0}", string.Join(",", excludedtables));
            return Compare.DatabaseDifference(sideType, db1, db2, excludedtables);
        }

  
        public string CompareTable(ActionType actiontype, CompareSideType sidetype, TableName tname1, TableName tname2, Dictionary<string, string[]> pk, string[] exceptColumns)
        {
            TableSchema schema1 = new TableSchema(tname1);
            TableSchema schema2 = new TableSchema(tname2);

            if (!Exists(tname1))
                return string.Empty;


            string sql = string.Empty;

            if (actiontype == ActionType.CompareSchema)
            {
                sql = Compare.TableSchemaDifference(sidetype, tname1, tname2);
                stdio.WriteLine("completed to {0} table schema {1} => {2}", sidetype, tname1, tname2);
            }
            else if (actiontype == ActionType.CompareData)
            {
                if (!Exists(tname2))
                {
                    return string.Empty;
                }

                if (Compare.TableSchemaDifference(sidetype, tname1, tname2) != string.Empty)
                {
                    stdio.WriteLine("failed to {0} becuase of different table schemas", sidetype);
                    return string.Empty;
                }

                bool hasPk = schema1.PrimaryKeys.Length > 0;
                sql = Compare.TableDifference(sidetype, schema1, schema2, schema1.PrimaryKeys.Keys, exceptColumns);

                if (!hasPk)
                {
                    stdio.WriteLine("warning: no primary key found : {0}", tname1);

                    string key = tname1.Name.ToUpper();
                    if (pk.ContainsKey(key))
                    {
                        stdio.WriteLine("use predefine keys defined in ini file: {0}", tname1);
                        sql = Compare.TableDifference(sidetype, schema1, schema2, pk[key], exceptColumns);
                    }
                    else
                    {
                        stdio.WriteLine("use entire row as primary keys:{0}", tname1);
                        var keys = schema1.Columns.Select(row => row.ColumnName).ToArray();
                        sql = Compare.TableDifference(sidetype, schema1, schema2, keys, exceptColumns);
                    }
                }

                stdio.WriteLine("completed to {0} table data {1} => {2}", sidetype, tname1, tname2);
            }

            if (sql != string.Empty && sidetype == CompareSideType.compare)
                stdio.WriteLine(sql);

            return sql;
        }

    }
}
