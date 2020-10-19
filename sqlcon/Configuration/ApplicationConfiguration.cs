using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys;
using Sys.Stdio;

namespace sqlcon
{
    class ApplicationConfiguration : Configuration, IApplicationConfiguration
    {
        private const string _SERVER0 = "home";
        private const string _SERVERS = "servers";

        private const string _PATH = "path";
        private const string _MAXROWS = "maxrows";


        public int TopLimit { get; private set; } = 20;

        public string OutputFile { get; set; }
        public string XmlDbDirectory { get; private set; }
        public WorkingDirectory WorkingDirectory { get; }

        public ApplicationConfiguration()
        {
            WorkingDirectory = new WorkingDirectory();
        }

        public override bool Initialize(ConfigFiles cfg)
        {
            const string _FILE_OUTPUT = "output";
            const string _XML_DB_FOLDER = "xmldb";
            const string _LIMIT = "limit";
            const string _TOP = "top";
            const string _EXPORT_MAX_COUNT = "export_max_count";
            const string _WORKING_DIRECTORY = "working.directory.commands";

            base.Initialize(cfg);

            this.OutputFile = GetValue<string>(_FILE_OUTPUT, "script.sql");
            this.XmlDbDirectory = GetValue<string>(_XML_DB_FOLDER, "db");
            this.WorkingDirectory.SetCurrentDirectory(GetValue<string>(_WORKING_DIRECTORY, "."));

            var limit = DS[_LIMIT];

            if (limit[_TOP].Defined)
                this.TopLimit = (int)limit[_TOP];

            if (limit[_EXPORT_MAX_COUNT].Defined)
                Context.SetValue(_MAXROWS, (int)limit[_EXPORT_MAX_COUNT]);

            Context.SetValue(_PATH, GetValue(_PATH, "."));
            return true;
        }

        public string Path => Context.GetValue(_PATH, string.Empty);
        public int MaxRows => Context.GetValue(_MAXROWS, 2000);


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
