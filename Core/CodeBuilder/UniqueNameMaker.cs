using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    public class UniqueNameMaker
    {
        private Dictionary<string, int> _names = new Dictionary<string, int>();

        public UniqueNameMaker()
        {

        }

        /// <summary>
        /// make unique name by adding posfix number
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string ToUniqueName(string name)
        {
            if (_names.ContainsKey(name))
                _names[name] += 1;
            else
                _names.Add(name, 0);

            int index = _names[name];
            if (index == 0)
                return name;
            else
                return $"{name}{index}";
        }
    }
}
