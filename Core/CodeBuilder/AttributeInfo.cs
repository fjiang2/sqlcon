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
using System.Threading.Tasks;
using Tie;

namespace Sys.CodeBuilder
{
    public class AttributeInfo
    {
        public string Name { get; set; }

        public string[] Args { get; set; }

        public Comment Comment { get; set; }

        public AttributeInfo(string name)
        {
            this.Name = name;
        }

        public AttributeInfo(string name, params object[] args)
        {
            this.Name = name;

            if (args != null)
            {
                List<string> list = new List<string>();
                foreach (var arg in args)
                {
                    if (arg is string)
                    {
                        list.Add(arg as string);
                    }
                    else if (arg is AttributeInfoArg)
                    {
                        list.Add((arg as AttributeInfoArg).ToString());
                    }
                    else
                    {
                        foreach (var propertyInfo in arg.GetType().GetProperties())
                        {
                            var val = VAL.Boxing(propertyInfo.GetValue(arg));
                            list.Add($"{propertyInfo.Name} = {val}");
                        }
                    }
                }

                this.Args = list.ToArray();
            }
        }

        public override string ToString()
        {
            string text;
            if (Args == null)
                text = string.Format("[{0}]", Name);
            else
                text = string.Format("[{0}({1})]", Name, string.Join(", ", Args));

            int pad = 70;
            pad = pad - text.Length;
            if (pad < 0)
                pad = 0;

            string sp = new string(' ', pad);

            if (Comment != null)
                return $"{text}{sp}{Comment}";
            else
                return text;
        }
    }
}
