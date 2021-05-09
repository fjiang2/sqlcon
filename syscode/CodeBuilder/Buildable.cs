//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        syscode(C# Code Builder)                                                                  //
//                                                                                                  //
//          Copyright(c) Datum Connect Inc.                                                         //
//                                                                                                  //
// This source code is subject to terms and conditions of the Datum Connect Software License. A     //
// copy of the license can be found in the License.html file at the root of this distribution. If   //
// you cannot locate the  Datum Connect Software License, please send an email to                   //
// datconn@gmail.com. By using this source code in any fashion, you are agreeing to be bound        //
// by the terms of the Datum Connect Software License.                                              //
//                                                                                                  //
// You must not remove this notice, or any other, from this software.                               //
//                                                                                                  //
//                                                                                                  //
//--------------------------------------------------------------------------------------------------//

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Buildable : IBuildable
    {
        private CodeBlock block = null;
        public Buildable Parent { get; set; }

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
        private CodeBlock BuildBlock()
        {
            CodeBlock block = new CodeBlock();
            BuildBlock(block);
            return block;
        }

        protected virtual void BuildBlock(CodeBlock block)
        {
        }

        public override string ToString()
        {
            return GetBlock().ToString();
        }

    }
}
