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
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{

    public class Value : Buildable
    {
        public TypeInfo Type { get; set; } = TypeInfo.Anonymous;
        public ValueOutputFormat Format { get; set; } = ValueOutputFormat.MultipleLine;

        private object value;

        public Value(object value)
        {
            this.value = value;
        }

        private Value NewValue(object value)
        {
            if (value is Value)
            {
                Value val = (Value)value;
                return val;
            }
            else
                return new Value(value) { Format = Format };
        }

        protected override void BuildBlock(CodeBlock block)
        {
            base.BuildBlock(block);
        }

        internal void BuildCode(CodeBlock block)
        {
            if (value is Value)
            {
                (value as Value).BuildCode(block);
            }
            else if (value is New)
            {
                block.Add((value as New).GetBlock(), indent: 1);
            }
            else if (value is Array)                        // new Foo[] { new Foo {...}, new Foo {...}, ...}
            {
                var A = value as Array;
                if (Type == TypeInfo.Anonymous)
                    Type = new TypeInfo { Type = A.GetType() };

                block.Append($"new {Type}");
                WriteArrayValue(block, A, 10);
            }
            else if (value is Dictionary<object, object>)   // new Dictionary<T1,T2> { [t1] = new T2 {...}, ... }
            {
                block.Append($"new {Type}");
                WriteDictionary(block, value as Dictionary<object, object>);
            }
            else
                block.Append(Primitive.ToPrimitive(value));
        }

        private void WriteArrayValue(CodeBlock block, Array A, int columnNumber)
        {
            Type ty = Type.GetElementType();

            if (ty != null && ty.IsPrimitive)
            {
                if (A.Length < 30)
                {
                    Format = ValueOutputFormat.SingleLine;
                }
                else if (A.Length < 100)
                {
                    Format = ValueOutputFormat.Wrap;
                    columnNumber = 10;
                }
                else
                {
                    Format = ValueOutputFormat.Wrap;
                    columnNumber = 20;
                }
            }

            switch (Format)
            {

                case ValueOutputFormat.SingleLine:
                    block.Append("{");
                    A.OfType<object>().ForEach(
                         x =>
                         {
                             NewValue(x).BuildCode(block);
                         },
                         _ => block.Append(",")
                         );

                    block.Append("}");
                    break;



                case ValueOutputFormat.Wrap:
                    block.Begin();
                    for (int i = 0; i < A.Length; i++)
                    {
                        if (i % columnNumber == 0)
                            block.AppendLine();

                        if (i != 0 && i % (columnNumber * 10) == 0)     //add empty line every 10 lines
                            block.AppendLine();

                        Value item = NewValue(A.GetValue(i));
                        item.BuildCode(block);

                        if (i != A.Length - 1)
                            block.Append(",");
                    }

                    block.End();
                    break;

                default:
                    block.Begin();

                    for (int i = 0; i < A.Length; i++)
                    {
                        if (i != 0 && i % columnNumber == 0)
                        {
                            block.AppendLine();
                        }

                        block.AppendLine();
                        Value item = NewValue(A.GetValue(i));
                        item.BuildCode(block);

                        if (i != A.Length - 1)
                            block.Append(",");

                    }

                    block.End();
                    break;
            }
        }

        private void WriteDictionary(CodeBlock block, Dictionary<object, object> A)
        {

            switch (Format)
            {
                case ValueOutputFormat.SingleLine:
                    block.Append("{");
                    A.ForEach(
                         kvp =>
                         {
                             block.Append($"[{kvp.Key}] = ");
                             NewValue(kvp.Value).BuildCode(block);
                         },
                         _ => block.Append(",")
                         );

                    block.Append("}");
                    break;

                default:
                    block.Begin();

                    A.ForEach(
                        kvp =>
                            {
                                block.AppendLine();
                                block.Append($"[{kvp.Key}] = ");
                                NewValue(kvp.Value).BuildCode(block);
                            },
                        _ => block.Append(",")
                        );

                    block.End();
                    break;
            }
        }


        public override string ToString()
        {
            return Primitive.ToPrimitive(value);
        }

        public static implicit operator Value(New value)
        {
            return new Value(value);
        }

        //public static implicit operator Value(Expression value)
        //{
        //    return new Value(value);
        //}
    }

}
