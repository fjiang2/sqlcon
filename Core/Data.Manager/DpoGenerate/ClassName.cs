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
using Sys.Data;
using Sys.CodeBuilder;

namespace Sys.Data.Manager
{
  

    public class ClassName
    {
        string nameSpace;
        string className;
        AccessModifier modifier;


        public ClassName(string nameSpace, AccessModifier modifier, string className)
        {
            this.nameSpace = nameSpace;
            this.modifier = modifier;
            this.className = className;
        }


        public ClassName(DPObject dpo)
            :this(dpo.GetType())
        { 
        }

        public ClassName(Type ty)
            : this(ty.Namespace, AccessModifier.Public, ty.Name)
        {
            if (ty.IsPublic)
                this.modifier = AccessModifier.Public;
            else
                this.modifier = AccessModifier.Internal;
        }


        public ClassName(string nameSpace, AccessModifier modifier, ClassTableName tname)
            :this(nameSpace, modifier, tname.ClassName)
        {
        }

        public ClassName(string nameSpace, AccessModifier modifier, ClassTableName tname, bool subNamespace)
            : this(nameSpace, modifier, tname)
        {
            if (subNamespace)  //create sub-namespace for each database
                this.nameSpace = string.Format("{0}.{1}", nameSpace, tname.SubNamespace);
                
        }

        public string Namespace { get { return this.nameSpace; } }
        public string Class { get { return this.className; } }
        public AccessModifier Modifier { get { return this.modifier; } }

       

        public override string ToString()
        {
            return string.Format("{0}.{1}", this.nameSpace, this.className);
        }
    }
}
