//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        DPO(Data Persistent Object)                                                               //
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

namespace Sys.CodeBuilder
{
    public class Field : Declare, ICodeBlock
    {
        private Value value;
        public string userValue { get; set; }

        public Field(TypeInfo type, string fieldName)
            : this(type, fieldName, null)
        {
        }


        public Field(TypeInfo type, string fieldName, Value value)
            : base(fieldName)
        {
            this.type = type;
            this.value = value;
        }



        protected override void BuildBlock(CodeBlock block)
        {
            base.BuildBlock(block);

            string _comment = string.Empty;
            if (comment != null)
            {
                _comment = comment.ToString();

                if (comment.Orientation == Orientation.Vertical)
                {
                    block.Append(_comment);
                    _comment = string.Empty;
                }
            }

            if (userValue != null)
            {
                block.AppendLine($"{Signature} = {userValue};{_comment}");
            }
            else if (value != null)
            {
                block.AppendLine();
                block.Append($"{Signature} = ");
                value.BuildCode(block);
                block.Append(";");
            }
            else
            {
                block.AppendLine($"{Signature};{_comment}");
            }

        }


    }
}
