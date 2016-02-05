using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class Comment
    {
        private string comment;

        public Comment(string text)
        {
            this.comment = text;
        }

        public override string ToString()
        {
            if (comment == null)
                return string.Empty;

            return $"\t\t//{comment}";
        }
    }
}
