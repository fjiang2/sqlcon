using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace sqlcon
{
    class Command
    {
        public readonly bool badcommand = false;
        private List<string> paths = new List<string>();

        public string Action;
        public string args { get; private set; }
        public string arg1 { get; private set; }
        public string arg2 { get; private set; }

        public readonly bool Refresh;

        public readonly bool IsVertical;
        public readonly bool IsSchema;

        public readonly bool HasHelp;
        public readonly bool HasIfExists;
        public readonly bool HasPage;
        public readonly bool HasRowId;

        public readonly bool HasDefinition;
        public readonly bool HasPrimaryKey;
        public readonly bool HasForeignKey;
        public readonly bool HasIdentityKey;
        public readonly bool HasDependency;
        public readonly bool HasIndex;
        public readonly bool HasStorage;
        public readonly bool ToJson;
        public readonly bool ToCSharp;

        public readonly int top;
       // public readonly Options optionPlus = new Options();
       // public readonly Options optionMinus = new Options();

        public readonly Options options = new Options();
        private readonly string columns;

        public Command(string line, Configuration cfg)
        {
            this.HasDefinition = false;
            this.IsVertical = false;
            this.top = cfg.Limit_Top;

            if (string.IsNullOrEmpty(line))
                return;


            int k = parseAction(line, out Action);
            this.args = line.Substring(k);

            string[] L;
            this.badcommand = !parseArgument(this.args, out L);

            if (L.Length > 0 && !L[0].StartsWith("/"))
                this.arg1 = L[0];

            if (L.Length > 1 && !L[1].StartsWith("/"))
                this.arg2 = L[1];

            for (int i = 0; i < L.Length; i++)
            {
                string a = L[i];
                if (a.StartsWith("/"))
                {
                    switch (a)
                    {
                        case "/def":
                            HasDefinition = true;
                            break;

                        case "/s":
                            IsSchema = true;
                            break;

                        case "/t":
                            IsVertical = true;
                            break;

                        case "/if":
                            HasIfExists = true;
                            break;

                        case "/p":
                            HasPage = true;
                            break;

                        case "/all":
                            top = 0;
                            break;

                        case "/r":
                            HasRowId = true;
                            break;

                        case "/refresh":
                            Refresh = true;
                            break;

                        case "/?":
                            HasHelp = true;
                            break;

                        case "/pk":
                            HasPrimaryKey = true;
                            break;

                        case "/fk":
                            HasForeignKey = true;
                            break;

                        case "/ik":
                            HasIdentityKey = true;
                            break;

                        case "/dep":
                            HasDependency = true;
                            break;

                        case "/ind":
                            HasIndex = true;
                            break;

                        case "/sto":
                            HasStorage = true;
                            break;

                        case "/json":
                            ToJson = true;
                            break;

                        case "/c#":
                            ToCSharp = true;
                            break;

                        default:
                            if (a.StartsWith("/top:"))
                                int.TryParse(a.Substring(5), out top);
                            else if (a.StartsWith("/col:"))
                                columns = a.Substring(5);
                            else
                                options.Add(a);

                            break;
                    }
                }
                else if (a.StartsWith("+") || a.StartsWith("-"))
                {
                    options.Add(a);
                }
                else
                {
                    if (paths.Count > 1)
                        this.badcommand = true;

                    paths.Add(a);
                }
            }
        }


        public bool Has(string name)
        {
            return options.Has('/', name);
        }

        public string GetValue(string name)
        {
            return options.GetValue('/', name);
        }

        public PathName Path1
        {
            get
            {
                if (this.paths.Count > 0)
                    return new PathName(paths[0]);
                else
                    return null;
            }
        }

        public PathName Path2
        {
            get
            {
                if (this.paths.Count > 1)
                    return new PathName(paths[1]);
                else
                    return null;
            }
        }

        public string wildcard
        {
            get
            {
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

        private static int parseAction(string line, out string action)
        {
            int k = 0;
            char[] buf = new char[200];
            while (k < line.Length)
            {
                if (line[k] == ' ' || line[k] == '.' || line[k] == '~' || line[k] == '\\' || line[k] == '"')
                {
                    break;
                }

                buf[k] = line[k];
                k++;
            }

            action = new string(buf, 0, k).ToLower();
            while (k < line.Length && line[k] == ' ')
                k++;
            return k;
        }

        private static bool parseArgument(string args, out string[] result)
        {
            if (args.Length == 0)
            {
                result = new string[] { };
                return true;
            }

            List<string> L = new List<string>();

            char[] buf = new char[5000];
            int k = 0;
            int i = 0;

            while (k < args.Length)
            {
                if (args[k] == ' ')
                {
                    if (i > 0)
                    {
                        L.Add(new string(buf, 0, i));
                        i = 0;
                    }
                }
                else if (args[k] == '"')
                {
                    k++;
                    while (k < args.Length && args[k] != '"')
                    {
                        buf[i++] = args[k];
                        k++;
                    }

                    if (k == args.Length)
                    {
                        stdio.ErrorFormat("Unclosed quotation mark after the character string \"");
                        result = new string[] { };
                        return false;
                    }

                    L.Add(new string(buf, 0, i));
                    i = 0;
                }
                else
                    buf[i++] = args[k];

                k++;
            }

            if (i > 0)
                L.Add(new string(buf, 0, i));

            result = L.ToArray();
            return true;
        }

     
    }
}
