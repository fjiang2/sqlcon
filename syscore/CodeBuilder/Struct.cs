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
using Sys;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Sys.CodeBuilder
{
    public class Struct : Prototype
    {
        private readonly List<Buildable> list = new List<Buildable>();

        public bool Sorted { get; set; } = false;

        public Struct(string structName)
            : base(structName)
        {
            base.Modifier = Modifier.Public;
        }

        public void Clear()
        {
            list.Clear();
        }

        public Struct Add(Buildable code)
        {
            this.list.Add(code);
            code.Parent = this;
            return this;
        }

        public Struct AddRange(IEnumerable<Buildable> members)
        {
            foreach (var member in members)
                this.Add(member);

            return this;
        }

        public Buildable AppendLine()
        {
            var builder = new Buildable();
            this.Add(builder);

            return builder;
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



        protected override void BuildBlock(CodeBlock clss)
        {
            base.BuildBlock(clss);
            if (Comment != null)
            {
                Comment.Alignment = Alignment.Top;
                clss.AppendFormat($"{Comment}");
            }

            clss.AppendFormat("{0} struct {1}", new ModifierString(Modifier), base.Name);
       
            var body = new CodeBlock();

            if (Sorted)
            {
                var flds = fields.Where(fld => (fld.Modifier & Modifier.Const) != Modifier.Const);
                foreach (Field field in flds.OrderBy(fld => fld.Modifier))
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

                flds = fields.Where(fld => (fld.Modifier & Modifier.Const) == Modifier.Const);
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
        }

    }
}
