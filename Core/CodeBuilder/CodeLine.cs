using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public interface ICodeBlock
    {
        CodeBlock GetBlock();
    }

    class CodeLine
    {
        public static readonly CodeLine EmptyLine = new CodeLine();

        private static readonly string[] TABS = new string[] { "", "\t", "\t\t", "\t\t\t", "\t\t\t\t", "\t\t\t\t\t" };

        internal static string Tab(int n)
        {
            if (n < TABS.Length)
                return TABS[n];

            return new string('\t', n);
        }


        public int tab { get; set; }
        public string line { get; set; }


        public override string ToString()
        {
            var t = Tab(tab);
            return $"{t}{line}";
        }
    }
}
