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
        private TableName tname;
        private DataTable dt;

        private IDictionary<DataColumn, TypeInfo> dict { get; }

        public DataContract2ClassBuilder(ApplicationCommand cmd, TableName tname, DataTable dt, bool allowDbNull)
            : base(cmd)
        {
            this.tname = tname;
            this.dt = dt;
            this.dict = CreateMapOfTypeInfo(dt, allowDbNull);

            builder.AddUsing("System");
            builder.AddUsing("System.Collections.Generic");
            builder.AddUsing("System.Data");
            builder.AddUsing("System.Linq");
            builder.AddUsing("Sys.Data");

            AddOptionalUsing();
        }

        public static IDictionary<DataColumn, TypeInfo> CreateMapOfTypeInfo(DataTable dt, bool allowDbNull)
        {
            Dictionary<DataColumn, TypeInfo> dict = new Dictionary<DataColumn, TypeInfo>();

            foreach (DataColumn column in dt.Columns)
            {
                TypeInfo ty = new TypeInfo { Type = column.DataType };

                if (allowDbNull)
                {
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
                }

                dict.Add(column, ty);
            }

            return dict;
        }


        protected override void CreateClass()
        {

            var clss = new Class(ClassName, new TypeInfo { Type = typeof(IDataContractRow) }, new TypeInfo { UserType = $"IEquatable<{ClassName}>" })
            {
                Modifier = Modifier.Public | Modifier.Partial
            };
            builder.AddClass(clss);


            foreach (DataColumn column in dt.Columns)
            {
                clss.Add(new Property(dict[column], PropertyName(column)) { Modifier = Modifier.Public });
            }

            if (ContainsMethod("FillObject"))
                Method_FillObject(clss);
            if (ContainsMethod("UpdateRow"))
                Method_UpdateRow(clss);
            if (ContainsMethod("CopyTo"))
                Method_CopyTo(clss);
            if (ContainsMethod("Equals"))
                Method_Equals(clss);
            if (ContainsMethod("NewObject"))
                Method_NewObject(clss);
            if (ContainsMethod("CreateTable"))
                Method_CreateTable(clss);
            //Method_CRUD(dt, clss);
            if (ContainsMethod("ToString"))
                Method_ToString(clss);


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

        private void Method_FillObject(Class clss)
        {
            Method mtdFillObject = new Method("FillObject")
            {
                Modifier = Modifier.Public,
                Params = new Parameters().Add(typeof(DataRow), "row")
            };
            clss.Add(mtdFillObject);
            var sent = mtdFillObject.Statement;

            foreach (DataColumn column in dt.Columns)
            {
                var type = dict[column];
                var NAME = COLUMN(column);
                var name = PropertyName(column);

                var line = $"this.{name} = row.Field<{type}>({NAME});";
                sent.AppendLine(line);
            }
        }

        private void Method_UpdateRow(Class clss)
        {
            Method mtdUpdateRow = new Method("UpdateRow")
            {
                Modifier = Modifier.Public,
                Params = new Parameters().Add(typeof(DataRow), "row")
            };
            clss.Add(mtdUpdateRow);
            var sent = mtdUpdateRow.Statement;
            foreach (DataColumn column in dt.Columns)
            {
                var type = dict[column];
                var NAME = COLUMN(column);
                var name = PropertyName(column);

                var line = $"row.SetField({NAME}, this.{name});";
                sent.AppendLine(line);
            }
        }

        private void Method_CopyTo(Class clss)
        {
            Method mtdCopyTo = new Method("CopyTo")
            {
                Modifier = Modifier.Public,
                Params = new Parameters().Add(ClassName, "obj")
            };
            clss.Add(mtdCopyTo);
            var sent = mtdCopyTo.Statement;

            foreach (DataColumn column in dt.Columns)
            {
                var type = dict[column];
                var NAME = COLUMN(column);
                var name = PropertyName(column);

                var line = $"obj.{name} = this.{name};";
                sent.AppendLine(line);
            }
        }

        private void Method_Equals(Class clss)
        {
            Method mtdEquals = new Method("Equals")
            {
                Modifier = Modifier.Public,
                Type = new TypeInfo { Type = typeof(bool) },
                Params = new Parameters().Add(ClassName, "obj")
            };
            clss.Add(mtdEquals);
            Statement sent = mtdEquals.Statement;
            sent.AppendLine("return ");
            IEnumerable<string> variables = dict.Keys.Select(column => PropertyName(column));
            variables.ForEach(
                variable => sent.Append($"this.{variable} == obj.{variable}"),
                variable => sent.AppendLine("&& ")
            );

            sent.Append(";");
        }

        private void Method_NewObject(Class clss)
        {
            Method mtdNewObject = new Method("NewObject")
            {
                Modifier = Modifier.Public | Modifier.Static,
                Type = new TypeInfo { UserType = ClassName },
                Params = new Parameters().Add(typeof(DataRow), "row"),
                IsExtensionMethod = false
            };
            clss.Add(mtdNewObject);
            Statement sent = mtdNewObject.Statement;
            sent.AppendLine($"return new {ClassName}");
            sent.Begin();

            int count = dt.Columns.Count;
            int i = 0;
            foreach (DataColumn column in dt.Columns)
            {
                var type = dict[column];
                var NAME = COLUMN(column);
                var line = $"{PropertyName(column)} = row.Field<{type}>({NAME})";
                if (++i < count)
                    line += ",";

                sent.AppendLine(line);
            }
            sent.End(";");
        }

        private void Method_CreateTable(Class clss)
        {
            Method method = new Method("CreateTable")
            {
                Modifier = Modifier.Public | Modifier.Static,
                Type = new TypeInfo { Type = typeof(DataTable) }
            };
            clss.Add(method);

            Statement sent = method.Statement;
            sent.AppendLine("DataTable dt = new DataTable();");
            foreach (DataColumn column in dt.Columns)
            {
                Type ty = dict[column].Type;
                var NAME = COLUMN(column);
                sent.AppendLine($"dt.Columns.Add(new DataColumn({NAME}, typeof({ty})));");
            }
            sent.AppendLine();
            sent.AppendLine("return dt;");
        }

        private void Method_ToString(Class clss)
        {
            Method method = new Method(new TypeInfo { Type = typeof(string) }, "ToString")
            {
                Modifier = Modifier.Public | Modifier.Override
            };
            clss.Add(method);
            Statement sent = method.Statement;

            IEnumerable<string> variables = dict.Keys.Select(column => PropertyName(column));
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

            CreateTableSchemaFields(tname, dt, clss);
            clss.AppendLine();
        }

        public void Method_CRUD(DataTable dt, Class clss)
        {
            var provider = ConnectionProviderManager.DefaultProvider;
            TableName tname = new TableName(provider, dt.TableName);

            SqlMaker gen = new SqlMaker(tname)
            {
                PrimaryKeys = dt.PrimaryKey.Select(x => x.ColumnName).ToArray()
            };

            foreach (DataColumn column in dt.Columns)
            {
                string cname = column.ColumnName;
                gen.Add(cname, "{" + cname + "}");
            }

            Method method = new Method("Insert")
            {
                Modifier = Modifier.Public,
                Type = new TypeInfo(typeof(string)),
            };
            method.Statement.AppendLine("return $\"" + gen.Insert() + "\";");
            clss.Add(method);

            method = new Method("Update")
            {
                Modifier = Modifier.Public,
                Type = new TypeInfo(typeof(string)),
            };
            method.Statement.AppendLine("return $\"" + gen.Update() + "\";");
            clss.Add(method);

            method = new Method("InsertOrUpdate")
            {
                Modifier = Modifier.Public,
                Type = new TypeInfo(typeof(string)),
            };
            method.Statement.AppendLine("return $\"" + gen.InsertOrUpdate() + "\";");
            clss.Add(method);

            method = new Method("Delete")
            {
                Modifier = Modifier.Public,
                Type = new TypeInfo(typeof(string)),
            };
            method.Statement.AppendLine("return $\"" + gen.Delete() + "\";");
            clss.Add(method);
        }
    }
}
