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

        public int TopLimit { get; private set; } = 20;
        public int MaxRows { get; private set; } = 2000;

        public string OutputFile { get; set; }
        public string XmlDbDirectory { get; private set; }
        public WorkingDirectory WorkingDirectory { get; }
        public string[] PATH { get; private set; }

        public ApplicationConfiguration()
        {
            WorkingDirectory = new WorkingDirectory();
        }

        public override bool Initialize(string cfgFile)
        {
            const string _FILE_OUTPUT = "output";
            const string _XML_DB_FOLDER = "xmldb";
            const string _LIMIT = "limit";
            const string _TOP = "top";
            const string _EXPORT_MAX_COUNT = "export_max_count";
            const string _WORKING_DIRECTORY = "working.directory.commands";
            const string _PATH = "path";

            base.Initialize(cfgFile);

            this.OutputFile = GetValue<string>(_FILE_OUTPUT, "script.sql");
            this.XmlDbDirectory = GetValue<string>(_XML_DB_FOLDER, "db");
            this.WorkingDirectory.SetCurrentDirectory(GetValue<string>(_WORKING_DIRECTORY, "."));

            var limit = DS[_LIMIT];
            
            if (limit[_TOP].Defined)
                this.TopLimit = (int)limit[_TOP];
            
            if (limit[_EXPORT_MAX_COUNT].Defined)
                this.MaxRows = (int)limit[_EXPORT_MAX_COUNT];

            string path = GetValue(_PATH, ".");
            this.PATH = path.Split(';');
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
