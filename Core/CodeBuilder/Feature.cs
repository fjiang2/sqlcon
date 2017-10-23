using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Feature : Declare, ICodeBlock
    {
        public int? value { get; set; }

        public Feature(string feature)
            : base(feature)
        {

        }


        protected override void BuildBlock(CodeBlock block)
        {
            base.BuildBlock(block);

            if (comment?.alignment == Alignment.Top)
            {
                block.AppendFormat(comment.ToString());
                comment.Clear();
            }

            if (value != null)
                block.AppendLine($"{name} = {value}, {comment}");
            else
                block.AppendLine($"{name}, {comment}");
        }

    }
}
