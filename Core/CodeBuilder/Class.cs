﻿//--------------------------------------------------------------------------------------------------//
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
using Sys;

namespace Sys.CodeBuilder
{
    public class Class : Declare
    {
        List<Buildable> list = new List<Buildable>();

        private Type[] inherits;
        public bool Sorted { get; set; } = false;

        public Class(string className)
            : this(className, new Type[] { })
        {
        }

        public Class(string className, Type[] inherits)
            : base(className)
        {
            this.inherits = inherits;
            base.modifier = Modifier.Public;
        }


        public Class Add(Buildable code)
        {
            this.list.Add(code);
            return this;
        }

        public Constructor AddConstructor()
        {
            var constructor = new Constructor(base.name);

            this.list.Add(constructor);
            return constructor;
        }

        public Field AddField<T>(Modifier modifier, string name, object value = null)
        {
            var field = new Field(new TypeInfo { type = typeof(T) }, name, value)
            {
                modifier = modifier
            };

            this.list.Add(field);
            return field;
        }

        public Property AddProperty<T>(Modifier modifier, string name, object value = null)
        {
            var property = new Property(new TypeInfo { type = typeof(T) }, name, value)
            {
                modifier = modifier
            };

            this.list.Add(property);
            return property;
        }


        public Member AddMember(string text)
        {
            var member = new Member(text);
            list.Add(member);
            return member;
        }

        private IEnumerable<Constructor> constructors
        {
            get
            {
                return list
                    .Where(item => item is Constructor)
                    .Select(item => (Constructor)item);
            }
        }

        private IEnumerable<Field> fields
        {
            get
            {
                return list
                    .Where(item => item is Field)
                    .Select(item => (Field)item);
            }
        }

        private IEnumerable<Method> methods
        {
            get
            {
                return list
                    .Where(item => item is Method)
                    .Select(item => (Method)item);
            }
        }

        private IEnumerable<Property> properties
        {
            get
            {
                return list
                    .Where(item => item is Property)
                    .Select(item => (Property)item);
            }
        }



        protected override CodeBlock BuildBlock()
        {
            CodeBlock clss = base.BuildBlock();

            clss.AppendFormat("{0} class {1}", new ModifierString(modifier), base.name);
            if (inherits.Length > 0)
                clss.AppendFormat("\t: {0}", string.Join(", ", inherits.Select(inherit => new TypeInfo { type = inherit }.ToString())));

            var body = new CodeBlock();

            if (Sorted)
            {
                var flds = fields.Where(fld => (fld.modifier & Modifier.Const) != Modifier.Const);
                foreach (Field field in flds.OrderBy(fld => fld.modifier))
                {
                    body.Add(field);
                }

                foreach (Constructor constructor in constructors)
                {
                    body.Add(constructor);
                    body.AppendLine();
                }

                foreach (Property property in properties)
                {
                    body.Add(property);

                    if (property.GetBlock().Count > 1)
                        body.AppendLine();
                }

                foreach (Method method in methods)
                {
                    body.Add(method);
                    body.AppendLine();
                }

                flds = fields.Where(fld => (fld.modifier & Modifier.Const) == Modifier.Const);
                if (flds.Count() > 0)
                {
                    body.AppendLine();
                    foreach (Field field in flds)
                    {
                        body.Add(field);
                    }
                }

            }
            else
            {
                list.ForEach(
                    item => body.Add(item),
                    item =>
                        {
                            if (item.Count == 1 && (item is Field || item is Property))
                                return;

                            //if (item.Count == 1 && (item is Member))
                            //    return;

                            body.AppendLine();
                        }
                    );
            }

            clss.AddWithBeginEnd(body);
            return clss;
        }

        public void AddCopyCloneEqualsFunc()
        {
            var rw = properties
                .Where(p => (p.modifier & Modifier.Public) == Modifier.Public && p.CanRead && p.CanWrite)
                .Select(p => p.name);

            var x = new UtilsMethod(this.name, rw);

            Add(x.Copy())
            .Add(x.Clone())
            .Add(x.Compare());
        }

        public void AddCopyCloneCompareExtension(string className, IEnumerable<string> propertyNames)
        {
          
            var x = new UtilsMethod(className, propertyNames);

            Add(x.CopyTo())
            .Add(x.CloneFrom())
            .Add(x.ComparTo())
            .Add(x.ToSimpleString());
        }
    }
}