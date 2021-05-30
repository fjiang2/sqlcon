using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Tie;

namespace Sys.Stdio
{
    public class Command : ICommand
    {
        public bool badcommand { get; private set; }
        protected List<string> paths = new List<string>();

        /// <summary>
        /// Action is the command in command line
        /// e.g. command> cd ..
        /// Action is cd
        /// </summary>
        public string Action { get; private set; }

        /// <summary>
        /// 
        /// </summary>
        public string args { get; private set; }
        public string arg1 { get; private set; }
        public string arg2 { get; private set; }
        public string[] Arguments { get; private set; }

        public Options Options { get; } = new Options();

        //private string line;

        public Command()
        {
        }

        public Command(string line)
        {
            ParseLine(line);
        }

        protected void ParseLine(string line)
        {
            if (string.IsNullOrEmpty(line))
                return;

            string _line;

            if (!eval(line, out _line))
            {
                badcommand = true;
                return;
            }


            int k = parseAction(_line, out string action);
            this.Action = action;
            this.args = _line.Substring(k);

            this.badcommand = !parseArgument(this.args, out string[] L);
            this.Arguments = L;

            if (L.Length > 0 && !L[0].StartsWith("/"))
                this.arg1 = L[0];

            if (L.Length > 1 && !L[1].StartsWith("/"))
                this.arg2 = L[1];

            for (int i = 0; i < L.Length; i++)
            {
                string a = L[i];
                if (a.StartsWith("/"))
                {
                    if (!CustomerizeOption(a))
                        Options.Add(a);

                }
                else if (a.StartsWith("+") || a.StartsWith("-"))
                {
                    Options.Add(a);
                }
                else
                {
                    paths.Add(a);
                }
            }
        }

        protected virtual bool CustomerizeOption(string option)
        {
            return false;
        }

        public bool Has(string name)
        {
            return Options.HasOnly('/', name);
        }

        public string GetValue(string name)
        {
            return Options.GetValue('/', name);
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
                        cerr.WriteLine("Unclosed quotation mark after the character string \"");
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
                        cerr.WriteLine("Unclosed expression character }");
                        result = string.Empty;
                        return false;
                    }

                    string code = new string(expr, 0, index);
                    if (IsFormatString(code))
                    {
                        buf[i++] = '{';
                        foreach (char ch in code) buf[i++] = ch;
                        buf[i++] = '}';
                    }
                    else
                    {
                        string text = string.Empty;
                        try
                        {
                            VAL val = Script.Evaluate(code, Context.DS);
                            text = val.ToSimpleString();
                        }
                        catch (Exception ex)
                        {
                            cerr.WriteLine($"error in {code}, {ex.Message}");
                        }

                        foreach (char ch in text) buf[i++] = ch;
                    }
                }
                else if (args[k] == '}')
                {
                    cerr.WriteLine("Unclosed expression character }");
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

        private static bool IsFormatString(string format)
        {
            foreach (char ch in format)
            {
                if (!char.IsNumber(ch) && ch != ':' && ch != ',')
                    return false;
            }

            return true;
        }

        public int GetInt32(string name, int defaultValue)
        {
            var obj = GetInt32(name);
            if (obj != null)
                return (int)obj;
            else
                return defaultValue;
        }

        public int? GetInt32(string name)
        {
            string value = GetValue(name);
            if (value == null)
                return null;

            if (int.TryParse(value, out int a))
                return a;

            throw new InvalidCastException($"invalid integer {name}={value}");
        }

        public double GetDouble(string name, double defaultValue)
        {
            var obj = GetDouble(name);
            if (obj != null)
                return (double)obj;
            else
                return defaultValue;
        }

        public double? GetDouble(string name)
        {
            string value = GetValue(name);
            if (value == null)
                return null;

            if (double.TryParse(value, out double a))
                return a;

            throw new InvalidCastException($"invalid double {name}={value}");
        }

        public bool GetBoolean(string name, bool defaultValue)
        {
            var obj = GetBoolean(name);
            if (obj != null)
                return (bool)obj;
            else
                return defaultValue;
        }

        public bool? GetBoolean(string name)
        {
            string value = GetValue(name);
            if (value == null)
                return null;

            if (bool.TryParse(value, out bool a))
                return a;

            throw new InvalidCastException($"invalid boolean {name}={value}");
        }

        public T GetEnum<T>(string name, T defaultValue) where T : struct
        {
            string value = GetValue(name);
            if (value == null)
                return defaultValue;

            if (int.TryParse(value, out int d))
            {
                return (T)Enum.ToObject(typeof(T), d);
            }
            else if (Enum.TryParse<T>(value, true, out T e))
            {
                return e;
            }

            return defaultValue;
        }

    }
}
