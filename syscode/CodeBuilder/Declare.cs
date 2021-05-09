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
    public class Declare : Buildable
    {
        protected List<AttributeInfo> attributes { get; } = new List<AttributeInfo>();

        public Modifier Modifier { get; set; } = Modifier.Public;

        public TypeInfo Type { get; set; } = new TypeInfo();

        public Comment Comment { get; set; }

        public string Name { get; }

        public Declare(string name)
        {
            this.Name = name;
        }

        public void AddAttribute(AttributeInfo attr)
        {
            attributes.Add(attr);
        }

        public AttributeInfo AddAttribute<T>() where T : Attribute
        {
            var name = typeof(T).Name;
            name = name.Substring(0, name.Length - nameof(Attribute).Length);
            var attr = new AttributeInfo(name);
            this.AddAttribute(attr);

            return attr;
        }


        protected string Signature 
        {
            get
            {
                if (Type != null)
                    return string.Format("{0} {1} {2}", new ModifierString(Modifier), Type, Name);
                else
                    return string.Format("{0} {1}", new ModifierString(Modifier), Name);
            }
        }




        protected override void BuildBlock(CodeBlock block)
        {

            if (attributes.Count != 0)
            {
                foreach (var attr in attributes)
                    block.AppendLine(attr.ToString());
            }

        }

    }
}
