using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using Sys.Stdio;

namespace sqlcon
{
    class ApplicationCommand : Command
    {

        public bool Refresh { get; private set; }
        public bool IsVertical { get; private set; }
        public bool IsSchema { get; private set; }
        public bool HasHelp { get; private set; }
        public bool HasIfExists { get; private set; }
        public bool HasPage { get; private set; }
        public bool HasDefinition { get; private set; }
        public bool HasPrimaryKey { get; private set; }
        public bool HasForeignKey { get; private set; }
        public bool HasIdentityKey { get; private set; }
        public bool HasDependency { get; private set; }
        public bool HasIndex { get; private set; }
        public bool HasStorage { get; private set; }
        public bool Append { get; private set; }
        public int Top { get; private set; }


        private bool hasRowId;
        private string columns;
        private Configuration cfg;

        public ApplicationCommand(Configuration cfg, string line)
        {
            this.cfg = cfg;
            this.Top = cfg.Limit_Top;

            ParseLine(line);
        }

        protected override bool CustomerizeOption(string a)
        {
            switch (a)
            {
                case "/def":
                    HasDefinition = true;
                    return true;

                case "/s":
                    IsSchema = true;
                    return true;

                case "/t":
                    IsVertical = true;
                    return true;

                case "/if":
                    HasIfExists = true;
                    return true;

                case "/p":
                    HasPage = true;
                    return true;

                case "/all":
                    Top = 0;
                    return true;

                case "/r":
                    hasRowId = true;
                    return true;

                case "/refresh":
                    Refresh = true;
                    return true;

                case "/?":
                    HasHelp = true;
                    return true;

                case "/pk":
                    HasPrimaryKey = true;
                    return true;

                case "/fk":
                    HasForeignKey = true;
                    return true;

                case "/ik":
                    HasIdentityKey = true;
                    return true;

                case "/dep":
                    HasDependency = true;
                    return true;

                case "/ind":
                    HasIndex = true;
                    return true;

                case "/sto":
                    HasStorage = true;
                    return true;

                default:
                    if (a.StartsWith("/top:"))
                    {
                        if (int.TryParse(a.Substring(5), out var _top))
                            Top = _top;

                        return true;
                    }
                    else if (a.StartsWith("/col:"))
                    {
                        columns = a.Substring(5);
                        return true;
                    }

                    return false;
            }
        }

        public Configuration Configuration => this.cfg;

        public bool HasRowId => hasRowId || Has("edit");


        public string wildcard
        {
            get
            {
                foreach (var path in paths)
                {
                    var pathName = new PathName(path);

                    if (pathName.wildcard != null)
                        return pathName.wildcard;
                }

                if (Path1 == null)
                    return null;
                else
                    return Path1.wildcard;
            }
        }

        public string where
        {
            get
            {
                foreach (var path in paths)
                {
                    var pathName = new PathName(path);

                    if (pathName.where != null)
                        return pathName.where;
                }

                if (Path1 == null)
                    return null;
                else
                    return Path1.where;
            }
        }


        public string[] Columns
        {
            get
            {
                if (this.columns == null)
                    return new string[] { };
                else
                    return this.columns.Split(',');
            }
        }

        public string[] Includes
        {
            get
            {
                string include = GetValue("include");
                try
                {
                    if (include != null)
                        return include.Split(',');
                    else
                        return new string[] { };
                }
                catch (Exception ex)
                {
                    throw new Exception($"invalid arugment /include:{include}, {ex.Message}");
                }
            }
        }

        public string[] Excludes
        {
            get
            {
                string include = GetValue("exclude");
                try
                {
                    if (include != null)
                        return include.Split(',');
                    else
                        return new string[] { };
                }
                catch (Exception ex)
                {
                    throw new Exception($"invalid arugment /exclude:{include}, {ex.Message}");
                }
            }
        }

        public string OutputDirectory
        {
            get
            {
                string path = OutputPath;
                if (path == null)
                    return null;

                if (Directory.Exists(path))
                    return path;

                string directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                return directory;
            }
        }

        public string OutputFileName
        {
            get
            {
                string path = OutputPath;
                if (path == null)
                    return null;

                if (File.Exists(path))
                    return path;

                if (Directory.Exists(path))
                    return null;

                string directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                return path;
            }
        }

        public string OutputPath
        {
            get
            {
                string path = GetValue("out");
                if (path == null)
                    return null;

                Append = Has("append");
                return path;
            }
        }

        public string InputPath
        {
            get
            {
                string path = GetValue("in");
                if (path == null)
                    return null;

                return path;
            }
        }
    }
}
