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
using System.Data;
using System.Reflection;
using Tie;
using Sys.CodeBuilder;
using System.ComponentModel;


namespace Sys.Data.Manager
{
    public class Packing
    {
        private FieldInfo[] publicFields;
        private Type dpoType;
        PersistentObject instance;

        CSharpBuilder classBuilder;
        Method pack;

        public Packing(Type dpoType)
        {
            this.dpoType = dpoType;
            instance = (PersistentObject)Activator.CreateInstance(this.dpoType);

            this.publicFields = dpoType.GetFields(BindingFlags.Public | BindingFlags.Instance);    //ignore public const fields

            Type baseType = typeof(BasePackage<>);
            baseType = baseType.MakeGenericType(dpoType);

            this.classBuilder = new CSharpBuilder()
            {
                Namespace = dpoType.Assembly.GetName().Name + "." + Setting.DPO_PACKAGE_SUB_NAMESPACE,
            };

            this.classBuilder.AddUsing("System")
            .AddUsing("System.Data")
            .AddUsing("System.Text")
            .AddUsing("System.Collections.Generic")
            .AddUsing("Sys")
            .AddUsing("Sys.Data")
            .AddUsing("Sys.Data.Manager")
            .AddUsing(dpoType.Namespace);


            var clss = new Class(ClassName, new CodeBuilder.TypeInfo { Type = baseType })
            {
                Modifier = Modifier.Public
            };



            //constructor
            clss.Add(new Constructor(ClassName));

            this.pack = new Method("Pack") { Modifier = Modifier.Protected | Modifier.Override };
            clss.Add(pack);

            classBuilder.AddClass(clss);
        }


        public string ClassName
        {
            get
            {
                return dpoType.Name + Setting.DPO_PACKAGE_SUFFIX_CLASS_NAME;
            }
        }

        public TableName TableName
        {
            get
            {
                return instance.TableName;
            }
        }

        private void PackRecord(DataRow dataRow)
        {
            PersistentObject dpo = (PersistentObject)Activator.CreateInstance(this.dpoType, new object[] { dataRow });

            foreach (FieldInfo fieldInfo in this.publicFields)
            {
                object obj = fieldInfo.GetValue(dpo);
                if (obj != null)
                {
                    VAL val = VAL.Boxing(obj);
                    string s = val.ToString();

                    if (obj is float)
                        s = obj.ToString() + "F";
                    else if (obj is string && s.Length > 100)
                    {
                        s = s
                            .Replace("\\r\\n", "\r\n")
                            .Replace("\\n", "\r\n")
                            .Replace("\\t", "\t")
                            .Replace("\\\"", "\"\"")
                            .Replace("\\\\", "\\")
                            ;


                        pack.Statement.AppendFormat("dpo.{0} = @{1}", fieldInfo.Name, s);
                    }
                    else
                    {
                        pack.Statement.AppendFormat("dpo.{0} = {1}", fieldInfo.Name, s);
                    }
                }
            }

            pack.Statement.AppendLine("list.Add(dpo)");
            pack.Statement.AppendLine();
        }




        int count = 0;
        public static bool operator !(Packing packing)
        {
            return packing.count == 0;
        }


        public bool Pack()
        {
            pack.Statement.AppendFormat("var dpo = new {0}()", dpoType.Name);

            PersistentObject dpo = (PersistentObject)Activator.CreateInstance(this.dpoType);
            DataTable dt = new TableReader(dpo.TableName).Table;
            if (dt.Rows.Count == 0)
                return false;

            this.count = dt.Rows.Count;

            //Property property = new Property(typeof(string), "TableName");
            //this.clss.AddProperty(property);
            //property.AddGet("return \"{0}\"", this.instance.TableName.Name);

            //property = new Property(typeof(Level), "Level");
            //this.clss.AddProperty(property);
            //property.AddGet("return Level.{0}", this.instance.Level);


            int i = 0;
            foreach (DataRow dataRow in dt.Rows)
            {
                PackRecord(dataRow);
                if (i < dt.Rows.Count - 1)
                    pack.Statement.AppendFormat("dpo = new {0}()", dpoType.Name);

                i++;
            }

            return true;
        }



        public override string ToString()
        {
            string comment = @"//
// Machine Packed Data
//   by {0}
//
";
            comment = string.Format(comment, ActiveAccount.Account.UserName);
            string classFormat = @"{0}{1}";

            return string.Format(classFormat, comment, this.classBuilder);
        }

    }
}
