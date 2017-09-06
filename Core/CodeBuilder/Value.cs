using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tie;

namespace Sys.CodeBuilder
{
    enum ValueOutputFormat
    {
        SingleLine,
        MultipleLine,
        Wrap
    }

    public class Value : Buildable
    {
        public TypeInfo type { get; set; } = TypeInfo.Anonymous;

        private object value;
        private ValueOutputFormat format { get; set; } = ValueOutputFormat.MultipleLine;

        public Value(object value)
        {
            this.value = value;
        }


        public static Value NewPropertyObject(TypeInfo type)
        {
            return new Value(new Dictionary<string, Value>()) { type = type };
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

            return VAL.Boxing(value).ToString();
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
                if (type == TypeInfo.Anonymous)
                    type = new TypeInfo { type = A.GetType() };

                block.Append($"new {type}");
                WriteArrayValue(block, A, 10);
            }
            else if (value is Dictionary<string, Value>)    // new Foo { A = 1, B = true }
            {
                block.Append($"new {type}");
                WriteDictionary(block, value as Dictionary<string, Value>);
            }
            else if (value is Dictionary<object, object>)   // new Dictionary<T1,T2> { [t1] = new T2 {...}, ... }
            {
                block.Append($"new {type}");
                WriteDictionary(block, value as Dictionary<object, object>);
            }
            else
                block.Append(ToPrimitive(value));
        }

        private void WriteArrayValue(CodeBlock block, Array A, int columnNumber)
        {
            Type ty = type.GetElementType();

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
