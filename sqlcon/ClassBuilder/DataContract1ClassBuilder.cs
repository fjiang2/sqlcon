using Sys;
using Sys.CodeBuilder;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace sqlcon
{
    class DataContract1ClassBuilder : TheClassBuilder
    {

        private DataTable dt;

        public string mtd { get; set; }
        public string[] keys { get; set; }

        private const string _ToDataTable = "ToDataTable";
        private bool isReadOnly = false;

        public DataContract1ClassBuilder(ApplicationCommand cmd, DataTable dt)
            : base(cmd)
        {
            this.dt = dt;
            if (cmd.Has("readonly"))
                isReadOnly = true;

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
            AddOptionalUsing();

        }

        private Dictionary<DataColumn, TypeInfo> dict = new Dictionary<DataColumn, TypeInfo>();


        protected override void CreateClass()
        {
            var clss = new Class(cname) { modifier = Modifier.Public | Modifier.Partial };

            builder.AddClass(clss);


            foreach (DataColumn column in dt.Columns)
            {
                clss.Add(new Property(dict[column], column.ColumnName) { modifier = Modifier.Public });
            }


            int i;
            int count;
            Statement sent;

            clss = new Class(cname + "Extension") { modifier = Modifier.Public | Modifier.Static };
            builder.AddClass(clss);

            Func<DataColumn, string> COLUMN = column => $"_{column.ColumnName.ToUpper()}";


            //Const Field
            Field field;


            if (dt.TableName != null)
            {
                field = new Field(new TypeInfo { type = typeof(string) }, "TableName", new Value(dt.TableName))
                {
                    modifier = Modifier.Public | Modifier.Const
                };
                clss.Add(field);
            }

            //primary keys
            var pk = dt.Columns
                .Cast<DataColumn>()
                .Where(column => keys.Select(key => key.ToUpper()).Contains(column.ColumnName.ToUpper()))
                .ToArray();

            if (pk.Length == 0)
                pk = new DataColumn[] { dt.Columns[0] };

            string pks = string.Join(", ", pk.Select(key => COLUMN(key)));
            field = new Field(new TypeInfo { type = typeof(string[]) }, "Keys")
            {
                modifier = Modifier.Public | Modifier.Static | Modifier.Readonly,
                userValue = $"new string[] {LP} {pks} {RP}"
            };
            clss.Add(field);

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
            string _GetField = "Field";
            if (mtd != null)
                _GetField = mtd;

            foreach (DataColumn column in dt.Columns)
            {
                var type = dict[column];
                var name = COLUMN(column);
                var line = $"{column.ColumnName} = row.{_GetField}<{type}>({name})";
                if (++i < count)
                    line += ",";

                sent.AppendLine(line);
            }
            sent.End(";");

            if (!isReadOnly)
            {
                Method method1 = new Method("FillObject")
                {
                    modifier = Modifier.Public | Modifier.Static,
                    args = new Arguments().Add(cname, "item").Add(typeof(DataRow), "row"),
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
                    var line = $"item.{column.ColumnName} = row.{_GetField}<{type}>({name});";
                    if (++i < count)
                        line += ",";

                    sent1.AppendLine(line);

                    line = $"row.SetField({name}, item.{column.ColumnName});";
                    sent2.AppendLine(line);
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


                method = new Method(_ToDataTable)
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


                method = new Method(_ToDataTable)
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
            }

            method = new Method("ToDictionary")
            {
                modifier = Modifier.Public | Modifier.Static,
                type = new TypeInfo { type = typeof(IDictionary<string, object>) },
                args = new Arguments().Add(cname, "item"),
                IsExtensionMethod = true
            };
            clss.Add(method);
            sent = method.statements;
            sent.AppendLine("return new Dictionary<string,object>() ");
            sent.Begin();
            count = dt.Columns.Count;
            i = 0;
            foreach (DataColumn column in dt.Columns)
            {
                Type ty = dict[column].type;
                var name = COLUMN(column);
                var line = $"[{name}] = item.{column.ColumnName}";
                if (++i < count)
                    line += ",";

                sent.AppendLine(line);
            }
            sent.End(";");


            method = new Method("FromDictionary")
            {
                modifier = Modifier.Public | Modifier.Static,
                type = new TypeInfo { userType = cname },
                args = new Arguments().Add(typeof(IDictionary<string, object>), "dict"),
                IsExtensionMethod = true
            };
            clss.Add(method);
            sent = method.statements;
            sent.AppendLine($"return new {cname}");
            sent.Begin();
            count = dt.Columns.Count;
            i = 0;
            foreach (DataColumn column in dt.Columns)
            {
                var type = dict[column];
                var name = COLUMN(column);
                var line = $"{column.ColumnName} = ({type})dict[{name}]";
                if (++i < count)
                    line += ",";

                sent.AppendLine(line);
            }
            sent.End(";");


            clss.AddUtilsMethod(cname, dict.Keys.Select(column => new PropertyInfo { PropertyName = column.ColumnName }), UtilsStaticMethod.CopyTo | UtilsStaticMethod.CompareTo | UtilsStaticMethod.ToSimpleString);
            clss.AppendLine();

            foreach (DataColumn column in dt.Columns)
            {
                field = new Field(new TypeInfo { type = typeof(string) }, COLUMN(column), new Value(column.ColumnName))
                {
                    modifier = Modifier.Public | Modifier.Const
                };
                clss.Add(field);
            }
        }
    }
}
