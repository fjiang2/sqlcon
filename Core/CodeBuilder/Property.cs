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
    public class Property :Format
    {
        Statements gets = new Statements(3);
        Statements sets = new Statements(3);

        public AccessModifier modifier { get; set; } = AccessModifier.Public;
        public TypeInfo returnType { get; set; }
        public string propertyName { get; set; }

        public Property(TypeInfo returnType, string propertyName)
        {
            this.returnType = returnType;
            this.propertyName = propertyName;
        }


        private string signature
        {
            get
            {
                return string.Format("{0} {1} {2}",
                new Modifier(modifier),
                returnType,
                propertyName
                );
            }
        }

        public Property AddGet(string format, params object[] args)
        {
            gets.Add(string.Format(format, args));
            return this;
        }

        public Property AddGet()
        {
            gets.Add("");

            return this;
        }


        public Property AddSet(string format, params object[] args)
        {
            sets.Add(string.Format(format, args));
            return this;
        }

        public Property AddSet()
        {
            sets.Add("");
            return this;
        }

        public Property AddGetField(Field field)
        {
            gets.Add(field.ToString());
            return this;
        }

        public Property AddSetField(Field field)
        {
            gets.Add(field.ToString());
            return this;
        }


        public override string ToString()
        {
            this.tab = 2;

            if (gets.Count == 0 && sets.Count == 0)
            {
                this.AddFormat("{0} {{get; set; }}", this.signature);
            }
            else
            {
                this.Add(this.signature);
                this.Add("{");
                this.tab++;
                if (gets.Count != 0)
                {
                    this.Add("get");
                    code.Append(gets.Code);

                }

                if (sets.Count != 0)
                {
                    this.Add("set");
                    code.Append(sets.Code);
                }

                this.tab--;
                this.Add("}");
            }

            return code.ToString();
        }
        

    }
}
