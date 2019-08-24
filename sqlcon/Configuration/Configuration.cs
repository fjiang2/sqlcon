﻿using System;
using System.Collections.Generic;
using System.Data.OleDb;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;
using Sys.Data;
using Sys.Data.IO;
using Sys.Networking;
using Sys.Stdio;
using Tie;

namespace sqlcon
{
    class Configuration
    {

        public const string _SERVER0 = "home";

        const string _FUNC_CONFIG = "config";
        const string _FUNC_CFG = "cfg";
        const string _SERVERS = "servers";

        const string _FILE_SYSTEM_CONFIG = "sqlcon.cfg";
        const string _FILE_OUTPUT = "output";
        const string _XML_DB_FOLDER = "xmldb";
        const string _FILE_LOG = "log";
        const string _FILE_EDITOR = "editor";

        const string _WORKING_DIRECTORY = "working.directory.commands";

        const string _LIMIT = "limit";
        const string _DICTIONARY_TABLES = "dictionarytables";

        const string _QUEREY = "query";

        private Memory Cfg = new Memory();

        public string CfgFile { get; private set; } = "user.cfg";

        public string OutputFile { get; set; }
        public string XmlDbDirectory { get; set; }
        public WorkingDirectory WorkingDirectory { get; }

        public List<KeyValueTable> dictionarytables = new List<KeyValueTable>();
        public int Limit_Top = 20;
        public int Export_Max_Count = 2000;

        public Configuration()
        {
            Script.FunctionChain.Add(functions);
            HostType.Register(typeof(DateTime), true);
            HostType.Register(typeof(Environment), true);
            Cfg.AddObject("MyDocuments", MyDocuments);
            WorkingDirectory = new WorkingDirectory();
        }

        public static string MyDocuments
        {
            get
            {
                return Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments) + "\\sqlcon";
            }
        }

