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

        private void DPObjectId()
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


            Property prop = clss.AddProperty<int>(Modifier.Protected | Modifier.Override, "DPObjectId");

            if (metaTable.Identity.Length > 0)
            {
                prop.gets.AppendFormat("return this.{0};", metaTable.Identity.ColumnNames[0]);
                return;
            }
            else if (metaTable.PrimaryKeys.Length == 1)
            {
                string key = metaTable.PrimaryKeys.Keys[0];
                IColumn column = metaTable.Columns[key];

                if (column.CType == CType.Int)
                {
                    prop.gets.AppendFormat("return this.{0};", dict_column_field[key].PropertyName);
                    return;
                }

            }

            prop.gets.Add(new CodeBlock().AppendLine("throw new NotImplementedException();"));

            return;
        }



        private void PrimaryKeys()
        {
            Property prop = clss.AddProperty<IPrimaryKeys>(Modifier.Public | Modifier.Override, "Primary");

            if (metaTable.PrimaryKeys.Length == 0)
            {
                prop.gets.AppendLine("return new PrimaryKeys(new string[] {});");
                return;
            }

            if (metaTable.PrimaryKeys.Length > 0)
            {
                prop.gets.AppendFormat("return new PrimaryKeys(new string[] {{ {0} }});", stringQL(metaTable.PrimaryKeys.Keys));
                return;
            }

            return;
        }


        private void IdentitiyKeys()
        {

            Property prop = clss.AddProperty<IIdentityKeys>(Modifier.Public | Modifier.Override, "Identity");


            if (metaTable.Identity.Length == 0)
            {
                prop.gets.AppendLine("return new IdentityKeys();");
                return;
            }

            if (metaTable.Identity.Length > 0)
            {
                prop.gets.AppendFormat(" return new IdentityKeys(new string[] {{ {0} }});", stringQL(metaTable.Identity.ColumnNames));
                return;
            }

            return;
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

        private void PrimaryConstructor()
        {

            var cons = clss.AddConstructor();

            if (metaTable.PrimaryKeys.Length == 0)
                return;

            string[] keys = metaTable.PrimaryKeys.Keys;
            foreach (string p in keys)
            {
                cons.args.Add(dict_column_field[p].Type, dict_column_field[p].PropertyName.ToLower());
            }

            Statement sent1 = new Statement();
            Statement sent2 = new Statement();
            foreach (string p in keys)
            {
                sent1.AppendFormat("this.{0} = {1};", dict_column_field[p].PropertyName, dict_column_field[p].PropertyName.ToLower());
                sent2.AppendFormat("this.{0} = {1};", dict_column_field[p].PropertyName, dict_column_field[p].PropertyName.ToLower());
            }

            cons.statements.Add(sent1);
            cons.statements.AppendLine("this.Load();");
            cons.statements.IF("!this.Exists", sent2.WrapByBeginEnd());


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

        private void Fields()
        {
            List<DpoField> imageFields = new List<DpoField>();

            foreach (IColumn column in metaTable.Columns)
            {
                DpoField field = new DpoField(this, column);
                field.GenerateField();
                if (column.CType == CType.Image)
                    imageFields.Add(field);
            }

            if (imageFields.Count > 0)
            {
                clss.AddMember("#region IMAGE PROPERTIES");
                foreach (DpoField field in imageFields)
                {
                    field.GenerateImageField();
                }
                clss.AddMember("#endregion");
            }

            return;
        }


        private void ConstStringColumnNames()
        {
            clss.AddMember("#region CONSTANT");

            foreach (IColumn column in metaTable.Columns)
            {
                DpoField field = new DpoField(this, column);
                field.GetConstStringColumnName();
            }

            clss.AddMember("#endregion");

            return;
        }


        public string Generate(Modifier modifier, ClassTableName ctname)
        {
            //must run it first to form Dictionary
            Fields();

            string comment = @"//
// Machine Generated Code
//
";


            if (nonvalized.Count > 0)
            {
                this.code.AddUsing("Tie");
                comment += "using Tie;";
            }


            this.clss.AddConstructor();
            var cons = this.clss.AddConstructor();
            cons.args.Add<DataRow>("row");
            cons.baseArgs = new string[] { "row" };

            SQL_CREATE_TABLE_STRING = Sys.Data.TableSchema.GenerateCREATE_TABLE(metaTable);

            var attr = clss.AddAttribute<TableAttribute>();
            GetTableAttribute(attr, metaTable, ctname);
            if (HasTableAttribute)
                this.clss.attribute = attr;

            PrimaryConstructor();
            DPObjectId();
            PrimaryKeys();
            IdentitiyKeys();
            FillAndCollect();

            ConstStringColumnNames();

            return code.ToString();
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


        #region Table Attribute Generate


        /// <summary>
        /// [TableName.Level] is not updated in [this.tname], then parameter [level] must be passed in
        /// </summary>
        /// <param name="level"></param>
        /// <returns></returns>
        internal static void GetTableAttribute(AttributeInfo attr, ITable metaTable, ClassTableName ctname)
        {
            attr.comment = new Comment(string.Format("Primary Keys = {0};  Identity = {1};", metaTable.PrimaryKeys, metaTable.Identity));

            List<string> args = new List<string>();

            TableName tableName = metaTable.TableName;
            switch (ctname.Level)
            {

                case Level.Application:
                    args.Add($"\"{tableName.Name}\"");
                    args.Add("Level.Application");
                    break;

                case Level.System:
                    args.Add($"\"{tableName.Name}\"");
                    args.Add("Level.System");
                    break;

                case Level.Fixed:
                    args.Add($"\"{tableName.DatabaseName.Name}\"");
                    args.Add("Level.Fixed");
                    break;
            }


            if (ctname.HasProvider)
            {
                if (!tableName.Provider.Equals(ConnectionProviderManager.DefaultProvider))
                {
                    
                    args.Add(string.Format("Provider = {0}", (int)tableName.Provider));
                }
            }

            if (!ctname.Pack)
                args.Add("Pack = false");

            attr.args = args.ToArray();
            return;
        }



        #endregion


    }
}
