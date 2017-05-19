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
        private object value;
        public string userValue { get; set; }

        public Field(TypeInfo type, string fieldName)
            : this(type, fieldName, null)
        {
        }


        public Field(TypeInfo type, string fieldName, object value)
            : base(fieldName)
        {
            this.type = type;
            this.value = value;
        }



        protected override CodeBlock BuildBlock()
        {
            CodeBlock block = base.BuildBlock();

            if (userValue != null)
            {
                block.AppendFormat("{0} = {1};", Signature, userValue);
            }
            else if (value != null)
            {
                if (value is string)
                    value = $"\"{value}\"";

                if (value is Array)
                {
                    block.AppendFormat("{0} = new {1}", Signature, type);
                    WriteArrayValue(block, value as Array, 10);
                }
                else if (value is Dictionary<string, string>)
                {
                    block.AppendFormat("{0} = new {1}", Signature, type);
                    WriteDictionaryValue(block, value as Dictionary<string, string>, 10);
                }
                else
                    block.AppendFormat("{0} = {1};", Signature, value);
            }
            else
            {
                block.AppendLine(Signature + ";");
            }

            return block;
        }


        private void WriteArrayValue(CodeBlock block, Array A, int columnNumber)
        {
            if (A.Length <= columnNumber)
            {
                block
                    .Append("{")
                    .Append(string.Join(",", A))
                    .Append("};");
                return;
            }

            block.Begin();
            for (int i = 0; i < A.Length; i++)
            {
                if (i != 0 && i % columnNumber == 0)
                {
                    block.AppendLine();
                }

                string text = A.GetValue(i).ToString();
                if (text.Length > 100)
                    block.AppendLine(text);
                else
                {
                    if (i == 0)
                        block.AppendLine();

                    block.Append(text);
                }

                if (i != A.Length - 1)
                    block.Append(",");

            }

            block.End(";");
        }

        private void WriteDictionaryValue(CodeBlock block, Dictionary<string, string> A, int columnNumber)
        {
            block.Begin();
            foreach (var kvp in A)
            {
                block.AppendFormat("[{0}] = {1},", kvp.Key, kvp.Value);
            }
            block.End(";");
        }
    }
}
