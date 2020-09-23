using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys;

namespace sqlcon
{
    class ApplicationConfiguration : Configuration, IApplicationConfiguration
    {
        private const string _SERVER0 = "home";
        private const string _SERVERS = "servers";

        const string _FILE_OUTPUT = "output";
        const string _XML_DB_FOLDER = "xmldb";
        const string _LIMIT = "limit";

        const string _WORKING_DIRECTORY = "working.directory.commands";

        public int TopLimit { get; private set; } = 20;
        public int MaxRows { get; private set; } = 2000;

        public string OutputFile { get; set; }
        public string XmlDbDirectory { get; private set; }
        public WorkingDirectory WorkingDirectory { get; }


        public ApplicationConfiguration()
        {
            WorkingDirectory = new WorkingDirectory();
        }

        public override bool Initialize(string cfgFile)
        {
            base.Initialize(cfgFile);

            this.OutputFile = GetValue<string>(_FILE_OUTPUT, "script.sql");
            this.XmlDbDirectory = GetValue<string>(_XML_DB_FOLDER, "db");
            this.WorkingDirectory.SetCurrentDirectory(GetValue<string>(_WORKING_DIRECTORY, "."));

            var limit = DS[_LIMIT];
            if (limit["top"].Defined)
                this.TopLimit = (int)limit["top"];

            if (limit["export_max_count"].Defined)
                this.MaxRows = (int)limit["export_max_count"];

            return true;
        }

        private IConnectionConfiguration connection = null;
        public IConnectionConfiguration Connection
        {
            get
            {
                if (connection == null)
                    connection = new ConnectionConfiguration(GetValue<string>(_SERVER0), DS.GetValue(_SERVERS));
                return connection;
            }
        }

    }
}
