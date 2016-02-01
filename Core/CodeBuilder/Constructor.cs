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

namespace Sys.CodeBuilder
{
    public class Constructor : Member
    {
        public Argument[] args { get; set; } = new Argument[] { };
        public Argument[] baseAgrs { get; set; } = new Argument[] { };

        List<string> statements = new List<string>();
        

        public Constructor(string constructorName )
            : base(constructorName)
        {
            base.modifier = AccessModifier.Public;
        }

        protected override string signature
        {
            get
            {
                string _constructor = string.Format("{0}{1}({2})",
                        new Modifier(modifier),
                        name,
                        string.Join(", ", args.Select(arg => arg.ToString()))
                        );

                string _base = string.Format(":base({0})", string.Join(", ", baseAgrs.Select(arg => arg.ToString())));

                if (baseAgrs == null || baseAgrs.Length == 0)
                    return _constructor;
                else
                    return string.Format("{0}\r\n\t\t{1}", _constructor, _base);
            }
        }

        public Constructor AddStatements(string format, params object[] args)
        {
            statements.Add(string.Format(format, args));
            return this;
        }

        public Constructor AddStatements()
        {
            statements.Add("");
            return this;
        }

        public Constructor AddField(Field field)
        {
            statements.Add(field.ToString());
            return this;
        }

        public override string ToString()
        {
            StringBuilder s = new StringBuilder();

            s.Append("\t\t").Append(this.signature).AppendLine()
                .AppendLine("\t\t{");

            foreach (string sent in statements)
            {
                if (sent == "")
                    s.AppendLine();
                else
                    s.Append("\t\t\t").Append(sent).AppendLine(";");
            }

            s.AppendLine("\t\t}");

            return s.ToString();
        }
        

    }
}
