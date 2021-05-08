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
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.CodeBuilder
{
    class UtilsMethod
    {
        private string className;
        private IEnumerable<PropertyInfo> variables;
        private TypeInfo classType;

        public UtilsMethod(string className, IEnumerable<PropertyInfo> variables)
        {
            this.className = className;
            this.variables = variables;
            this.classType = new TypeInfo { UserType = className };

        }

        public Method Map()
        {
            return Assign("Map");
        }

        public Method Copy()
        {
            return Assign("Copy");
        }

        private Method Assign(string methodName)
        {
            Method mtd = new Method(methodName)
            {
                Modifier = Modifier.Public,
            };

            mtd.Params.Add(className, "obj");

            var sent = mtd.Statement;

            foreach (var variable in variables)
            {
                sent.AppendFormat("this.{0} = obj.{0};", variable);
            }

            return mtd;
        }
        public Method CopyTo()
        {
            Method mtd = new Method("CopyTo")
            {
                Modifier = Modifier.Public | Modifier.Static,
                IsExtensionMethod = true
            };

            mtd.Params.Add(className, "from");
            mtd.Params.Add(className, "to");

            var sent = mtd.Statement;

            foreach (var variable in variables)
            {
                sent.AppendFormat("to.{0} = from.{0};", variable);
            }

            return mtd;
        }


        public Method Clone()
        {
            Method mtd = new Method(classType, "Clone")
            {
                Modifier = Modifier.Public,
            };

            var sent = mtd.Statement;

            sent.AppendFormat("var obj = new {0}();", className);
            sent.AppendLine();

            foreach (var variable in variables)
            {
                sent.AppendFormat("obj.{0} = this.{0};", variable);
            }

            sent.AppendLine();
            sent.RETURN("obj");

            return mtd;
        }

        public Method CloneFrom()
        {
            Method mtd = new Method(classType, "Clone")
            {
                Modifier = Modifier.Public | Modifier.Static,
                IsExtensionMethod = true
            };

            mtd.Params.Add(className, "from");
            var sent = mtd.Statement;

            sent.AppendFormat("var obj = new {0}();", className);
            sent.AppendLine();

            foreach (var variable in variables)
            {
                sent.AppendFormat("obj.{0} = from.{0};", variable);
            }

            sent.AppendLine();
            sent.RETURN("obj");

            return mtd;
        }

        public Method Equals()
        {
            Method mtd = new Method(new TypeInfo { Type = typeof(bool) }, "Equals")
            {
                Modifier = Modifier.Public | Modifier.Override,
            };

            mtd.Params.Add<object>("obj");

            var sent = mtd.Statement;
            sent.AppendFormat("var x = ({0})obj;", className);
            sent.AppendLine();

            sent.AppendLine("return ");

            variables.ForEach(
                variable => sent.Append($"this.{variable}.Equals(x.{variable})"),
                variable => sent.AppendLine("&& ")
                );

            sent.Append(";");
            return mtd;
        }

        public Method _GetHashCode()
        {
            Method mtd = new Method(new TypeInfo { Type = typeof(int) }, "GetHashCode")
            {
                Modifier = Modifier.Public | Modifier.Override,
            };

            var sent = mtd.Statement;
            sent.AppendLine("return 0;");
            return mtd;
        }

        public Method Compare()
        {
            Method mtd = new Method(new TypeInfo { Type = typeof(bool) }, "Compare")
            {
                Modifier = Modifier.Public,
            };

            mtd.Params.Add(className, "obj");

            var sent = mtd.Statement;

            sent.AppendLine("return ");

            variables.ForEach(
                variable => sent.Append($"this.{variable}.Equals(obj.{variable})"),
                variable => sent.AppendLine("&& ")
                );

            sent.Append(";");
            return mtd;
        }


        public Method CompareTo()
        {
            Method mtd = new Method(new TypeInfo { Type = typeof(bool) }, "CompareTo")
            {
                Modifier = Modifier.Public | Modifier.Static,
                IsExtensionMethod = true
            };

            mtd.Params.Add(className, "a");
            mtd.Params.Add(className, "b");

            var sent = mtd.Statement;

            sent.AppendLine("return ");

            variables.ForEach(
                variable => sent.Append($"a.{variable} == b.{variable}"),
                variable => sent.AppendLine("&& ")
                );

            sent.Append(";");
            return mtd;
        }

        public Method ToSimpleString()
        {
            Method mtd = new Method(new TypeInfo { Type = typeof(string) }, "ToSimpleString")
            {
                Modifier = Modifier.Public | Modifier.Static,
                IsExtensionMethod = true
            };

            mtd.Params.Add(className, "obj");

            var sent = mtd.Statement;


            StringBuilder builder = new StringBuilder("\"{{");
            int index = 0;
            variables.ForEach(
                variable => builder.Append($"{variable}:{{{index++}}}"),
                variable => builder.Append(", ")
                );

            builder.AppendLine("}}\", ");

            variables.ForEach(
                variable => builder.Append($"obj.{variable}"),
                variable => builder.AppendLine(", ")
                );

            sent.AppendFormat("return string.Format({0});", builder);
            return mtd;
        }

        public Method _ToString()
        {
            Method mtd = new Method(new TypeInfo { Type = typeof(string) }, "ToString")
            {
                Modifier = Modifier.Public | Modifier.Override,
            };

            var sent = mtd.Statement;


            StringBuilder builder = new StringBuilder("\"{{");
            int index = 0;
            variables.ForEach(
                variable => builder.Append($"{variable}:{{{index++}}}"),
                variable => builder.Append(", ")
                );

            builder.AppendLine("}}\", ");

            variables.ForEach(
                variable => builder.Append($"this.{variable}"),
                variable => builder.AppendLine(", ")
                );

            sent.AppendFormat("return string.Format({0});", builder);
            return mtd;
        }

        public Method _ToString_v2()
        {
            Method mtd = new Method(new TypeInfo { Type = typeof(string) }, "ToString")
            {
                Modifier = Modifier.Public | Modifier.Override,
            };

            var sent = mtd.Statement;


            sent.Append("return ");
            sent.Append("$\"");
            variables.ForEach(
                variable => sent.Append($"{variable}:{{{variable}}}"),
                variable => sent.Append(", ")
                );
            sent.Append("\";");

            return mtd;
        }

        public Method ToDictinary()
        {
            Method method = new Method("ToDictionary")
            {
                Modifier = Modifier.Public,
                Type = new TypeInfo { Type = typeof(IDictionary<string, object>) },
            };
            var sent = method.Statement;
            sent.AppendLine("return new Dictionary<string,object>() ");
            sent.Begin();

            foreach (var variable in variables)
            {
                var line = $"[\"{variable}\"] = this.{variable},";
                sent.AppendLine(line);
            }
            sent.End(";");

            return method;
        }

        public Method FromDictinary()
        {
            var type = new TypeInfo { Type = typeof(IDictionary<string, object>) };
            Method method = new Method("Copy")
            {
                Modifier = Modifier.Public,
                Params = new Parameters(new Parameter[] { new Parameter(type, "dictionary") }),
            };

            var sent = method.Statement;
            foreach (var variable in variables)
            {
                TypeInfo typeInfo = variable.PropertyType;
                if (typeInfo.Type == typeof(System.Xml.Linq.XElement))
                    typeInfo.Type = typeof(string);

                var line = $"this.{variable} = ({typeInfo})dictionary[\"{variable.PropertyName}\"];";
                sent.AppendLine(line);
            }

            return method;
        }
    }
}
