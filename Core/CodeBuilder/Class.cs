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
using System.Reflection;

namespace Sys.CodeBuilder
{
    public class Class : Buildable
    {
        public Modifier modifier { get; set; } = Modifier.Public;

        List<Constructor> constructors = new List<Constructor>();
        List<Field> fields = new List<Field>();
        List<Method> methods = new List<Method>();
        List<Property> properties = new List<Property>();

        private string className;
        private Type[] inherits;

        public Class(string className)
            : this(className, new Type[] { })
        {
        }

        public Class(string className, Type[] inherits)
        {
            this.className = className;
            this.inherits = inherits;
        }

        public Class AddConstructor(Constructor constructor)
        {
            this.constructors.Add(constructor);
            return this;
        }

        public Class AddField(Field field)
        {
            this.fields.Add(field);
            return this;
        }

        public Class AddField<T>(Modifier modifer, string name, object value = null)
        {
            var field = new Field(new TypeInfo { type = typeof(T) }, name, value)
            {
                modifier = modifier
            };

            this.fields.Add(field);
            return this;
        }


        public Class AddMethod(Method method)
        {
            this.methods.Add(method);
            return this;
        }

        public Class AddProperty(Property property)
        {
            this.properties.Add(property);
            return this;
        }

        protected override CodeBlock BuildBlock()
        {
            CodeBlock clss = base.BuildBlock();

            clss.AppendFormat("{0} class {1}", new ModifierString(modifier), className);
            if (inherits.Length > 0)
                clss.AppendFormat("\t: {0}", string.Join(", ", inherits.Select(inherit => new TypeInfo { type = inherit }.ToString())));

            int tab = 0;
            var body = new CodeBlock();

            var flds = fields.Where(fld => (fld.modifier & Modifier.Const) != Modifier.Const);
            foreach (Field field in flds)
            {
                body.Add(field, tab);
            }

            foreach (Constructor constructor in constructors)
            {
                body.Add(constructor, tab);
                body.AppendLine();
            }

            foreach (Property property in properties)
            {
                body.Add(property, tab);
                body.AppendLine();
            }

            foreach (Method method in methods)
            {
                body.Add(method, tab);
                body.AppendLine();
            }

            flds = fields.Where(fld => (fld.modifier & Modifier.Const) == Modifier.Const);
            if (flds.Count() > 0)
            {
                body.AppendLine();
                foreach (Field field in flds)
                {
                    body.Add(field, tab);
                }
            }


            clss.AddBeginEnd(body);
            return clss;
        }

    }
}
