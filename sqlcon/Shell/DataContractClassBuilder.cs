using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using Sys.CodeBuilder;

namespace sqlcon
{
    class DataContractClassBuilder
    {
        private DataTable dt;

        public string ns { get; set; } = "Sys.DataContracts";
        public string cn { get; set; } = "DataContract";
        public string mtd { get; set; }
        private const string mtd2 = "ToDataTable";


        public DataContractClassBuilder(DataTable dt)
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

        }

        private Dictionary<DataColumn, TypeInfo> dict = new Dictionary<DataColumn, TypeInfo>();

        private ClassBuilder CreateDataContract()
        {

            ClassBuilder builder = new ClassBuilder { nameSpace = ns, };
            var clss = new Class(cn) { modifier = Modifier.Public | Modifier.Partial };
            builder.AddClass(clss);

            builder.AddUsing("System");
            builder.AddUsing("System.Collections.Generic");
            builder.AddUsing("System.Data");
            builder.AddUsing("System.Linq");

            foreach (DataColumn column in dt.Columns)
            {
                clss.AddProperty(new Property(dict[column], column.ColumnName) { modifier = Modifier.Public });
            }

            return builder;
        }

        private ClassBuilder CreateReader()
        {

            ClassBuilder builder = new ClassBuilder { nameSpace = ns, };
            var clss = new Class(cn + "Extension") { modifier = Modifier.Public | Modifier.Partial };
            builder.AddClass(clss);

            {
                if (mtd == null)
                    mtd = $"To{cn}Collection";

                Method method = new Method(mtd)
                {
                    modifier = Modifier.Public | Modifier.Static,
                    type = new TypeInfo { userType = $"IEnumerable<{cn}>" },
                    args = new Arguments().Add<DataTable>("dt"),
                    IsExtensionMethod = true
                };
                clss.AddMethod(method);
                var sent = method.statements;

                sent.AppendLine("return dt.AsEnumerable()");
                sent.AppendLine($".Select(row => new {cn}");
                sent.Begin();

                int count = dt.Columns.Count;
                int i = 0;
                foreach (DataColumn column in dt.Columns)
                {
                    var type = dict[column];
                    var line = $"{column.ColumnName} = row.Field<{type}>(\"{column.ColumnName}\")";
                    if (++i < count)
                        line += ",";

                    sent.AppendLine(line);
                }

                sent.End(");");
            }

            {
                Method method = new Method(mtd2)
                {
                    modifier = Modifier.Public | Modifier.Static,
                    type = new TypeInfo { type = typeof(DataTable) },
                    args = new Arguments().Add($"IEnumerable<{cn}>", "items"),
                    IsExtensionMethod = true
                };
                clss.AddMethod(method);

                var sent = method.statements;
                sent.AppendLine("DataTable dt = new DataTable();");
                foreach (DataColumn column in dt.Columns)
                {
                    Type ty = dict[column].type;
                    sent.AppendLine($"dt.Columns.Add(new DataColumn(\"{column.ColumnName}\",typeof({ty})));");
                }

                method.statements.AppendLine();

                sent.AppendLine("foreach(var item in items)");
                sent.Begin();
                sent.AppendLine("var row = dt.NewRow();");
                foreach (DataColumn column in dt.Columns)
                {
                    var ty = dict[column];
                    var line = $"row[\"{column.ColumnName}\"] = item.{column.ColumnName};";
                    sent.AppendLine(line);
                }
                sent.AppendLine("dt.Rows.Add(row);");
                sent.End();

                sent.AppendLine("dt.AcceptChanges();");
                sent.AppendLine("return dt;");
            }

            //Property prop = new Property(new TypeInfo { type = typeof(int) }, "Text");
            //prop.gets.AppendLine("var i=2;");
            //prop.gets.AppendLine("return this.text;");
            //prop.sets.AppendLine("this.text = value;");
            //builder.AddProperty(prop);

            return builder;
        }

        public string WriteFile(string path)
        {
            var builder1 = CreateDataContract();
            var builder2 = CreateReader();

            string code = $"{ builder1}\r\n{builder2}";
            string file = Path.ChangeExtension(Path.Combine(path, cn), "cs");
            using (var writer = file.NewStreamWriter())
            {
                writer.WriteLine(code);
            }

            return file;
        }
    }
}
