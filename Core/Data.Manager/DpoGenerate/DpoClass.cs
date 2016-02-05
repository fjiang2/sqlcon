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
using System.Text;
using System.Data;
using System.IO;
using Sys.Data;
using System.Reflection;
using System.Linq;
using Tie;
using Sys.CodeBuilder;

namespace Sys.Data.Manager
{


    class DpoClass
    {
        private string nameSpace;
        private string className;

        private ITable metaTable;
        private List<string> nonvalized;
        private List<string> nullableFields;

        public Dictionary<string, PropertyDefinition> dict_column_field = new Dictionary<string, PropertyDefinition>();
        public bool HasColumnAttribute = true;
        public bool HasTableAttribute = true;

        Dictionary<TableName, Type> dict;

        internal ClassBuilder code;
        internal Class clss;

        public DpoClass(ITable metaTable, ClassName cname, Dictionary<TableName, Type> dict)
        {
            this.metaTable = metaTable;

            this.nameSpace = cname.Namespace;
            this.className = cname.Class;

            this.dict = dict;

            this.code = new ClassBuilder { nameSpace = cname.Namespace, };

            code.AddUsing("System");
            code.AddUsing("System.Collections.Generic");
            code.AddUsing("System.Text");
            code.AddUsing("System.Data");
            code.AddUsing("System.Drawing");
            code.AddUsing("Sys.Data");
            code.AddUsing("Sys.Data.Manager");

            clss = new Class(cname.Class, new Type[] { typeof(DPObject) })
            {
                modifier = Modifier.Public | Modifier.Partial
            };

            this.code.AddClass(clss);

            nonvalized = NonvalizedList(nameSpace, className);
            nullableFields = NullableList(nameSpace, className);
        }


        public Dictionary<TableName, Type> Dict
        {
            get { return this.dict; }
        }

        public ITable MetaTable
        {
            get { return this.metaTable; }
        }

        public List<string> Nonvalized
        {
            get
            {
                return this.nonvalized;
            }
        }

        public List<string> NullableFields
        {
            get
            {
                return this.nullableFields;
            }
        }

        private string DPObjectId()
        {

            string DPObjectId = @"
        //must override when logger is used
        protected override int DPObjectId
        {
            get
            {
                throw new NotImplementedException();
            }
        }
        ";

            string DPObjectIdProperty = @"
        protected override int DPObjectId
        {{
            get
            {{
                return this.{0};
            }}
        }}
";

            Property prop = clss.AddProperty<int>(Modifier.Protected | Modifier.Override, "DPObjectId");

            if (metaTable.Identity.Length > 0)
            {
                prop.gets.AppendFormat("return this.{0};", metaTable.Identity.ColumnNames[0]);
                return string.Format(DPObjectIdProperty, metaTable.Identity.ColumnNames[0]);
            }
            else if (metaTable.PrimaryKeys.Length == 1)
            {
                string key = metaTable.PrimaryKeys.Keys[0];
                IColumn column = metaTable.Columns[key];

                if (column.CType == CType.Int)
                {
                    prop.gets.AppendFormat("return this.{0};", dict_column_field[key].PropertyName);
                    return string.Format(DPObjectIdProperty, dict_column_field[key].PropertyName);
                }

            }

            prop.gets.Add(new CodeBlock().AppendLine("throw new NotImplementedException();"));

            return DPObjectId;
        }



        private string PrimaryKeys()
        {

            string prop1 = @"
        public override IPrimaryKeys Primary
        {
            get
            {
                return new PrimaryKeys(new string[] {});
            }
        }
        ";

            string prop2 = @"
        public override IPrimaryKeys Primary
        {{
            get
            {{
                return new PrimaryKeys(new string[] {{ {0} }});
            }}
        }}
";
            Property prop = clss.AddProperty<IPrimaryKeys>(Modifier.Public | Modifier.Override, "Primary");

            if (metaTable.PrimaryKeys.Length == 0)
            {
                prop.gets.AppendLine("return new PrimaryKeys(new string[] {});");
                return prop1;
            }

            if (metaTable.PrimaryKeys.Length > 0)
            {
                prop.gets.AppendFormat("return new PrimaryKeys(new string[] {{ {0} }});", stringQL(metaTable.PrimaryKeys.Keys));
                return string.Format(prop2, stringQL(metaTable.PrimaryKeys.Keys));
            }

            return "";
        }


        private string IdentitiyKeys()
        {

            string prop1 = @"
        public override IIdentityKeys Identity
        {
            get
            {
                return new IdentityKeys();
            }
        }
        ";

            string prop2 = @"
        public override IIdentityKeys Identity
        {{
            get
            {{
                return new IdentityKeys(new string[] {{ {0} }});
            }}
        }}
";

            Property prop = clss.AddProperty<IIdentityKeys>(Modifier.Public | Modifier.Override, "Identity");


            if (metaTable.Identity.Length == 0)
            {
                prop.gets.AppendLine("return new IdentityKeys();");
                return prop1;
            }

            if (metaTable.Identity.Length > 0)
            {
                prop.gets.AppendFormat(" return new IdentityKeys(new string[] {{ {0} }});", stringQL(metaTable.Identity.ColumnNames));
                return string.Format(prop2, stringQL(metaTable.Identity.ColumnNames));
            }

            return "";
        }

