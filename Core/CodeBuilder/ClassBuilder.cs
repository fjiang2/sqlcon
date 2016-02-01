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
    public class ClassBuilder
    {
        public string nameSpace { get; set; } = "Sys.Unknown";
        public AccessModifier modifier { get; set; } = AccessModifier.Public;

        List<string> usings = new List<string>();

        List<Constructor> constructors = new List<Constructor>();
        List<Field> fields = new List<Field>();
        List<Method> methods = new List<Method>();
        List<Property> properties = new List<Property>();

        private string className;
        private Type[] inherits;

        public ClassBuilder(string className)
            :this(className, new Type[]{})
        { 
        }

        public ClassBuilder(string className, Type[] inherits)
        {
            this.className = className;
            this.inherits = inherits;
        }

        public ClassBuilder AddUsing(string name)
        {
            this.usings.Add(name);
            return this;
        }

        public ClassBuilder AddConstructor(Constructor constructor)
        {
            this.constructors.Add(constructor);
            return this;
        }
  
        public ClassBuilder AddField(Field field)
        {
            this.fields.Add(field);
            return this;
        }

        public ClassBuilder AddMethod(Method method)
        {
            this.methods.Add(method);
            return this;
        }

        public ClassBuilder AddProperty(Property property)
        {
            this.properties.Add(property);
            return this;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();
            
            s.Append(string.Join("", usings.Select(name =>string.Format("using {0};\r\n",name))));
            s.AppendLine();

            s.AppendFormat("namespace {0}", this.nameSpace).AppendLine();
            s.AppendLine("{");

            s.AppendFormat("\t{0} class {1}", new Modifier(modifier), className);
            if(inherits.Length >0)
                s.AppendFormat(" : {0}", string.Join(", ", inherits.Select(inherit => new TypeInfo { type = inherit }.ToString())));
            
            s.AppendLine();
            s.AppendLine("\t{");

            foreach (Field field in fields)
                s.Append("\t\t").AppendLine(field.ToString());

            s.AppendLine();

            foreach (Constructor constructor in constructors)
                s.AppendLine(constructor.ToString());

            foreach (Property property in properties)
                s.AppendLine(property.ToString());

            foreach (Method method in methods)
                s.AppendLine(method.ToString());


            s.AppendLine("\t}");
            s.AppendLine("}");


            return s.ToString();
        }

    }
}
