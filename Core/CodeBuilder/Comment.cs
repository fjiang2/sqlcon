using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public enum Orientation
    {
        Horizontal,
        Vertical
    }

    public class Comment
    {
        private string comment;

        public Orientation Orientation { get; set; } = Orientation.Horizontal;

        public Comment(string text)
        {
            this.comment = text;
        }

        public override string ToString()
        {
            if (comment == null)
                return string.Empty;

            if (Orientation == Orientation.Horizontal)
                return $"\t\t//{comment}";
            else
                return $"//{comment}";
        }
    }
}
