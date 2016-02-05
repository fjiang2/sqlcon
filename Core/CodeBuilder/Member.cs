using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Member : Buildable
    {
        private CodeBlock comments = new CodeBlock();

        public Member()
        {
        }

        public Member Add(string comment)
        {
            comments.AppendLine(comment);
            return this;
        }

        protected override CodeBlock BuildBlock()
        {
            CodeBlock block = base.BuildBlock();
            block.Add(comments);
            return block;
        }

    }
}
