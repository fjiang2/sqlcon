using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tie;

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
        public string[] arguments { get; private set; }
        public readonly bool Refresh;

        public readonly bool IsVertical;
        public readonly bool IsSchema;

        public readonly bool HasHelp;
        public readonly bool HasIfExists;
        public readonly bool HasPage;
        private bool hasRowId;

        public readonly bool HasDefinition;
        public readonly bool HasPrimaryKey;
        public readonly bool HasForeignKey;
        public readonly bool HasIdentityKey;
        public readonly bool HasDependency;
        public readonly bool HasIndex;
        public readonly bool HasStorage;
        public readonly bool ToCSharp;

        public readonly int top;
        // public readonly Options optionPlus = new Options();
        // public readonly Options optionMinus = new Options();

        public readonly Options options = new Options();
        private readonly string columns;
        private Configuration cfg;

        public Command(string line, Configuration cfg)
        {
            this.cfg = cfg;

            this.HasDefinition = false;
            this.IsVertical = false;
            this.top = cfg.Limit_Top;

            if (string.IsNullOrEmpty(line))
                return;

            if (!eval(line, out string _line))
            {
                badcommand = true;
                return;
            }

            int k = parseAction(_line, out Action);
            this.args = _line.Substring(k);

            string[] L;
            this.badcommand = !parseArgument(this.args, out L);
            this.arguments = L;

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
                            hasRowId = true;
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
                    paths.Add(a);
                }
            }
        }

        public Configuration Configuration => this.cfg;

        public bool HasRowId => hasRowId || Has("edit");

        public bool Has(string name)
        {
            return options.HasOnly('/', name);
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

        private static int parseAction(string line, out string action)
        {
            int k = 0;
            char[] buf = new char[200];
            while (k < line.Length)
            {
                if (line[k] == ' ' || line[k] == '.' || line[k] == '~' || line[k] == '\\' || line[k] == '/' || line[k] == '"')
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
            int k = 0;  //index of args[]
            int i = 0;  //index of buf[]

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
                else if (args[k] == '"')    //quotation mark argument
                {
                    k++;
                    while (k < args.Length && args[k] != '"')
                    {
                        buf[i++] = args[k];
                        k++;
                    }

                    if (k == args.Length)
                    {
                        cout.ErrorFormat("Unclosed quotation mark after the character string \"");
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


        /// <summary>
        /// evaluate expression
        /// </summary>
        /// <param name="args"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private bool eval(string args, out string result)
        {
            int i = 0;
            int k = 0;
            char[] buf = new char[5000];
            while (k < args.Length)
            {
                if (k < args.Length - 1 && ((args[k] == '{' && args[k + 1] == '{') || (args[k] == '}' && args[k + 1] == '}')))
                {
                    buf[i++] = args[k++];
                }
                else if (args[k] == '{')
                {
                    k++;
                    int index = 0; //index of expr[]
                    char[] expr = new char[4000];
                    while (k < args.Length && args[k] != '}')
                    {
                        expr[index++] = args[k];
                        k++;
                    }

                    if (k == args.Length)
                    {
                        cout.ErrorFormat("Unclosed expression character }");
                        result = string.Empty;
                        return false;
                    }

                    string code = new string(expr, 0, index);
                    string text = string.Empty;
                    try
                    {
                        VAL val = Script.Evaluate(code, Context.DS);
                        text = val.ToSimpleString();
                    }
                    catch (Exception ex)
                    {
                        cout.Error($"error in {code}, {ex.Message}");
                    }
                    foreach (char ch in text)
                    {
                        buf[i++] = ch;
                    }
                }
                else if (args[k] == '}')
                {
                    cout.ErrorFormat("Unclosed expression character }");
                    result = string.Empty;
                    return false;
                }
                else
                    buf[i++] = args[k];

                k++;
            }

            result = new string(buf, 0, i);
            return true;
        }

    }
}
