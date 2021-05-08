//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        DPO(Data Persistent Object)                                                               //
//                                                                                                  //
//          Copyright(c) Datum Connect Inc.                                                         //
//                                                                                                  //
// This source code is subject to terms and conditions of the Datum Connect Software License. A     //
// copy of the license can be found in the License.html file at the root of this distribution. If   //
// you cannot locate the  Datum Connect Software License, please send an email to                   //
// datconn@gmail.com. By using this source code in any fashion, you are agreeing to be bound        //
// by the terms of the Datum Connect Software License.                                              //
//                                                                                                  //
// You must not remove this notice, or any other, from this software.                               //
//                                                                                                  //
//                                                                                                  //
//--------------------------------------------------------------------------------------------------//
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    class UniqueNameMaker
    {
        private Dictionary<string, int> names = new Dictionary<string, int>();

        public UniqueNameMaker(string className)
        {
            names.Add(className, 0);
        }

        /// <summary>
        /// make unique name by adding posfix number
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public string ToUniqueName(string name)
        {
            if (names.ContainsKey(name))
                names[name] += 1;
            else
                names.Add(name, 0);

            int index = names[name];
            if (index == 0)
                return name;
            else
                return $"{name}{index}";
        }
    }
}
