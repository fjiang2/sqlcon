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
using Sys.Stdio;
using Sys;

namespace sqlcon
{
    class Side : IDataPath
    {
        public DatabaseName DatabaseName { get; private set; }
        private ConnectionProvider provider;

        public Side(ConnectionProvider provider)
        {
            this.provider = provider;
            this.DatabaseName = new DatabaseName(provider, Provider.InitialCatalog);
        }


        public Side(ConnectionProvider provider, DatabaseName dname)
        {
            this.provider = provider;
            this.DatabaseName = dname;
        }

        public void UpdateDatabase(ConnectionProvider provider)
        {
            this.provider = provider;
            this.DatabaseName = new DatabaseName(provider, Provider.InitialCatalog);
        }

        public ConnectionProvider Provider
        {
            get { return this.provider; }
        }


        public string Path
        {
            get { return this.provider.Name; }
        }

        public string GenerateScript()
        {
            return DatabaseName.GenerateClause();
        }

        public bool ExecuteScript(string scriptFile, int batchSize = 1, bool verbose = false)
        {
            return ExecuteSqlScript(this.Provider, scriptFile, batchSize, verbose);
        }

        private static bool ExecuteSqlScript(ConnectionProvider provider, string scriptFile, int batchSize, bool verbose)
        {
            if (!File.Exists(scriptFile))
            {
                cerr.WriteLine($"no input file found : {scriptFile}");
                return false;
            }

            cout.WriteLine("executing {0}", scriptFile);
            var script = new SqlScript(provider, scriptFile)
            {
                BatchSize = batchSize
            };

            script.Reported += (sender, e) =>
            {
                if (verbose)
                    cout.WriteLine($"processed line:{e.Line} batch:{e.BatchLine}/{e.BatchSize} total:{e.TotalSize}");
            };

            bool hasError = false;
            script.Error += (sender, e) =>
            {
                hasError = true;
                cerr.WriteLine($"line:{e.Line}, {e.Exception.Message}, SQL:{e.Command}");
            };

            Func<bool> stopOnError = () =>
            {
                return !cin.YesOrNo("are you sure to continue (yes/no)?");
            };

            script.Execute(stopOnError);
            cout.WriteLine("completed.");

            return !hasError;
        }



        public override string ToString()
        {
            return string.Format("Server={0}, Db={1}", Provider.DataSource, this.DatabaseName.Name);
        }

    }
}
