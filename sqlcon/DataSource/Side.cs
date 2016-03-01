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
using Sys;

namespace sqlcon
{
    class Side  : IDataPath
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
            return new DatabaseClause(DatabaseName).GenerateClause();
        }

        public bool ExecuteScript(string scriptFile)
        {
            return ExecuteSqlScript(this.Provider, scriptFile);
        }

        private static bool ExecuteSqlScript(ConnectionProvider provider, string scriptFile)
        {
            if (!File.Exists(scriptFile))
            {
                stdio.ErrorFormat("no input file found : {0}", scriptFile);
                return false;
            }

            stdio.WriteLine("Execute {0}", scriptFile);
            var script = new SqlScript(provider, scriptFile);
            script.Reported += (sender, e) =>
            {
                // stdio.WriteLine("processed: {0}>{1}", e.Value1, e.Value2);
            };

            bool hasError = false;
            script.Error += (sender, e) =>
            {
                hasError = true;
                stdio.ErrorFormat("line:{0}, {1}, SQL:{2}", e.Line, e.Exception.Message, e.Command);
            };

            Func<bool> stopOnError = () =>
            {
                return !stdio.YesOrNo("are you sure to contune (yes/no)?:");
            };

            script.Execute(stopOnError);
            stdio.WriteLine("completed to run {0}", scriptFile);

            return !hasError;
        }

        

        public override string ToString()
        {
            return string.Format("Server={0}, Db={1}",Provider.DataSource, this.DatabaseName.Name);
        }

    }
}
