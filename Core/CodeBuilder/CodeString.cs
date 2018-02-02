using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class CodeString
    {
        private string code;
        public CodeString(string code)
        {
            this.code = code;
        }

        public override string ToString()
        {
            if (string.IsNullOrEmpty(code))
                return "null";

            return code;
        }
    }
}
