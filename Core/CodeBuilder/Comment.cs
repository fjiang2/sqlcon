using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public enum Alignment
    {
        Top,
        Center,
        Right,
    }

    public class Comment
    {
        private string comment;

        public Alignment alignment { get; set; } = Alignment.Right;

        public Comment(string text)
        {
            this.comment = text;
        }

        public void Clear()
        {
            comment = null;
        }

        public override string ToString()
        {
            if (comment == null)
                return string.Empty;

            if (alignment == Alignment.Right)
                return $"\t\t//{comment}";
            else
                return $"//{comment}";
        }
    }
}
