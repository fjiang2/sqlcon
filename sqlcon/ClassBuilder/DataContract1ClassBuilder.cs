﻿using Sys;
using Sys.CodeBuilder;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Sys.Data;

namespace sqlcon
{
    class DataContract1ClassBuilder : DataTableClassBuilder
    {
        private const string _ToDataTable = "ToDataTable";

        public DataContract1ClassBuilder(ApplicationCommand cmd, TableName tname, DataTable dt, bool allowDbNull)
            : base(cmd, tname, dt, allowDbNull)
        {
            this.tname = tname;
            this.dt = dt;

            builder.AddUsing("System");
            builder.AddUsing("System.Collections.Generic");
            builder.AddUsing("System.Data");
            builder.AddUsing("System.Linq");
            AddOptionalUsing();
        }

        protected override void CreateClass()
        {
            Class_TableSchema();
            Class clss3 = Class_Extension(out int index1, out int index2);

            Class clss2 = Class_Assoication(clss3, index1, index2);
            if (clss2.Index > 0)
                builder.AddClass(clss2);

            builder.AddClass(clss3);
        }

        private void Class_TableSchema()
        {
            var clss = new Class(ClassName) { Modifier = Modifier.Public | Modifier.Partial };

            builder.AddClass(clss);
            foreach (DataColumn column in dt.Columns)
            {
                clss.Add(new Property(dict[column], PropertyName(column)) { Modifier = Modifier.Public });
            }
        }

        private Class Class_Assoication(Class clss3, int index1, int index2)
        {
            Class clss = new Class(ClassName + ASSOCIATION) { Modifier = Modifier.Public };

            bool hasFK = cmd.HasForeignKey;
            bool hasAssoc = cmd.Has("assoc");
            if (hasAssoc)
                hasFK = true;

            if (hasFK)
            {
                var field = CreateConstraintField(tname);
                if (field != null)
                    clss3.Insert(index1, field);
            }

            if (hasAssoc)
            {
                var properties = base.CreateAssoicationClass(tname, clss);
                Method_Association(clss3, index2, properties);
            }

            return clss;
        }

        private Class Class_Extension(out int index1, out int index2)
        {
            Class clss = new Class(ClassName + EXTENSION) { Modifier = Modifier.Public | Modifier.Static };

            //Const Field
            CreateTableSchemaFields(tname, dt, clss);
            index1 = clss.Index;

            if (ContainsMethod("FillObject"))
            {
                Method_ToCollection(clss);
                Method_FillObject(clss);
                //if (ContainsMethod("NewObject"))
                //    Method_NewObject(clss);
            }
            if (ContainsMethod("UpdateRow"))
                Method_UpdateRow(clss);
            if (ContainsMethod("CreateTable"))
                Method_CreateTable(clss);
            if (ContainsMethod("ToDataTable"))
            {
                Method_ToDataTable1(clss);
                //Method_ToDataTable2(clss);
            }
            if (ContainsMethod("ToDictionary"))
                Method_ToDictionary(clss);
            if (ContainsMethod("FromDictionary"))
                Method_FromDictionary(clss);

            UtilsStaticMethod option = UtilsStaticMethod.Undefined;
            if (ContainsMethod("CopyTo"))
                option |= UtilsStaticMethod.CopyTo;

            if (ContainsMethod("CompareTo"))
                option |= UtilsStaticMethod.CompareTo;

            if (ContainsMethod("ToSimpleString"))
                option |= UtilsStaticMethod.ToSimpleString;

            clss.AddUtilsMethod(ClassName, dict.Keys.Select(column => new PropertyInfo { PropertyName = PropertyName(column) }), option);
            index2 = clss.Index;
            clss.AppendLine();

            Field field;
            foreach (DataColumn column in dt.Columns)
            {
                field = new Field(new TypeInfo { Type = typeof(string) }, COLUMN(column), new Value(column.ColumnName))
                {
                    Modifier = Modifier.Public | Modifier.Const
                };
                clss.Add(field);
            }

            return clss;
        }

        private void Method_ToCollection(Class clss)
        {
            Statement sent;
            Method method = new Method($"To{ClassName}Collection")
            {
                Modifier = Modifier.Public | Modifier.Static,
                Type = new TypeInfo { UserType = $"List<{ClassName}>" },
                Params = new Parameters().Add(typeof(DataTable), "dt"),
                IsExtensionMethod = true
            };
            clss.Add(method);
            sent = method.Statement;
            sent.AppendLine("return dt.AsEnumerable()");
            sent.AppendLine(".Select(row =>");
            sent.Begin();
            sent.AppendLine($"var obj = new {ClassName}();");
            sent.AppendLine("FillObject(obj, row);");
            sent.AppendLine("return obj;");
            sent.End(")");
            sent.AppendLine(".ToList();");
        }


        private void Method_FillObject(Class clss)
        {
            Method method = new Method("FillObject")
            {
                Modifier = Modifier.Public | Modifier.Static,
                Params = new Parameters().Add(ClassName, "item").Add(typeof(DataRow), "row"),
                IsExtensionMethod = true
            };
            clss.Add(method);
            var sent1 = method.Statement;
            foreach (DataColumn column in dt.Columns)
            {
                var type = dict[column];
                var name = COLUMN(column);
                var line = $"item.{PropertyName(column)} = row.{GetField}<{type}>({name});";

                sent1.AppendLine(line);
            }
        }

