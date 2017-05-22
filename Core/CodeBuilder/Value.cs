using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tie;

namespace Sys.CodeBuilder
{
    public enum ValueOutputFormat
    {
        SingleLine,
        MultipleLine,
        Wrap
    }

    public class Value : Buildable
    {
        private object value;
        public TypeInfo type { get; set; } = TypeInfo.Anonymous;
        public ValueOutputFormat format { get; set; } = ValueOutputFormat.MultipleLine;

        public Value(object value)
        {
            this.value = value;
        }

        public static string ToPrimitive(object value)
        {
            return VAL.Boxing(value).ToString();
        }
        private static Value NewValue(object value)
        {
            if (value is Value)
                return (Value)value;

            else
                return new Value(value);
        }


        public void BuildCode(CodeBlock block)
        {
            if (value is Value)
            {
                (value as Value).BuildCode(block);
            }
            else if (value is Array)
            {
                block.Append($"new {type}");
                WriteArrayValue(block, value as Array, 10);
            }
            else if (value is ObjectValue)
            {
                block.Append($"new {type}");
                WriteDictionary(block, value as ObjectValue);
            }
            else if (value is Dictionary<object, object>)
            {
                block.Append($"new {type}");
                WriteDictionary(block, value as Dictionary<object, object>);
            }
            else
                block.Append(ToPrimitive(value));
        }

        private void WriteArrayValue(CodeBlock block, Array A, int columnNumber)
        {
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

        private void WriteDictionary(CodeBlock block, ObjectValue A)
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

    public class ObjectValue : Dictionary<string, Value>
    {

    }
}
