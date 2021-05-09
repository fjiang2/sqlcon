//--------------------------------------------------------------------------------------------------//
//                                                                                                  //
//        syscode(C# Code Builder)                                                                  //
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
    public class Identifier
    {
        private readonly string name;

        public Identifier(string name)
        {
            if (string.IsNullOrEmpty(name))
                throw new ArgumentNullException("identifier cannot be blank");

            this.name = name;
        }


        public override bool Equals(object obj)
        {
            Identifier id = obj as Identifier;
            if (id != null)
                return this.name.Equals(id.name);

            return false;
        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override string ToString()
        {
            return name;
        }

        public static bool operator ==(Identifier id1, Identifier id2)
        {
            return id1.name.Equals(id2.name);
        }

        public static bool operator !=(Identifier id1, Identifier id2)
        {
            return !id1.name.Equals(id2.name);
        }

        public static implicit operator Identifier(string ident)
        {
            return new Identifier(ident);
        }

        public static explicit operator string(Identifier ident)
        {
            return ident.name;
        }
    }
}
