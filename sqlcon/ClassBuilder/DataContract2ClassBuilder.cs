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


        public DataContract2ClassBuilder(Command cmd, DataTable dt)
            : base(cmd)
        {
            this.dt = dt;

            foreach (DataColumn column in dt.Columns)
            {
                TypeInfo ty = new TypeInfo { type = column.DataType };
                foreach (DataRow row in dt.Rows)
                {
                    if (row[column] == DBNull.Value)
                        ty.Nullable = true;
                    break;
                }

                dict.Add(column, ty);
            }


            builder.AddUsing("System");
            builder.AddUsing("System.Collections.Generic");
            builder.AddUsing("System.Data");
            builder.AddUsing("System.Linq");
            builder.AddUsing("Sys.Data");

            AddOptionalUsing();
        }

        private Dictionary<DataColumn, TypeInfo> dict = new Dictionary<DataColumn, TypeInfo>();

        protected override void CreateClass()
        {

            var clss = new Class(cname, new TypeInfo { type = typeof(IDataContractRow) }, new TypeInfo { userType = $"IEquatable<{cname}>" })
            {
                modifier = Modifier.Public | Modifier.Partial
            };
            builder.AddClass(clss);



            foreach (DataColumn column in dt.Columns)
            {
                clss.Add(new Property(dict[column], column.ColumnName) { modifier = Modifier.Public });
            }



            int i;
            int count;
            Statement sent;

            Func<DataColumn, string> COLUMN = column => "_" + column.ColumnName.ToUpper();

            Method method;



            Method mtdFillObject = new Method("FillObject")
            {
                modifier = Modifier.Public,
                args = new Arguments().Add(typeof(DataRow), "row")
            };
            clss.Add(mtdFillObject);

            Method mtdUpdateRow = new Method("UpdateRow")
            {
                modifier = Modifier.Public,
                args = new Arguments().Add(typeof(DataRow), "row")
            };
            clss.Add(mtdUpdateRow);

            Method mtdCopyTo = new Method("CopyTo")
            {
                modifier = Modifier.Public,
                args = new Arguments().Add(cname, "obj")
            };
            clss.Add(mtdCopyTo);




            var sent1 = mtdFillObject.statements;
            var sent2 = mtdUpdateRow.statements;
            var sent3 = mtdCopyTo.statements;

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
                modifier = Modifier.Public,
                type = new TypeInfo { type = typeof(bool) },
                args = new Arguments().Add(cname, "obj")
            };
            clss.Add(mtdEquals);
            sent = mtdEquals.statements;
            sent.AppendLine("return ");
            var variables = dict.Keys.Select(column => column.ColumnName);
            variables.ForEach(
               variable => sent.Append($"this.{variable} == obj.{variable}"),
               variable => sent.AppendLine("&& ")
               );
            sent.Append(";");

            Method mtdNewObject = new Method("NewObject")
            {
                modifier = Modifier.Public | Modifier.Static,
                type = new TypeInfo { userType = cname },
                args = new Arguments().Add(typeof(DataRow), "row"),
                IsExtensionMethod = false
            };
            clss.Add(mtdNewObject);
            sent = mtdNewObject.statements;
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
                modifier = Modifier.Public | Modifier.Static,
                type = new TypeInfo { type = typeof(DataTable) }
            };
            clss.Add(method);
            sent = method.statements;
            sent.AppendLine("DataTable dt = new DataTable();");
            foreach (DataColumn column in dt.Columns)
            {
                Type ty = dict[column].type;
                var NAME = COLUMN(column);
                sent.AppendLine($"dt.Columns.Add(new DataColumn({NAME}, typeof({ty})));");
            }
            sent.AppendLine();
            sent.AppendLine("return dt;");


            method = new Method(new TypeInfo { type = typeof(string) }, "ToString")
            {
                modifier = Modifier.Public | Modifier.Override
            };
            clss.Add(method);
            sent = method.statements;


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
                Field field = new Field(new TypeInfo { type = typeof(string) }, COLUMN(column), new Value(column.ColumnName))
                {
                    modifier = Modifier.Public | Modifier.Const
                };
                clss.Add(field);
            }

        }

    }
}
