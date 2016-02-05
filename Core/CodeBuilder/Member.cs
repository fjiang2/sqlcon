using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Member : Buildable
    {
        private CodeBlock code = new CodeBlock();

        public Member(string line)
        {
            code.AppendLine(line);
        }

        public Member Add(string line)
        {
            code.AppendLine(line);
            return this;
        }

        protected override CodeBlock BuildBlock()
        {
            CodeBlock block = base.BuildBlock();
            block.Add(code);
            return block;
        }

    }
}
