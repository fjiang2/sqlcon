using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;

using Sys.Data.IO;
using Sys.Networking;
using Sys.Stdio;
using Tie;

namespace sqlcon
{
    class Configuration : IConfiguration
    {
        const string _SERVER0 = "home";
        const string _SERVERS = "servers";

        const string _FUNC_CONFIG = "config";
        const string _FUNC_CFG = "cfg";

        const string _FILE_SYSTEM_CONFIG = "sqlcon.cfg";
        const string _FILE_OUTPUT = "output";
        const string _XML_DB_FOLDER = "xmldb";

        const string _WORKING_DIRECTORY = "working.directory.commands";

        const string _LIMIT = "limit";

        private Memory Cfg = new Memory();

        public string UserConfigurationFile { get; private set; } = "user.cfg";

        public string OutputFile { get; set; }
        public string XmlDbDirectory { get; private set; }
        public WorkingDirectory WorkingDirectory { get; }

        public int TopLimit { get; private set; } = 20;
        public int MaxRows { get; private set; } = 2000;
        private static TextWriter cerr = Console.Error;


        public Configuration()
        {
            Script.FunctionChain.Add(functions);
            HostType.Register(typeof(DateTime), true);
            HostType.Register(typeof(Environment), true);
            Cfg.AddObject("MyDocuments", MyDocuments);
            WorkingDirectory = new WorkingDirectory();
        }

        public static string MyDocuments => Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\sqlcon";

        private IConnectionConfiguration connection = null;
        public IConnectionConfiguration Connection
        {
            get
            {
                if (connection == null)
                    connection = new ConnectionConfiguration(GetValue<string>(_SERVER0), Cfg.GetValue(_SERVERS));
                return connection;
            }
        }

        private static VAL functions(string func, VAL parameters, Memory DS)
        {
            const string _FUNC_LOCAL_IP = "localip";

            string conn;
            switch (func)
            {
                case _FUNC_CONFIG:
                    conn = ConnectionString.SearchXmlConnectionString(parameters);
                    if (conn != null)
                        return new VAL(conn);
                    else
                        return new VAL();

                case _FUNC_CFG:
                    conn = ConnectionString.SearchTieConnectionString(parameters, DS);
                    if (conn != null)
                        return new VAL(conn);
                    else
                        return new VAL();

                case "include":
                    include(parameters, DS);
                    return new VAL();

                case "mydoc":
                    return new VAL(MyDocuments);

                case _FUNC_LOCAL_IP:
                    if (parameters.Size > 1)
                    {
                        cerr.WriteLine($"function {_FUNC_LOCAL_IP} requires 0, 1 or 2 parameters");
                        return new VAL();
                    }

                    if (parameters.Size == 1 && parameters[0].VALTYPE != VALTYPE.intcon)
                    {
                        cerr.WriteLine($"function {_FUNC_LOCAL_IP}(nic) requires integer parameter");
                        return new VAL();
                    }

                    if (parameters.Size == 2 && parameters[1].VALTYPE != VALTYPE.intcon)
                    {
                        cerr.WriteLine($"function {_FUNC_LOCAL_IP}(nic, port) requires integer parameter");
                        return new VAL();
                    }

                    int index = 0;
                    if (parameters.Size == 1)
                        index = (int)parameters[0];

                    int port = 0;
                    if (parameters.Size == 2)
                        port = (int)parameters[1];

                    string address = LocalHost.GetLocalIP(index);
                    if (port > 0)
                        return new VAL($"{address}:{port}");
                    else
                        return new VAL(address);
            }

            return null;
        }

        private bool TryReadCfg(string cfgFile)
        {

            using (var reader = new StreamReader(cfgFile))
            {
                string code = reader.ReadToEnd();
                if (string.IsNullOrEmpty(code))
                    return false;

                try
                {
                    Script.Execute(code, Cfg);
                }
                catch (Exception ex)
                {
                    cerr.WriteLine($"configuration file format error in {cfgFile}, {ex.Message}");
                    return false;
                }
            }

            return true;
        }

