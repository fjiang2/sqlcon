using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Tie;

namespace Sys.CodeBuilder
{
    public class AttributeInfo
    {
        public string name { get; set; }
        public string[] args { get; set; }

        public Comment comment { get; set; }

        public AttributeInfo(string name)
        {
            this.name = name;
        }

        public AttributeInfo(string name, params object[] args)
        {
            this.name = name;

            if (args != null)
            {
                List<string> list = new List<string>();
                foreach (var arg in args)
                {
                    if (arg is string)
                    {
                        list.Add(arg as string);
                    }
                    else if (arg is AttributeInfoArg)
                    {
                        list.Add((arg as AttributeInfoArg).ToString());
                    }
                    else
                    {
                        foreach (var propertyInfo in arg.GetType().GetProperties())
                        {
                            var val = VAL.Boxing(propertyInfo.GetValue(arg));
                            list.Add($"{propertyInfo.Name} = {val}");
                        }
                    }
                }

                this.args = list.ToArray();
            }
        }

        public override string ToString()
        {
            string text;
            if (args == null)
                text = string.Format("[{0}]", name);
            else
                text = string.Format("[{0}({1})]", name, string.Join(", ", args));

            int pad = 70;
            pad = pad - text.Length;
            if (pad < 0)
                pad = 0;

            string sp = new string(' ', pad);

            if (comment != null)
                return $"{text}{sp}{comment}";
            else
                return text;
        }
    }
}
