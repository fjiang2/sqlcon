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
    public class Field : Declare, IBuildable
    {
        private Value value;

        public string UserValue { get; set; }

        public Field(TypeInfo type, string fieldName)
            : this(type, fieldName, null)
        {
        }


        public Field(TypeInfo type, string fieldName, Value value)
            : base(fieldName)
        {
            this.Type = type;
            this.value = value;
        }



        protected override void BuildBlock(CodeBlock block)
        {
            base.BuildBlock(block);

            string _comment = string.Empty;
            if (Comment != null)
            {
                _comment = Comment.ToString();

                if (Comment.Alignment == Alignment.Top)
                {
                    block.Append(_comment);
                    _comment = string.Empty;
                }
            }

            if (UserValue != null)
            {
                block.AppendLine($"{Signature} = {UserValue};{_comment}");
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
