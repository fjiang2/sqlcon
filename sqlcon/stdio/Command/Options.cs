using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Stdio
{

    public class OptionItem
    {
        public char Prefix { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }
    }

    public class Options
    {
        List<OptionItem> list = new List<OptionItem>();

        public void Add(string arg)
        {
            OptionItem item = new OptionItem();

            if (arg.Length > 1)
            {
                item.Prefix = arg[0];
                arg = arg.Substring(1);
                int index = arg.IndexOf(':');
                if (index == -1)
                {
                    item.Name = arg;
                    item.Value = null;
                }
                else
                {
                    item.Name = arg.Substring(0, index);
                    item.Value = arg.Substring(index + 1);
                }

                list.Add(item);
            }
            //else  //Tie code may use + or - in expression
            //    throw new Exception("bad argument");
        }

        public bool Has(string arg)
        {
            if (arg.Length > 1)
                return Has(arg[0], arg.Substring(1));
            else
                throw new Exception("bad argument");
        }

        public bool Has(char prefix, string name)
        {
            return list.Where(item => item.Prefix == prefix && item.Name == name).Count() > 0;
        }
        public bool HasOnly(char prefix, string name)
        {
            return list.Where(item => item.Prefix == prefix && item.Name == name && item.Value == null).Count() > 0;
        }

        public string GetValue(string arg)
        {
            if (arg.Length > 1)
                return GetValue(arg[0], arg.Substring(1));
            else
                throw new Exception("bad argument");
        }

        public string GetValue(char prefix, string name)
        {
            return list.Where(item => item.Prefix == prefix && item.Name == name).Select(item => item.Value).FirstOrDefault();
        }
    }
}