        private static VAL functions(string func, VAL parameters, Memory DS)
        {
            const string _FUNC_LOCAL_IP = "localip";

            string conn;
            switch (func)
            {
                case _FUNC_CONFIG:
                    conn = SearchXmlConnectionString(parameters);
                    if (conn != null)
                        return new VAL(conn);
                    else
                        return new VAL();

                case _FUNC_CFG:
                    conn = SearchTieConnectionString(parameters, DS);
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
                        cerr.WriteLine($"function {_FUNC_LOCAL_IP} requires 1 or zero parameter");
                        return new VAL();
                    }

                    if (parameters.Size == 1 && parameters[0].VALTYPE != VALTYPE.intcon)
                    {
                        cerr.WriteLine($"function {_FUNC_LOCAL_IP} requires integer parameter");
                        return new VAL();
                    }

                    int index = 0;
                    if (parameters.Size == 1)
                        index = (int)parameters[0];

                    string address = LocalHost.GetLocalIP(index);
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
                    Console.WriteLine("configuration file format error in {0}, {1}", cfgFile, ex.Message);
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

        public string DefaultServerPath
        {
            get
            {
                string path = GetValue<string>(Configuration._SERVER0);
                var provider = GetProvider(path);
                path = string.Format("{0}\\{1}", provider.ServerName, provider.DefaultDatabaseName.Name);
                return path;
            }
        }

        private static string PeelOleDb(string connectionString)
        {
            if (connectionString.ToLower().IndexOf("sqloledb") >= 0)
            {
                var x1 = new OleDbConnectionStringBuilder(connectionString);
                var x2 = new SqlConnectionStringBuilder();
                x2.DataSource = x1.DataSource;
                x2.InitialCatalog = (string)x1["Initial Catalog"];
                x2.UserID = (string)x1["User Id"];
                x2.Password = (string)x1["Password"];
                return x2.ConnectionString;
            }

            return connectionString;
        }

        private List<ConnectionProvider> providers = null;
        public List<ConnectionProvider> Providers
        {
            get
            {
                if (providers == null)
                    providers = GetConnectionProviders();

                return providers;
            }
        }

        private List<ConnectionProvider> GetConnectionProviders()
        {
            List<ConnectionProvider> pvds = new List<ConnectionProvider>();

            var machines = Cfg.GetValue(_SERVERS);
            if (machines.Undefined)
                return pvds;

            foreach (var pair in machines)
            {
                if (pair[0].IsNull || pair[1].IsNull)
                {
                    string text = pair[0].ToSimpleString();
                    cerr.WriteLine($"warning: undefined connection string at servers.{text}");
                    continue;
                }

                string serverName = pair[0].Str;
                string connectionString = PeelOleDb(pair[1].Str);
                try
                {
                    ConnectionProvider provider = ConnectionProviderManager.Register(serverName, connectionString);
                    pvds.Add(provider);
                }
                catch (Exception ex)
                {
                    cerr.WriteLine(ex.Message);
                }
            }

            return pvds;
        }


        public List<ServerName> ServerNames
        {
            get
            {
                var names = Providers.Select(pvd => pvd.ServerName)
                    .Distinct()
                    .ToList();

                return names;
            }
        }

        public ConnectionProvider GetProvider(string path)
        {
            string[] x = path.Split('\\');
            if (x.Length != 3)
            {
                cerr.WriteLine($"invalid server path: {path}, correct format is server\\database");
                return null;
            }

            return GetProvider(x[1], x[2]);
        }


        private ConnectionProvider GetProvider(string serverName, string databaseName)
        {
            var provider = Providers.Find(x => x.Name == serverName);
            if (provider != null)
            {
                if (databaseName == "~")
                    databaseName = provider.DefaultDatabaseName.Name;

                return ConnectionProviderManager.CloneConnectionProvider(provider, serverName, databaseName);
            }
            else
            {
                cerr.WriteLine($"invalid server path: \\{serverName}\\{databaseName}");
                return null;
            }
        }

        public bool Initialize(string cfgFile)
        {

            string theDirectory = Path.GetDirectoryName(System.Reflection.Assembly.GetExecutingAssembly().Location);
            string sysCfgFile = Path.Combine(theDirectory, _FILE_SYSTEM_CONFIG);

            if (!File.Exists(sysCfgFile))
            {
                Console.WriteLine("configuration file {0} not found", sysCfgFile);
                return false;
            }

            if (!TryReadCfg(sysCfgFile))
                return false;

            //user.cfg is optional
            if (!string.IsNullOrEmpty(cfgFile) && File.Exists(cfgFile))
            {
                this.CfgFile = cfgFile;
                TryReadCfg(cfgFile);
            }

            if (Cfg.GetValue(_DICTIONARY_TABLES).Defined)
            {
                var d = Cfg.GetValue(_DICTIONARY_TABLES);
                foreach (var t in d)
                {
                    dictionarytables.Add(new KeyValueTable { TableName = (string)t["table"], KeyName = (string)t["key"], ValueName = (string)t["value"] });
                }
            }


            this.OutputFile = Cfg.GetValue<string>(_FILE_OUTPUT, "script.sql");
            this.XmlDbDirectory = Cfg.GetValue<string>(_XML_DB_FOLDER, "db");
            this.WorkingDirectory.SetCurrentDirectory(Cfg.GetValue<string>(_WORKING_DIRECTORY, "."));

            var limit = Cfg[_LIMIT];
            if (limit["top"].Defined)
                this.Limit_Top = (int)limit["top"];

            if (limit["export_max_count"].Defined)
                this.Export_Max_Count = (int)limit["export_max_count"];


            var log = Cfg[_FILE_LOG];
            if (log.Defined) Context.DS.Add(_FILE_LOG, log);

            var editor = Cfg.GetValue<string>(_FILE_EDITOR, "notepad.exe");
            Context.DS.Add(_FILE_EDITOR, new VAL(editor));


            var x = Cfg[_QUEREY];
            if (x.Defined)
            {
                foreach (var pair in x)
                    Context.DS.Add((string)pair[0], pair[1]);
            }

            return true;

        }

        private static string SearchXmlConnectionString(VAL val)
        {
            if (val.Size != 3)
            {
                Console.WriteLine("required 2 parameters on function config(file,path,value), 1: app.config/web.config name; 2: path to reach connection string; 3:connection string attribute");
                return null;
            }

            if (val[0].VALTYPE != VALTYPE.stringcon || val[1].VALTYPE != VALTYPE.stringcon || val[2].VALTYPE != VALTYPE.stringcon)
            {
                Console.WriteLine("error on function config(file,path,value) argument type, 1: string, 2: string, 3:string");
                return null;
            }

            string xmlFile = (string)val[0];
            string path = (string)val[1];
            string value = (string)val[2];

            try
            {
                return SearchConnectionString(xmlFile, path, value);
            }
            catch (Exception)
            {
                Console.WriteLine("cannot find connection string on {0}, path={1}", xmlFile, path);
                return null;
            }
        }


        /// <summary>
        /// search *.config file
        /// </summary>
        /// <param name="xmlFile"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        private static string SearchConnectionString(string xmlFile, string path, string valueAttr)
        {
            if (!File.Exists(xmlFile))
            {
                Console.WriteLine("warning: not found {0}", xmlFile);
                return null;
            }

            string[] segments = path.Split('|');
            XElement X = XElement.Load(xmlFile);
            for (int i = 0; i < segments.Length - 1; i++)
            {
                X = X.Element(segments[i]);
            }

            string attr = segments.Last();
            string[] pair = attr.Split('=');
            var connectionString = X.Elements()
                .Where(x => x.Attribute(pair[0]).Value == pair[1])
                .Select(x => x.Attribute(valueAttr).Value)
                .FirstOrDefault();

            return cleanConnectionString(connectionString);
        }

        private static string cleanConnectionString(string connectionString)
        {
            string[] L = connectionString.Split(';');
            for (int i = 0; i < L.Length; i++)
            {
                if (L[i].ToUpper() == "Provider=sqloledb".ToUpper())
                {
                    connectionString = connectionString.Replace(L[i] + ";", "");
                    break;
                }
            }

            return connectionString;
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
                Console.WriteLine("required 1 parameters on function include(file), file can be local disk file, hyperlink, and ftp link");
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
                Console.WriteLine("configuration file {0} doesn't exist, {1}", link, ex.Message);
                return;
            }

            if (!exists)
            {
                Console.WriteLine("configuration file {0} doesn't exist", link);
                return;
            }

            string code = null;
            try
            {
                code = link.ReadAllText();
            }
            catch (Exception ex)
            {
                Console.WriteLine("failed to load configuration file {0}, {1}", link, ex.Message);
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
                Console.WriteLine("configuration file format error in {0}, {1}", link, ex.Message);
            }
        }