        private void Method_UpdateRow(Class clss)
        {
            Method method = new Method("UpdateRow")
            {
                Modifier = Modifier.Public | Modifier.Static,
                Params = new Parameters().Add(ClassName, "item").Add(typeof(DataRow), "row"),
                IsExtensionMethod = true
            };
            clss.Add(method);
            var sent = method.Statement;

            foreach (DataColumn column in dt.Columns)
            {
                var name = COLUMN(column);
                var line = $"row.SetField({name}, item.{PropertyName(column)});";
                sent.AppendLine(line);
            }
        }

      

        private void Method_ToDataTable1(Class clss)
        {
            Method method = new Method(_ToDataTable)
            {
                Modifier = Modifier.Public | Modifier.Static,
                Params = new Parameters().Add($"IEnumerable<{ClassName}>", "items").Add(typeof(DataTable), "dt"),
                IsExtensionMethod = true
            };
            clss.Add(method);
            Statement sent = method.Statement;
            sent.AppendLine("foreach (var item in items)");
            sent.Begin();
            sent.AppendLine("var row = dt.NewRow();");
            sent.AppendLine("UpdateRow(item, row);");
            sent.AppendLine("dt.Rows.Add(row);");
            sent.End();
            sent.AppendLine("dt.AcceptChanges();");
        }

        private void Method_ToDataTable2(Class clss)
        {
            Method method = new Method(_ToDataTable)
            {
                Modifier = Modifier.Public | Modifier.Static,
                Type = new TypeInfo { Type = typeof(DataTable) },
                Params = new Parameters().Add($"IEnumerable<{ClassName}>", "items"),
                IsExtensionMethod = true
            };
            clss.Add(method);
            Statement sent = method.Statement;
            sent.AppendLine("var dt = CreateTable();");
            sent.AppendLine("ToDataTable(items, dt);");
            sent.AppendLine("return dt;");
            sent = method.Statement;
        }

        private void Method_ToDictionary(Class clss)
        {
            Method method = new Method("ToDictionary")
            {
                Modifier = Modifier.Public | Modifier.Static,
                Type = new TypeInfo { Type = typeof(IDictionary<string, object>) },
                Params = new Parameters().Add(ClassName, "item"),
                IsExtensionMethod = true
            };
            clss.Add(method);
            Statement sent = method.Statement;
            sent.AppendLine("return new Dictionary<string,object>() ");
            sent.Begin();
            int count = dt.Columns.Count;
            int i = 0;
            foreach (DataColumn column in dt.Columns)
            {
                Type ty = dict[column].Type;
                var name = COLUMN(column);
                var line = $"[{name}] = item.{PropertyName(column)}";
                if (++i < count)
                    line += ",";

                sent.AppendLine(line);
            }
            sent.End(";");
        }

        private void Method_FromDictionary(Class clss)
        {
            Method method = new Method("FromDictionary")
            {
                Modifier = Modifier.Public | Modifier.Static,
                Type = new TypeInfo { UserType = ClassName },
                Params = new Parameters().Add(typeof(IDictionary<string, object>), "dict"),
                IsExtensionMethod = true
            };
            clss.Add(method);
            Statement sent = method.Statement;
            sent.AppendLine($"return new {ClassName}");
            sent.Begin();
            int count = dt.Columns.Count;
            int i = 0;
            foreach (DataColumn column in dt.Columns)
            {
                var type = dict[column];
                var name = COLUMN(column);
                var line = $"{PropertyName(column)} = ({type})dict[{name}]";
                if (++i < count)
                    line += ",";

                sent.AppendLine(line);
            }
            sent.End(";");
        }

        private Method Method_Association(Class clss, int index, List<AssociationPropertyInfo> properties)
        {
            string associationClassName = $"{ClassName}Association";
            Method method = new Method("GetAssociation")
            {
                Modifier = Modifier.Public | Modifier.Static,
                Type = new TypeInfo { UserType = associationClassName },
                Params = new Parameters().Add(ClassName, "entity"),
                IsExtensionMethod = true
            };
            Statement sent = method.Statement;
            sent.RETURN("entity.AsEnumerable().GetAssociation().FirstOrDefault()");
            clss.Insert(index++, method);


            method = new Method("GetAssociation")
            {
                Modifier = Modifier.Public | Modifier.Static,
                Type = new TypeInfo { UserType = $"IEnumerable<{associationClassName}>" },
                Params = new Parameters().Add($"IEnumerable<{ClassName}>", "entities"),
                IsExtensionMethod = true
            };
            clss.Insert(index++, method);

            sent = method.Statement;
            sent.AppendLine("var reader = entities.Expand();");
            sent.AppendLine();
            sent.AppendLine($"var associations = new List<{associationClassName}>();");
            sent.AppendLine();

            foreach (var property in properties)
            {
                sent.AppendLine($"var _{property.PropertyName} = reader.Read<{property.PropertyType}>();");
            }
            sent.AppendLine();

            sent.AppendLine("foreach (var entity in entities)");
            sent.Begin();
            sent.AppendLine($"var association = new {associationClassName}");
            sent.Begin();

            foreach (var p in properties)
            {
                if (p.OneToMany)
                    sent.AppendLine($"{p.PropertyName} = new EntitySet<{p.PropertyType}>(_{p.PropertyName}.Where(row => row.{p.FK_Column} == entity.{p.PK_Column})),");
                else
                    sent.AppendLine($"{p.PropertyName} = new EntityRef<{p.PropertyType}>(_{p.PropertyName}.FirstOrDefault(row => row.{p.FK_Column} == entity.{p.PK_Column})),");
            }

            sent.End(";");
            sent.AppendLine("associations.Add(association);");
            sent.End();

            sent.AppendLine();
            sent.AppendLine($"return associations;");
            return method;
        }
    }
}