        private string stringQL(string[] S)
        {
            string s = "";
            foreach (string p in S)
            {
                if (s != "")
                    s += ", ";

                s += "_" + dict_column_field[p].PropertyName;
            }

            return s;
        }

        private string PrimaryConstructor()
        {

            string constructor = @"
        public @CLASSNAME(@PARM)
        {
           @ASSIGNMENT

           this.Load();
           if(!this.Exists)
           {
              @ASSIGNMENT    
           }
        }
        ";


            var cons = clss.AddConstructor();

            if (metaTable.PrimaryKeys.Length == 0)
                return string.Empty;

            string[] keys = metaTable.PrimaryKeys.Keys;
            string s1 = "";
            foreach (string p in keys)
            {
                if (s1 != "")
                    s1 += ", ";

                s1 += string.Format("{0} {1}", dict_column_field[p].Type, dict_column_field[p].PropertyName.ToLower());
                cons.args.Add(dict_column_field[p].Type, dict_column_field[p].PropertyName.ToLower());
            }

            Statement sent1 = new Statement();
            Statement sent2 = new Statement();
            string s2 = "";
            foreach (string p in keys)
            {
                s2 += string.Format("this.{0} = {1}; ", dict_column_field[p].PropertyName, dict_column_field[p].PropertyName.ToLower());
                sent1.AppendFormat("this.{0} = {1};", dict_column_field[p].PropertyName, dict_column_field[p].PropertyName.ToLower());
                sent2.AppendFormat("this.{0} = {1};", dict_column_field[p].PropertyName, dict_column_field[p].PropertyName.ToLower());
            }

            cons.statements.Add(sent1);
            cons.statements.AppendLine("this.Load();");
            cons.statements.IF("!this.Exists", sent2.WrapByBeginEnd());

            return constructor
                .Replace("@PARM", s1)
                .Replace("@ASSIGNMENT", s2);

        }

        private string FillAndCollect()
        {

            Method fill = new Method("Fill")
            {
                modifier = Modifier.Public | Modifier.Override,
                args = new Arguments().Add<DataRow>("row")
            };

            Method collect = new Method("Collect")
            {
                modifier = Modifier.Public | Modifier.Override,
                args = new Arguments().Add<DataRow>("row")
            };

            foreach (IColumn column in metaTable.Columns)
            {
                var fieldDef = dict_column_field[column.ColumnName];
                string fieldName = fieldDef.PropertyName;
                fill.statements.AppendFormat("this.{0} = GetField<{1}>(row, _{0});", fieldName, fieldDef.Type);
                collect.statements.AppendFormat("SetField(row, _{0}, this.{0});", fieldName);
            }

            clss.Add(fill);
            clss.Add(collect);

            CodeBlock block = new CodeBlock();
            block.Add(fill);
            block.AppendLine();
            block.Add(collect);
            return block.ToString(2);

        }

        private string Fields()
        {
            List<DpoField> imageFields = new List<DpoField>();
            string fields = "";
            foreach (IColumn column in metaTable.Columns)
            {
                DpoField field = new DpoField(this, column);
                fields += field.GenerateField() + "\r\n";
                if (column.CType == CType.Image)
                    imageFields.Add(field);
            }

            if (imageFields.Count > 0)
            {
                fields += "\r\n";
                fields += "        #region IMAGE PROPERTIES";
                clss.AddComment("#region IMAGE PROPERTIES");
                foreach (DpoField field in imageFields)
                {
                    fields += field.GenerateImageField() + "\r\n";
                }
                fields += "        #endregion";
                clss.AddComment("#endregion");
            }

            return fields;
        }


        private string ConstStringColumnNames()
        {
            clss.AddComment("#region CONSTANT");
            string columns = "";
            foreach (IColumn column in metaTable.Columns)
            {
                DpoField field = new DpoField(this, column);
                columns += field.GetConstStringColumnName() + "\r\n";
            }

            clss.AddComment("#endregion");

            return columns;
        }


