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
    public class Property : Declare, ICodeBlock
    {
        private object value;

        public Statement Gets { get; } = new Statement();
        public Statement Sets { get; } = new Statement();

        public Modifier GetModifier { get; set; } = Modifier.Public;
        public Modifier SetModifier { get; set; } = Modifier.Public;

        public bool IsLambda { get; set; }

        public Property(TypeInfo returnType, string propertyName)
            : this(returnType, propertyName, null)
        {
        }

        public Property(TypeInfo returnType, string propertyName, object value)
            : base(propertyName)
        {
            this.Type = returnType;
            this.value = value;
        }

        public bool CanRead
        {
            get
            {
                return (Gets.Count == 0 && Sets.Count == 0) || Gets.Count > 0;
            }
        }
        public bool CanWrite
        {
            get
            {
                return (Gets.Count == 0 && Sets.Count == 0) || Sets.Count > 0;
            }
        }

        private string get
        {
            get
            {
                if (GetModifier == Modifier.Private)
                    return "private get";
                else if (GetModifier == Modifier.Protected)
                    return "protected get";
                else
                    return "get";
            }
        }

        private string set
        {
            get
            {
                if (SetModifier == Modifier.Private)
                    return "private set";
                else if (SetModifier == Modifier.Protected)
                    return "protected set";
                else
                    return "set";
            }
        }

        public string Expression { get; set; }

        protected override void BuildBlock(CodeBlock block)
        {
            base.BuildBlock(block);

            if (Comment?.Alignment == Alignment.Top)
            {
                block.AppendFormat(Comment.ToString());
                Comment.Clear();
            }

            if (Expression != null)
            {
                block.AppendFormat("{0}{1}", $"{Signature} => {Expression};", Comment);
            }
            else if (Gets.Count == 0 && Sets.Count == 0)
            {
                if (value != null)
                    block.AppendFormat("{0}{1}", $"{Signature} {{ {get}; {set}; }} = {value};", Comment);
                else
                    block.AppendFormat("{0}{1}", $"{Signature} {{ {get}; {set}; }}", Comment);
            }
            else if (!IsLambda)
            {
                block.AppendLine(Signature + Comment);
                block.Begin();
                if (Gets.Count != 0)
                {
                    block.AppendLine(get);
                    block.AddWithBeginEnd(Gets);
                }

                if (Sets.Count != 0)
                {
                    block.AppendLine(set);
                    block.AddWithBeginEnd(Sets);
                }

                block.End();
            }
            else
            {
                block.AppendLine(Signature + Comment);
                if (Gets.Count != 0)
                    Lambda(block, get, Gets);

                if (Sets.Count != 0)
                    Lambda(block, set, Sets);
            }

            return;
        }

        private void Lambda(CodeBlock block, string opr, Statement statement)
        {
            if (opr.EndsWith("get") && statement.Count == 1)
            {
                block.Append($" => {statement}");
                return;
            }

            block.Append($"{opr} => ").AddWithBeginEnd(statement);
        }
    }
}
