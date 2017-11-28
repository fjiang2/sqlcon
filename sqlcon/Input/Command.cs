using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tie;

namespace sqlcon
{
    class Command : BaseCommand
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
        public bool ToCSharp { get; private set; }
        public int Top { get; private set; }


        private bool hasRowId;
        private string columns;
        private Configuration cfg;

        public Command(string line, Configuration cfg)
            : base(line)
        {
            this.cfg = cfg;

            this.HasDefinition = false;
            this.IsVertical = false;
            this.Top = cfg.Limit_Top;
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

                case "/c#":
                    ToCSharp = true;
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



    }
}