        private static string SearchTieConnectionString(VAL val, Memory DS)
        {
            if (val.Size != 2)
            {
                Console.WriteLine("required 2 parameters on function cfg(file,variable), 1: app.cfg/web.cfg name; 2: variable to reach connection string");
                return null;
            }

            if (val[0].VALTYPE != VALTYPE.stringcon || val[1].VALTYPE != VALTYPE.stringcon)
            {
                Console.WriteLine("error on function cfg(file,variable) argument type, 1: string, 2: string");
                return null;
            }

            string cfgFile = (string)val[0];
            string variable = (string)val[1];

            try
            {
                Memory localDS = new Memory();
                if (File.Exists(cfgFile))
                {
                    using (var reader = new StreamReader(cfgFile))
                    {
                        string code = reader.ReadToEnd();
                        try
                        {
                            Script.Execute(code, localDS);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("configuration file format error in {0}, {1}", cfgFile, ex.Message);
                            return null;
                        }
                    }
                }
                else
                {
                    Console.WriteLine("cannot find configuration file: {0}", cfgFile);
                    return null;
                }

                VAL value = localDS.GetValue(variable);
                if (value.Undefined)
                {
                    Console.WriteLine("undefined variable {0}", variable);
                    return null;
                }
                else if (!(value.Value is string))
                {
                    Console.WriteLine("connection string must be string, {0}={1}", variable, value.ToString());
                    return null;
                }
                else
                    return cleanConnectionString((string)value);
            }
            catch (Exception)
            {
                Console.WriteLine("cannot find connection string on {0}, variable={1}", cfgFile, variable);
                return null;
            }
        }

    }
}

