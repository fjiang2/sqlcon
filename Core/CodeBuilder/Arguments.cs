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
    public class Arguments
    {
        private readonly List<Argument> args = new List<Argument>();

        public Arguments()
        {
        }

        public Arguments(IEnumerable<Argument> args)
        {
            foreach (var arg in args)
                this.args.Add(arg);
        }

        public Arguments Add(Argument arg)
        {
            args.Add(arg);
            return this;
        }

        public Arguments Add(string userType, string name)
        {
            var arg = new Argument(new TypeInfo { UserType = userType }, name);

            args.Add(arg);
            return this;
        }

        public Arguments Add(Type type, string name)
        {
            var arg = new Argument(new TypeInfo { Type = type }, name);

            args.Add(arg);
            return this;
        }

        public Arguments Add<T>(string name)
        {
            var arg = new Argument(new TypeInfo { Type = typeof(T) }, name);
            args.Add(arg);

            return this;
        }

        public bool IsEmpty
        {
            get
            {
                return args.Count == 0;
            }
        }

        public override string ToString()
        {
            return string.Join(", ", args.Select(arg => arg.ToString()));
        }
    }
}
