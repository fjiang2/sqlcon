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
        public string cname { get; set; } = "DataContract";
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

        private CSharpBuilder CreateDataContract()
        {

            CSharpBuilder builder = new CSharpBuilder { nameSpace = ns, };
            var clss = new Class(cname) { modifier = Modifier.Public | Modifier.Partial };
            builder.AddClass(clss);

            builder.AddUsing("System");
            builder.AddUsing("System.Collections.Generic");
            builder.AddUsing("System.Data");
            builder.AddUsing("System.Linq");

            foreach (DataColumn column in dt.Columns)
            {
                clss.Add(new Property(dict[column], column.ColumnName) { modifier = Modifier.Public });
            }

            return builder;
        }

        private CSharpBuilder CreateReader(CSharpBuilder builder)
        {

            var clss = new Class(cname + "Extension") { modifier = Modifier.Public | Modifier.Static };
            builder.AddClass(clss);

            {
                if (mtd == null)
                    mtd = $"To{cname}Collection";

                Method method = new Method(mtd)
                {
                    modifier = Modifier.Public | Modifier.Static,
                    type = new TypeInfo { userType = $"IEnumerable<{cname}>" },
                    args = new Arguments().Add<DataTable>("dt"),
                    IsExtensionMethod = true
                };
                clss.Add(method);
                var sent = method.statements;

                sent.AppendLine("return dt.AsEnumerable()");
                sent.AppendLine($".Select(row => new {cname}");
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
                    args = new Arguments().Add($"IEnumerable<{cname}>", "items"),
                    IsExtensionMethod = true
                };
                clss.Add(method);

                var sent = method.statements;
                sent.AppendLine("DataTable dt = new DataTable();");
                foreach (DataColumn column in dt.Columns)
                {
                    Type ty = dict[column].type;
                    sent.AppendLine($"dt.Columns.Add(new DataColumn(\"{column.ColumnName}\", typeof({ty})));");
                }

                method.statements.AppendLine();

                sent.AppendLine("foreach (var item in items)");
                sent.Begin();
                sent.AppendLine("var row = dt.NewRow();");
                foreach (DataColumn column in dt.Columns)
                {
                    var ty = dict[column];
                    var line = $"row.SetField(\"{column.ColumnName}\", item.{column.ColumnName});";
                    sent.AppendLine(line);
                }
                sent.AppendLine("dt.Rows.Add(row);");
                sent.End();

                sent.AppendLine("dt.AcceptChanges();");
                sent.AppendLine("return dt;");
            }


            clss.AddCopyCloneCompareExtension(cname, dict.Keys.Select(column=>column.ColumnName));
            return builder;
        }

        public string WriteFile(string path)
        {
            var builder = CreateDataContract();
            CreateReader(builder);

            string code = $"{ builder}";
            string file = Path.ChangeExtension(Path.Combine(path, cname), "cs");
            using (var writer = file.NewStreamWriter())
            {
                writer.WriteLine(code);
            }

            return file;
        }
    }
}
