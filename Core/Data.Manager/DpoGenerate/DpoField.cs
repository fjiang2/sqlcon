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
using Tie;
using Sys.CodeBuilder;


namespace Sys.Data.Manager
{
    class DpoField
    {
        DpoClass dpoClass;

        private IColumn column;
        
        
        public DpoField(DpoClass dpoClass, IColumn column)
        {
            this.dpoClass = dpoClass;
            this.column = column;
        }



        public void GenerateField()
        {
            
            string fieldName = column.ColumnName.FieldName();
            string ty = ColumnSchema.GetFieldType(column.DataType, column.Nullable);

            Property prop = new Property(new CodeBuilder.TypeInfo { userType = ty }, fieldName);
            var attr = prop.AddAttribute<ColumnAttribute>();
            if (dpoClass.option.HasColumnAttribute || column.ColumnName != fieldName)
            {
                Attribute(attr, column);
            }

            if (dpoClass.Nonvalized.IndexOf(fieldName) != -1)
                prop.AddAttribute<NonValizedAttribute>();

            //When programmer make field Nullable, it must be Nullable
            //if(dgc.NullableFields.IndexOf(fieldName) != -1)
            //    nullable = true;

            dpoClass.clss.Add(prop);


            if(dpoClass.option.HasColumnAttribute)
                attr.comment = new Comment(string.Format("{0}({1}) {2}", column.DataType, column.AdjuestedLength(), column.Nullable ? "null" : "not null"));

            dpoClass.dict_column_field.Add(column.ColumnName, new PropertyDefinition(ty, fieldName));

            

            if (column.ForeignKey != null && dpoClass.Dict != null)
            {
                TableName pkTableName = new TableName(
                    column.ForeignKey.TableName.DatabaseName, 
                    column.ForeignKey.PK_Schema,
                    column.ForeignKey.PK_Table);  //column.ForeignKey.TableName;

                if (dpoClass.Dict.ContainsKey(pkTableName))
                {
                    Type type = dpoClass.Dict[pkTableName];
                    ForeignKey.GetAttribute(column.ForeignKey, type);
                }
                else
                {
                    //###
                    //ForeignKey check for external Dpo classes, they don't be load into dict
                    //var log = new LogDpoClass(pkTableName);
                    //if (log.Exists)
                    //{
                    //    string classFullName = string.Format("{0}.{1}", log.name_space, log.class_name);
                    //    line = string.Format("{0}{1}\r\n", tab, ForeignKey.GetAttribute(column.ForeignKey, classFullName)) + line;
                    //}
                    //else
                        throw new MessageException("cannot generate Dpo class of FK {0} before generate Dpo class of PK {1}",
                            dpoClass.MetaTable.TableName,
                            pkTableName);
                }
            }

            return;
        }

       

        public void GetConstStringColumnName()
        {
            var clss = dpoClass.clss;
            string _columnName = dpoClass.dict_column_field[column.ColumnName].PropertyName;


            Field field = new Field(new CodeBuilder.TypeInfo { type = typeof(string) }, $"_{_columnName}", $"\"{column.ColumnName}\"")
            {
                modifier= Modifier.Public | Modifier.Const
            };

            clss.Add(field);
            
            return;
        }

        public void GenerateImageField()
        {
            string fieldName = column.ColumnName.FieldName();
            
            Property prop = new Property(new CodeBuilder.TypeInfo { userType = "Image" }, $"{fieldName}Image");
            prop.gets.IF($"{fieldName} != null", new CodeBlock()
                    .AppendLine($"System.IO.MemoryStream stream = new System.IO.MemoryStream({fieldName});")
                    .AppendLine("return System.Drawing.Image.FromStream(stream);")
                    .WrapByBeginEnd())
                .AppendLine("return null;");

            prop.sets.IF("value != null", new CodeBlock()
                    .AppendLine("System.IO.MemoryStream stream = new System.IO.MemoryStream();")
                    .AppendLine("value.Save(stream, System.Drawing.Imaging.ImageFormat.Jpeg);")
                    .AppendLine($"{fieldName} = stream.ToArray();")
                    .WrapByBeginEnd());

            dpoClass.clss.Add(prop);
        }


        private static void Attribute(AttributeInfo attr, IColumn column)
        {
            List<string> args = new List<string>();
            string _columnName = column.ColumnName.FieldName();
            args.Add($"_{_columnName}");
            args.Add($"CType.{column.CType}");

            switch (column.CType)
            {
                case CType.VarBinary:
                case CType.Binary:
                    args.Add(string.Format("Length = {0}", column.AdjuestedLength()));
                    break;

                case CType.Char:
                case CType.VarChar:
                case CType.NChar:
                case CType.NVarChar:
                    int len = column.AdjuestedLength();
                    if (len != -1)
                        args.Add(string.Format("Length = {0}", len));
                    break;


                //case CType.Numeric:
                case CType.Decimal:
                    args.Add($"Precision = {column.Precision}");
                    args.Add($" Scale = {column.Scale}");
                    break;
            }

            

            if (column.Nullable)
                args.Add("Nullable = true");   //see: bool Nullable = false; in class DataColumnAttribute

            if (column.IsIdentity)
                args.Add("Identity = true");

            if (column.IsPrimary)
                args.Add("Primary = true");

            if (column.IsComputed)
                args.Add("Computed = true");

            attr.args = args.ToArray();
        }
    }
}
