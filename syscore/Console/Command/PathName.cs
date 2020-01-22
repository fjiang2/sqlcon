using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Stdio
{
    public class PathName
    {
        public readonly string wildcard = null;
        public readonly string where = null;
        public readonly string name = null;
        private string[] fullSegments = new string[0];
        public readonly string[] segments = new string[0];


        private string fullName;
        public PathName(string fullName)
        {
            this.fullName = fullName;

            if (string.IsNullOrEmpty(fullName))
                fullSegments = new string[0];

            else
            {
                fullSegments = fullName.Split('\\');
                int n1 = 0;
                int n2 = fullSegments.Length - 1;

                if (string.IsNullOrEmpty(fullSegments[n1]))
                    fullSegments[n1] = "\\";

                if (fullSegments[n2] == "")
                {
                    fullSegments = fullSegments.Take(n2).ToArray();
                    segments = fullSegments;
                }
                else if (fullSegments[n2].IndexOf('*') >= 0 || fullSegments[n2].IndexOf('?') >= 0)
                {
                    wildcard = fullSegments[n2];
                    fullSegments = fullSegments.Take(n2).ToArray();
                    segments = fullSegments;
                }
                else if (IsWhere(fullSegments[n2]))
                {
                    where = fullSegments[n2];
                    fullSegments = fullSegments.Take(n2).ToArray();
                    segments = fullSegments;
                }
                else
                {
                    name = fullSegments[n2];
                    segments = fullSegments.Take(n2).ToArray();
                }
            }
        }



        private static bool IsWhere(string text)
        {
            string[] keys = new string[] { "(", ")", "=", ">", "<", " and ", " or ", " between ", " not ", " is " };
            text = text.ToLower();

            foreach (var key in keys)
            {
                if (text.IndexOf(key) > 0)
                    return true;
            }

            return false;
        }

        public string[] FullSegments
        {
            get
            {
                return this.fullSegments;
            }
        }

        public override string ToString()
        {
            return this.fullName;
        }
    }
}
