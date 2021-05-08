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

        private object value;
        private ValueOutputFormat format { get; set; } = ValueOutputFormat.MultipleLine;

        public Value(object value)
        {
            this.value = value;
        }


        public static Value NewPropertyObject(TypeInfo type)
        {
            return new Value(new Dictionary<string, Value>()) { Type = type };
        }

        public static string ToPrimitive(object value)
        {
            //make double value likes integer, e.g. ToPrimitive(25.0) returns "25, ToPrimitive(25.3) returns "25.3"
            if (value is double)
            {
                return value.ToString();
            }
            else if (value is Guid)
            {
                return $"new Guid(\"{value}\")";
            }
            else if (value is CodeString)
            {
                return value.ToString();
            }
            else if (value is byte[])
            {
                var hex = (value as byte[])
                    .Select(b => $"0x{b:X}")
                    .Aggregate((b1, b2) => $"{b1},{b2}");
                return "new byte[] {" + hex + "}";
                //return "new byte[] {0x" + BitConverter.ToString((byte[])value).Replace("-", ",0x") + "}";
            }

            return Extension.ToCodeString(value);
        }

        private Value NewValue(object value)
        {
            if (value is Value)
            {
                Value val = (Value)value;
                return val;
            }
            else
                return new Value(value) { format = format };
        }

        private Dictionary<string, Value> objectValue => value as Dictionary<string, Value>;

        public void AddProperty(string propertyName, Value value)
        {
            if (objectValue == null)
                throw new Exception("object property is initialized, use new Value()");

            if (objectValue.ContainsKey(propertyName))
                throw new Exception($"duplicated property name:{propertyName}");

            objectValue.Add(propertyName, value);
        }

        internal void BuildCode(CodeBlock block)
        {
            if (value is Value)
            {
                (value as Value).BuildCode(block);
            }
            else if (value is Array)                        // new Foo[] { new Foo {...}, new Foo {...}, ...}
            {
                var A = value as Array;
                if (Type == TypeInfo.Anonymous)
                    Type = new TypeInfo { Type = A.GetType() };

                block.Append($"new {Type}");
                WriteArrayValue(block, A, 10);
            }
            else if (value is Dictionary<string, Value>)    // new Foo { A = 1, B = true }
            {
                block.Append($"new {Type}");
                WriteDictionary(block, value as Dictionary<string, Value>);
            }
            else if (value is Dictionary<object, object>)   // new Dictionary<T1,T2> { [t1] = new T2 {...}, ... }
            {
                block.Append($"new {Type}");
                WriteDictionary(block, value as Dictionary<object, object>);
            }
            else
                block.Append(ToPrimitive(value));
        }

        private void WriteArrayValue(CodeBlock block, Array A, int columnNumber)
        {
            Type ty = Type.GetElementType();

            if (ty != null && ty.IsPrimitive)
            {
                if (A.Length < 30)
                {
                    format = ValueOutputFormat.SingleLine;
                }
                else if (A.Length < 100)
                {
                    format = ValueOutputFormat.Wrap;
                    columnNumber = 10;
                }
                else
                {
                    format = ValueOutputFormat.Wrap;
                    columnNumber = 20;
                }
            }

            switch (format)
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

            switch (format)
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

        private void WriteDictionary(CodeBlock block, Dictionary<string, Value> A)
        {
            switch (format)
            {
                case ValueOutputFormat.SingleLine:
                    block.Append("{");
                    A.ForEach(
                         kvp =>
                         {
                             block.Append($"{kvp.Key} = ");
                             kvp.Value.BuildCode(block);
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
                              block.Append($"{kvp.Key} = ");
                              kvp.Value.BuildCode(block);
                          },
                           _ => block.Append(",")
                        );

                    block.End();
                    break;
            }
        }

        public override string ToString()
        {
            return ToPrimitive(value);
        }
    }

}
