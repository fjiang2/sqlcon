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
        public string clss { get; set; } = "DataContract";
        public string mtd { get; set; } =  "ToEnumerable";
        public string mtd2 { get; set; } = "ToDataTable";


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

            ClassBuilder builder = new ClassBuilder(clss)
            {
                nameSpace = ns,
                modifier = AccessModifier.Public | AccessModifier.Partial
            };

            builder.AddUsing("System");
            builder.AddUsing("System.Collections.Generic");
            builder.AddUsing("System.Data");
            builder.AddUsing("System.Linq");

            foreach (DataColumn column in dt.Columns)
            {
                builder.AddProperty(new Property(dict[column], column.ColumnName) { modifier = AccessModifier.Public });
            }

            return builder;
        }

        private ClassBuilder CreateReader()
        {
            ClassBuilder builder = new ClassBuilder(clss + "Reader")
            {
                nameSpace = ns,
                modifier = AccessModifier.Public | AccessModifier.Static
            };

            {
                Method method = new Method(mtd)
                {
                    modifier = AccessModifier.Public | AccessModifier.Static,
                    type = new TypeInfo { userType = $"IEnumerable<{clss}>" },
                    args = new Argument[] { new Argument(new TypeInfo { type = typeof(DataTable) }, "dt") }
                };
                builder.AddMethod(method);

                Statement sent = new Statement();
                sent.Append("return dt.AsEnumerable()");
                sent.Append($".Select(row => new {clss}");
                sent.Append("{");

                int count = dt.Columns.Count;
                int i = 0;
                sent.Indent(true);
                foreach (DataColumn column in dt.Columns)
                {
                    var type = dict[column];
                    var line = $"{column.ColumnName} = row.Field<{type}>(\"{column.ColumnName}\")";
                    if (++i < count)
                        line += ",";

                    sent.Add(line);
                }
                sent.Indent(false);
                sent.Append("})");

                method.AddStatement(sent);
            }

            {
                Method method = new Method(mtd2)
                {
                    modifier = AccessModifier.Public | AccessModifier.Static,
                    type = new TypeInfo { type = typeof(DataTable) },
                    args = new Argument[] { new Argument( new TypeInfo { userType = $"IEnumerable<{clss}>" }, "items") }
                };
                builder.AddMethod(method);

                Statement sent = new Statement();
                sent.Append("DataTable dt = new DataTable();");
                foreach (DataColumn column in dt.Columns)
                {
                    Type ty = dict[column].type;
                    sent.Append($"dt.Columns.Add(new DataColumn(\"{column.ColumnName}\",typeof({ty})));");
                }

                method.AddLine();

                sent.Append("foreach(var item in items)");
                sent.Append("{");
                sent.Indent(true);
                sent.Append("var row = dt.NewRow();");
                foreach (DataColumn column in dt.Columns)
                {
                    var ty = dict[column];
                    var line = $"row[\"{column.ColumnName}\"] = item.{column.ColumnName};";
                    sent.Add(line);
                    //if (ty.Nullable)
                    //{
                    //    sent.Add($"if(item.{column.ColumnName} == null)");
                    //    sent.Add($"\trow[\"{column.ColumnName}\"] = DBNull.Value;");
                    //}
                }
                sent.Append("dt.Rows.Add(row);");
                sent.Indent(false);
                sent.Append("}");

                method.AddStatement(sent);
                method.AddStatement("dt.AcceptChanges()");
                method.AddStatement("return dt");
            }
            return builder;
        }

        public string WriteFile(string path)
        {
            var builder1 = CreateDataContract();
            var builder2 = CreateReader();

            string code = $"{ builder1}\r\n{builder2}";
            string file = Path.ChangeExtension(Path.Combine(path, clss), "cs");
            using (var writer = file.NewStreamWriter())
            {
                writer.WriteLine(code);
            }

            return file;
        }
    }
}