        public string Generate(Modifier modifier, ClassTableName ctname)
        {
            //must run it first to form Dictionary
            string fields = Fields();

            long revision = 0;

            Type type = GetType(nameSpace, className);
            if (type != null)
            {
                RevisionAttribute[] attributes = type.GetAttributes<RevisionAttribute>();
                if (attributes.Length > 0)
                {
                    revision = attributes[0].Revision + 1;
                }
            }

            string rev = string.Format("[Revision({0})]", revision);
            if (!HasTableAttribute)
                rev = "";

            string comment = @"//
// Machine Generated Code
//   by {0}
//
";
            string who = "devel";
            if (ActiveAccount.Account != null)
                who = ActiveAccount.Account.UserName;

            comment = string.Format(comment, who);
            string usingString = @"{0}
using System;
using System.Collections.Generic;
using System.Text;
using System.Data;
using System.Drawing;
using Sys.Data;
using Sys.Data.Manager;
";
            comment = string.Format(usingString, comment);
            if (nonvalized.Count > 0)
                comment += "using Tie;";

            string clss = @"@COMMENT

namespace @NAMESPACE
{
    @REVISION
    @ATTRIBUTE
    @MODIFIER partial class @CLASSNAME : DPObject
    {
@FIELDS
        public @CLASSNAME()
        {
        }

        public @CLASSNAME(DataRow row)
            :base(row)
        {
        }

@PRIMARYCONSTRUCTOR

@DPOOBJECTID

@PRIMARYKEYS

@IDENTITYKEYS
 
@CONSTANT

@CREATE_TABLE

@FILL_COLLECT
    }
}

";

            this.clss.AddConstructor();
            var cons = this.clss.AddConstructor();
            cons.args.Add<DataRow>("row");
            cons.baseArgs = new string[] { "row" };


            //public DPCollection<@CLASSNAME> DPCollection(string where, params object[] args)
            //{ 
            //    return new DPCollection<@CLASSNAME>(SELECT(where, args));
            //}


            string constString = @"
        #region CONSTANT

{0}
       
        #endregion 
";

            string CONSTANT = string.Format(constString, ConstStringColumnNames());

            string CREATE_TABLE = @"
        #region CREATE TABLE 

        protected override string CreateTableString 
        {{ 
            get {{ return CREATE_TABLE_STRING; }}
        }}    
        
        private const string CREATE_TABLE_STRING =@""{0}"";


        #endregion 
";


            SQL_CREATE_TABLE_STRING = Sys.Data.TableSchema.GenerateCREATE_TABLE(metaTable);
            CREATE_TABLE = string.Format(CREATE_TABLE, SQL_CREATE_TABLE_STRING);
            if (this.HasColumnAttribute || !this.HasTableAttribute)
                CREATE_TABLE = "";

            string m = "public";
            if (modifier == Modifier.Protected)
                m = "protected";
            else if (modifier == Modifier.Internal)
                m = "internal";
            else if (modifier == Modifier.Private)
                m = "private";

            string attribute = metaTable.GetTableAttribute(ctname);
            if (!HasTableAttribute)
                attribute = "";

            clss = clss
                      .Replace("@COMMENT", comment)
                      .Replace("@NAMESPACE", nameSpace)
                      .Replace("@REVISION", rev)
                      .Replace("@ATTRIBUTE", attribute)
                      .Replace("@MODIFIER", m)
                      .Replace("@PRIMARYCONSTRUCTOR", PrimaryConstructor())
                      .Replace("@CLASSNAME", className)
                      .Replace("@FIELDS", fields)
                      .Replace("@DPOOBJECTID", DPObjectId())
                      .Replace("@PRIMARYKEYS", PrimaryKeys())
                      .Replace("@IDENTITYKEYS", IdentitiyKeys())
                      .Replace("@CONSTANT", CONSTANT)
                      .Replace("@CREATE_TABLE", CREATE_TABLE)
                      .Replace("@FILL_COLLECT", FillAndCollect());


            return clss + code.ToString();


        }

        private string SQL_CREATE_TABLE_STRING;
        public bool IsTableChanged(TableName name)
        {
            DPObject dpo = (DPObject)HostType.NewInstance(string.Format("{0}.{1}", nameSpace, className), new object[] { });
            if (dpo == null)
                return true;
            else
            {
                if (!dpo.TableName.Equals(name))
                    return true;

                if (dpo.SQL_CREATE_TABLE_STRING != SQL_CREATE_TABLE_STRING)
                    return true;

                if (dpo.TableName.Id == dpo.TableId)
                    return true;
            }

            return false;
        }


        private static Type GetType(string nameSpace, string className)
        {
            return HostType.GetType(string.Format("{0}.{1}", nameSpace, className));
        }

        private static List<string> NonvalizedList(string nameSpace, string className)
        {
            List<string> list = new List<string>();
            Type type = GetType(nameSpace, className);

            if (type == null)   //if class not existed
                return list;

            FieldInfo[] fields = type.GetFields();
            foreach (FieldInfo fieldInfo in fields)
            {
                var attributes = (System.Attribute[])fieldInfo.GetCustomAttributes(typeof(NonValizedAttribute), true);
                if (attributes.Length != 0)
                    list.Add(fieldInfo.Name);
            }

            return list;
        }


        private static List<string> NullableList(string nameSpace, string className)
        {
            List<string> list = new List<string>();
            Type type = GetType(nameSpace, className);

            if (type == null)   //if class not existed
                return list;

            FieldInfo[] fields = type.GetFields();
            foreach (FieldInfo fieldInfo in fields)
            {
                if (fieldInfo.FieldType.IsGenericType && fieldInfo.FieldType != typeof(Nullable<DateTime>))
                {
                    if (fieldInfo.FieldType.GetGenericTypeDefinition() == typeof(Nullable<>))
                        list.Add(fieldInfo.Name);
                }
            }

            return list;
        }
    }
}
