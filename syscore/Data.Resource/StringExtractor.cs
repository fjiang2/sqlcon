using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using Tie;

namespace Sys.Data.Resource
{
    public class StringExtractor
    {
        public List<string> includes = new List<string>
        {
            ".cs"
        };

        public List<string> excludes = new List<string>
        {
            "\\bin\\",
            "\\obj\\",
            "DS.Designer.cs",
        };

        public string[] fonts = new string[]
        {
            "Microsoft Sans Serif",
        };


        private Dictionary<string, Token> stringTokens = new Dictionary<string, Token>();

        private StringDumper dumper;

        public StringExtractor(StringDumper dumper)
        {
            this.dumper = dumper;
        }

        public void Extract(string root)
        {
            var files = Directory.GetFiles(root, "*.cs", SearchOption.AllDirectories);

            foreach (string file in files)
            {
                if (!MatchFileName(file))
                {
                    //Console.WriteLine($"skip: {file}");
                    continue;
                }

                if (file.EndsWith("AssemblyInfo.cs"))
                    continue;

                Console.WriteLine(file);
                ProcessFile(file);
            }
        }

        public void Extract(string[] files)
        {
            foreach (string file in files)
            {
                Console.WriteLine(file);
                ProcessFile(file);
            }
        }


        private bool MatchFileName(string path)
        {
            foreach (string a in excludes)
            {
                if (path.IndexOf(a) > 0)
                    return false;
            }

            foreach (string a in includes)
            {
                if (path.IndexOf(a) > 0)
                    return true;
            }

            return false;
        }

        private List<Token> ProcessFile(string path)
        {
            string code = File.ReadAllText(path);
            var L = Script.Tokenize(code).ToArray();

            List<Token> L2 = new List<Token>();
            token prev = new token();
            for (int i = 0; i < L.Length; i++)
            {
                if (i > 0)
                    prev = L[i - 1];

                token current = L[i];
                if (current.ty == tokty.stringcon)
                {
                    if (IsGoodString(current.tok))
                    {
                        Token tok = new Token
                        {
                            name = current.tok,
                            value = current.tok,
                            line = 0 // current.line,
                        };

                        tok.name = ToIdentifier(current.tok);

                        if (i > 0 && prev.tok == "$")
                            tok.type = "$";

                        L2.Add(tok);
                    }
                }
            }

            if (L2.Count == 0)
                return L2;

            Output(path, L2);

            return L2;
        }

        private void Output(string cs, List<Token> L2)
        {
            foreach (var line in L2)
            {
                if (line.value.Any(x => char.IsLetter(x)))
                {
                    if (!stringTokens.ContainsKey(line.name))
                        stringTokens.Add(line.name, line);

                    if (line.type != null)
                        dumper.Add(cs, line.line, "$", line.name, line.value);
                    else
                        dumper.Add(cs, line.line, "", line.name, line.value);
                }
            }
        }

        private bool IsGoodString(string text)
        {
            if (fonts.Contains(text))
                return false;

            if (string.IsNullOrWhiteSpace(text))
                return false;

            if (text.Length == 1)
                return false;

            var L = text.Split('|');
            if (L.Length == 2 && L[1].StartsWith("System."))
                return false;

            return true;
        }

        public static string ToIdentifier(string s)
        {
            s = s.Trim();
            int hash = s.GetHashCode();

            string name = ident.Identifier(s).ToUpper();
            if (name.Length <= 20)
                return name;

            name = name.Substring(0, 20);

            if (hash >= 0)
                return $"{name}{hash}";
            else
                return $"{name}_{-hash}";
        }

        class Token
        {
            public string name { get; set; }
            public string value { get; set; }
            public string type { get; set; }
            public int line { get; set; }
        }
    }

}
