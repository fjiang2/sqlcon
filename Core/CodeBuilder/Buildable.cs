using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Buildable : ICodeBlock
    {
        private CodeBlock block = null;

        public CodeBlock GetBlock()
        {
            if (block == null)
                block = BuildBlock();

            return block;
        }

        public int Count
        {
            get
            {
                return GetBlock().Count;
            }
        }
        /// <summary>
        /// Generate code, BuildBlock can be invoked only once
        /// </summary>
        /// <returns></returns>
        protected virtual CodeBlock BuildBlock()
        {
            CodeBlock block = new CodeBlock();

            return block;
        }

        public override string ToString()
        {
            return GetBlock().ToString();
        }

    }
}
