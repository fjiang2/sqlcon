using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using Sys;
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

        private CSharpBuilder CreateDataContractExtension(CSharpBuilder builder)
        {
            int i;
            int count;
            Statement sent;

            var clss = new Class(cname + "Extension") { modifier = Modifier.Public | Modifier.Static };
            builder.AddClass(clss);

            Func<DataColumn, string> COLUMN = column => "_" + column.ColumnName.ToUpper();

            //Const Field
            foreach (DataColumn column in dt.Columns)
            {
                Field field = new Field(new TypeInfo { type = typeof(string) }, COLUMN(column), column.ColumnName)
                {
                    modifier = Modifier.Public | Modifier.Const
                };
                clss.Add(field);
            }

          
            Method method = new Method($"To{cname}Collection")
            {
                modifier = Modifier.Public | Modifier.Static,
                type = new TypeInfo { userType = $"List<{cname}>" },
                args = new Arguments().Add(typeof(DataTable), "dt"),
                IsExtensionMethod = true
            };
            clss.Add(method);
            sent = method.statements;
            sent.AppendLine("return dt.AsEnumerable()");
            sent.AppendLine(".Select(row => NewObject(row))");
            sent.AppendLine(".ToList();");



            {
                Method method0 = new Method("NewObject")
                {
                    modifier = Modifier.Public | Modifier.Static,
                    type = new TypeInfo { userType = cname },
                    args = new Arguments().Add(typeof(DataRow), "row"),
                    IsExtensionMethod = false
                };
                clss.Add(method0);
                sent = method0.statements;
                sent.AppendLine($"return new {cname}");
                sent.Begin();

                count = dt.Columns.Count;
                i = 0;
                foreach (DataColumn column in dt.Columns)
                {
                    var type = dict[column];
                    var name = COLUMN(column);
                    var line = $"{column.ColumnName} = row.Field<{type}>({name})";
                    if (++i < count)
                        line += ",";

                    sent.AppendLine(line);
                }
                sent.End(";");

                Method method1 = new Method("UpdateObject")
                {
                    modifier = Modifier.Public | Modifier.Static,
                    args = new Arguments().Add(typeof(DataRow), "row").Add(cname, "item"),
                    IsExtensionMethod = true
                };
                clss.Add(method1);

                Method method2 = new Method("UpdateRow")
                {
                    modifier = Modifier.Public | Modifier.Static,
                    args = new Arguments().Add(cname, "item").Add(typeof(DataRow), "row"),
                    IsExtensionMethod = true
                };
                clss.Add(method2);

                var sent1 = method1.statements;
                var sent2 = method2.statements;
                foreach (DataColumn column in dt.Columns)
                {
                    var type = dict[column];
                    var name = COLUMN(column);
                    var line = $"item.{column.ColumnName} = row.Field<{type}>({name});";
                    if (++i < count)
                        line += ",";

                    sent1.AppendLine(line);

                    line = $"row.SetField({name}, item.{column.ColumnName});";
                    sent2.AppendLine(line);
                }
            }


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
                var name = COLUMN(column);
                sent.AppendLine($"dt.Columns.Add(new DataColumn({name}, typeof({ty})));");
            }
            sent.AppendLine();
            sent.AppendLine("return dt;");



            method = new Method(mtd2)
            {
                modifier = Modifier.Public | Modifier.Static,
                args = new Arguments().Add($"IEnumerable<{cname}>", "items").Add(typeof(DataTable), "dt"),
                IsExtensionMethod = true
            };
            clss.Add(method);
            sent = method.statements;
            sent.AppendLine("foreach (var item in items)");
            sent.Begin();
            sent.AppendLine("var row = dt.NewRow();");
            sent.AppendLine("UpdateRow(item, row);");
            sent.AppendLine("dt.Rows.Add(row);");
            sent.End();
            sent.AppendLine("dt.AcceptChanges();");


            method = new Method(mtd2)
            {
                modifier = Modifier.Public | Modifier.Static,
                type = new TypeInfo { type = typeof(DataTable) },
                args = new Arguments().Add($"IEnumerable<{cname}>", "items"),
                IsExtensionMethod = true
            };
            clss.Add(method);
            sent = method.statements;
            sent.AppendLine("var dt = CreateTable();");
            sent.AppendLine("ToDataTable(items, dt);");
            sent.AppendLine("return dt;");
            sent = method.statements;


            method = new Method("ForEach")
            {
                modifier = Modifier.Public | Modifier.Static,
                args = new Arguments().Add(typeof(DataTable), "dt").Add($"Action<{cname}>", "action"),
                IsExtensionMethod = true
            };
            clss.Add(method);
            sent = method.statements;
            sent.AppendLine("foreach (DataRow row in dt.Rows)");
            sent.Begin();
            sent.AppendLine("var item = NewObject(row);");
            sent.AppendLine("action(item);");
            sent.AppendLine("UpdateRow(item, row);");
            sent.End();

            method = new Method("ForEach<T>")
            {
                modifier = Modifier.Public | Modifier.Static,
                type = new TypeInfo { userType = "IEnumerable<T>" },
                args = new Arguments().Add(typeof(DataTable), "dt").Add($"Func<{cname},T>", "func"),
                IsExtensionMethod = true
            };
            clss.Add(method);
            sent = method.statements;
            sent.AppendLine("List<T> list = new List<T>();");
            sent.AppendLine("foreach (DataRow row in dt.Rows)");
            sent.Begin();
            sent.AppendLine("var item = NewObject(row);");
            sent.AppendLine(" T t = func(item);");
            sent.AppendLine("list.Add(t);");
            sent.End();
            sent.AppendLine("return list;");


            clss.AddCopyCloneCompareExtension(cname, dict.Keys.Select(column => column.ColumnName));
            return builder;
        }

        public string WriteFile(string path)
        {
            var builder = CreateDataContract();
            CreateDataContractExtension(builder);

            string code = $"{ builder}";
            string file = Path.ChangeExtension(Path.Combine(path, cname), "cs");
            code.WriteIntoFile(file);

            return file;
        }
    }
}