        public VAL GetValue(VAR variable)
        {
            return Cfg[variable];
        }

        public T GetValue<T>(string variable, T defaultValue = default(T))
        {
            VAL val = Cfg.GetValue(variable);
            if (val.Defined)
            {
                if (typeof(T) == typeof(VAL))
                    return (T)(object)val;
                else if (val.HostValue is T)
                    return (T)val.HostValue;
                else if (typeof(T).IsEnum && val.HostValue is int)
                    return (T)val.HostValue;
            }

            return defaultValue;
        }



        public bool Initialize(string cfgFile)
        {

            string theDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string sysCfgFile = Path.Combine(theDirectory, _FILE_SYSTEM_CONFIG);

            if (!File.Exists(sysCfgFile))
            {
                cerr.WriteLine($"configuration file {sysCfgFile} not found");
                return false;
            }

            if (!TryReadCfg(sysCfgFile))
                return false;

            //user.cfg is optional
            if (!string.IsNullOrEmpty(cfgFile) && File.Exists(cfgFile))
            {
                this.UserConfigurationFile = cfgFile;
                TryReadCfg(cfgFile);
            }

            this.OutputFile = Cfg.GetValue<string>(_FILE_OUTPUT, "script.sql");
            this.XmlDbDirectory = Cfg.GetValue<string>(_XML_DB_FOLDER, "db");
            this.WorkingDirectory.SetCurrentDirectory(Cfg.GetValue<string>(_WORKING_DIRECTORY, "."));

            var limit = Cfg[_LIMIT];
            if (limit["top"].Defined)
                this.TopLimit = (int)limit["top"];

            if (limit["export_max_count"].Defined)
                this.MaxRows = (int)limit["export_max_count"];


            CopyVariableContext(stdio.FILE_LOG);
            CopyVariableContext(stdio.FILE_EDITOR);

            CopyContext(Cfg["Context"]);

            return true;
        }

        private static void CopyContext(VAL context)
        {
            if (context.Defined && context.IsAssociativeArray())
            {
                foreach (Member member in context.Members)
                {
                    Context.DS.Add(member.Name, member.Value);
                }
            }
        }

        private void CopyVariableContext(string from, string to = null)
        {
            if (to == null)
                to = from;

            VAL val = Cfg.GetValue(from);
            if (val.Defined)
                Context.DS.Add(to, val);
        }






        /// <summary>
        /// load cfg file from ftp site or web site
        /// </summary>
        /// <param name="val"></param>
        /// <param name="DS"></param>
        private static void include(VAL val, Memory DS)
        {
            if (val.Size != 1 || val[0].VALTYPE != VALTYPE.stringcon)
            {
                cerr.WriteLine("required 1 parameters on function include(file), file can be local disk file, hyperlink, and ftp link");
                return;
            }

            string url = (string)val[0];
            if (string.IsNullOrEmpty(url))
                return;

            var link = FileLink.CreateLink(url);
            bool exists = false;
            try
            {
                exists = link.Exists;
            }
            catch (Exception ex)
            {
                cerr.WriteLine($"configuration file {link} doesn't exist, {ex.Message}");
                return;
            }

            if (!exists)
            {
                cerr.WriteLine($"configuration file {link} doesn't exist");
                return;
            }

            string code = null;
            try
            {
                code = link.ReadAllText();
            }
            catch (Exception ex)
            {
                cerr.WriteLine($"failed to load configuration file {link}, {ex.Message}");
                return;
            }

            if (string.IsNullOrEmpty(code))
                return;

            try
            {
                Script.Execute(code, DS);
            }
            catch (Exception ex)
            {
                cerr.WriteLine($"configuration file format error in {link}, {ex.Message}");
            }
        }



    }
}

