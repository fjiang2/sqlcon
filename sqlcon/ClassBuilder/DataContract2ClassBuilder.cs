using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using Sys;
using Sys.CodeBuilder;
using Sys.Data;

namespace sqlcon
{

    class DataContract2ClassBuilder : TheClassBuilder
    {
        private DataTable dt;

        public string mtd { get; set; }
        private IDictionary<DataColumn, TypeInfo> dict { get; }

        public DataContract2ClassBuilder(ApplicationCommand cmd, DataTable dt)
            : base(cmd)
        {
            this.dt = dt;
            this.dict = CreateMapOfTypeInfo(dt);

            builder.AddUsing("System");
            builder.AddUsing("System.Collections.Generic");
            builder.AddUsing("System.Data");
            builder.AddUsing("System.Linq");
            builder.AddUsing("Sys.Data");

            AddOptionalUsing();
        }

        public static IDictionary<DataColumn, TypeInfo> CreateMapOfTypeInfo(DataTable dt)
        {
            Dictionary<DataColumn, TypeInfo> dict = new Dictionary<DataColumn, TypeInfo>();

            foreach (DataColumn column in dt.Columns)
            {
                TypeInfo ty = new TypeInfo { Type = column.DataType };
                if (column.AllowDBNull)
                {
                    ty.Nullable = true;
                }
                else
                {
                    foreach (DataRow row in dt.Rows)
                    {
                        if (row[column] == DBNull.Value)
                            ty.Nullable = true;
                        break;
                    }
                }

                dict.Add(column, ty);
            }

            return dict;
        }


        protected override void CreateClass()
        {

            var clss = new Class(cname, new TypeInfo { Type = typeof(IDataContractRow) }, new TypeInfo { UserType = $"IEquatable<{cname}>" })
            {
                Modifier = Modifier.Public | Modifier.Partial
            };
            builder.AddClass(clss);



            foreach (DataColumn column in dt.Columns)
            {
                clss.Add(new Property(dict[column], column.ColumnName) { Modifier = Modifier.Public });
            }



            int i;
            int count;
            Statement sent;

            Func<DataColumn, string> COLUMN = column => "_" + column.ColumnName.ToUpper();

            Method method;



            Method mtdFillObject = new Method("FillObject")
            {
                Modifier = Modifier.Public,
                Params = new Parameters().Add(typeof(DataRow), "row")
            };
            clss.Add(mtdFillObject);

            Method mtdUpdateRow = new Method("UpdateRow")
            {
                Modifier = Modifier.Public,
                Params = new Parameters().Add(typeof(DataRow), "row")
            };
            clss.Add(mtdUpdateRow);

            Method mtdCopyTo = new Method("CopyTo")
            {
                Modifier = Modifier.Public,
                Params = new Parameters().Add(cname, "obj")
            };
            clss.Add(mtdCopyTo);




            var sent1 = mtdFillObject.Statement;
            var sent2 = mtdUpdateRow.Statement;
            var sent3 = mtdCopyTo.Statement;

            count = dt.Columns.Count;
            i = 0;
            foreach (DataColumn column in dt.Columns)
            {
                var type = dict[column];
                var NAME = COLUMN(column);
                var name = column.ColumnName;

                var line = $"this.{name} = row.Field<{type}>({NAME});";
                sent1.AppendLine(line);

                line = $"row.SetField({NAME}, this.{name});";
                sent2.AppendLine(line);

                line = $"obj.{name} = this.{name};";
                sent3.AppendLine(line);
            }

            Method mtdEquals = new Method("Equals")
            {
                Modifier = Modifier.Public,
                Type = new TypeInfo { Type = typeof(bool) },
                Params = new Parameters().Add(cname, "obj")
            };
            clss.Add(mtdEquals);
            sent = mtdEquals.Statement;
            sent.AppendLine("return ");
            var variables = dict.Keys.Select(column => column.ColumnName);
            variables.ForEach(
               variable => sent.Append($"this.{variable} == obj.{variable}"),
               variable => sent.AppendLine("&& ")
               );
            sent.Append(";");

            Method mtdNewObject = new Method("NewObject")
            {
                Modifier = Modifier.Public | Modifier.Static,
                Type = new TypeInfo { UserType = cname },
                Params = new Parameters().Add(typeof(DataRow), "row"),
                IsExtensionMethod = false
            };
            clss.Add(mtdNewObject);
            sent = mtdNewObject.Statement;
            sent.AppendLine($"return new {cname}");
            sent.Begin();

            count = dt.Columns.Count;
            i = 0;
            foreach (DataColumn column in dt.Columns)
            {
                var type = dict[column];
                var NAME = COLUMN(column);
                var line = $"{column.ColumnName} = row.Field<{type}>({NAME})";
                if (++i < count)
                    line += ",";

                sent.AppendLine(line);
            }
            sent.End(";");

            method = new Method("CreateTable")
            {
                Modifier = Modifier.Public | Modifier.Static,
                Type = new TypeInfo { Type = typeof(DataTable) }
            };
            clss.Add(method);
            sent = method.Statement;
            sent.AppendLine("DataTable dt = new DataTable();");
            foreach (DataColumn column in dt.Columns)
            {
                Type ty = dict[column].Type;
                var NAME = COLUMN(column);
                sent.AppendLine($"dt.Columns.Add(new DataColumn({NAME}, typeof({ty})));");
            }
            sent.AppendLine();
            sent.AppendLine("return dt;");


            method = new Method(new TypeInfo { Type = typeof(string) }, "ToString")
            {
                Modifier = Modifier.Public | Modifier.Override
            };
            clss.Add(method);
            sent = method.Statement;


            StringBuilder sb = new StringBuilder("\"{{");
            int index = 0;
            variables.ForEach(
                variable => sb.Append($"{variable}:{{{index++}}}"),
                variable => sb.Append(", ")
                );

            sb.AppendLine("}}\", ");

            variables.ForEach(
                variable => sb.Append($"{variable}"),
                variable => sb.AppendLine(", ")
                );

            sent.AppendFormat("return string.Format({0});", sb);
            clss.AppendLine();

            //Const Field
            foreach (DataColumn column in dt.Columns)
            {
                Field field = new Field(new TypeInfo { Type = typeof(string) }, COLUMN(column), new Value(column.ColumnName))
                {
                    Modifier = Modifier.Public | Modifier.Const
                };
                clss.Add(field);
            }

        }

    }
}
