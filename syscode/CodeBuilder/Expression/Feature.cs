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
    public class Feature : Declare, IBuildable
    {
        public int? Value { get; set; }

        public Feature(string feature)
            : base(feature)
        {

        }


        protected override void BuildBlock(CodeBlock block)
        {
            base.BuildBlock(block);

            if (Comment?.Alignment == Alignment.Top)
            {
                block.AppendFormat(Comment.ToString());
                Comment.Clear();
            }

            if (Value != null)
                block.AppendLine($"{Name} = {Value}, {Comment}");
            else
                block.AppendLine($"{Name}, {Comment}");
        }

    }
}
