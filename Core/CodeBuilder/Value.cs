using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tie;

namespace Sys.CodeBuilder
{
    public class Value : Buildable
    {
        private object value;
        public TypeInfo type { get; set; } = TypeInfo.Anonymous;

        public Value(object value)
        {
            this.value = value;
        }

        public void BuildCode(CodeBlock block)
        {
            if(value is Value)
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
                block.Append(VAL.Boxing(value).ToString());
        }

        private void WriteArrayValue(CodeBlock block, Array A, int columnNumber)
        {
            //if (A.Length <= columnNumber)
            //{
            //    block
            //        .Append("{")
            //        .Append(A.Concatenate(","))
            //        .Append("}");

            //    return;
            //}

            block.Begin();

            for (int i = 0; i < A.Length; i++)
            {
                if (i != 0 && i % columnNumber == 0)
                {
                    block.AppendLine();
                }

                block.AppendLine();
                Value item = new Value(A.GetValue(i));
                item.BuildCode(block);

                if (i != A.Length - 1)
                    block.Append(",");

            }

            block.End();
        }

        private void WriteDictionary(CodeBlock block, Dictionary<object, object> A)
        {
            block.Begin();

            A.ForEach(
                kvp =>
                    {
                        block.AppendLine();
                        block.Append($"[{kvp.Key}] = ");
                        new Value(kvp.Value).BuildCode(block);
                    },
                _ => block.Append(",")
                );

            block.End();
        }

        private void WriteDictionary(CodeBlock block, ObjectValue A)
        {
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
        }

        public override string ToString()
        {
            return VAL.Boxing(value).ToString();
        }
    }

    public class ObjectValue : Dictionary<string, Value>
    {

    }
}
