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

            if (value is Array)
            {
                block.Append($"new {type}");
                WriteArrayValue(block, value as Array, 10);
            }
            else if (value is Dictionary<object, object>)
            {
                block.Append($"new {type}");
                WriteDictionaryValue(block, value as Dictionary<object, object>, 10);
            }
            else
                block.Append(VAL.Boxing(value).ToString());
        }

        private void WriteArrayValue(CodeBlock block, Array A, int columnNumber)
        {
            if (A.Length <= columnNumber)
            {
                block
                    .Append("{")
                    .Append(string.Join(",", A))
                    .Append("}");
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

            block.End();
        }

        private void WriteDictionaryValue(CodeBlock block, Dictionary<object, object> A, int columnNumber)
        {
            block.Begin();
            foreach (var kvp in A)
            {
                block.AppendFormat("[{0}] = {1},", kvp.Key, kvp.Value);
            }
            block.End();
        }


        public override string ToString()
        {
            return VAL.Boxing(value).ToString();
        }
    }
}
