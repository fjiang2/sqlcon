using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using Sys;
using Sys.CodeBuilder;
using Sys.Data;
using Sys.Data.Manager;

namespace sqlcon
{
    class Linq2SQLClassBuilder
    {
        private TableName tname;

        public string ns { get; set; } = "Sys.Database";

        private string cname;


        public Linq2SQLClassBuilder(TableName tname)
        {
            this.tname = tname;
            this.cname = tname.ToClassName(null);
        }

        private Dictionary<DataColumn, TypeInfo> dict = new Dictionary<DataColumn, TypeInfo>();

        private CSharpBuilder CreateClass()
        {

            CSharpBuilder builder = new CSharpBuilder { nameSpace = ns, };
            var clss = new Class(cname)
            {
                modifier = Modifier.Public | Modifier.Partial
            };
            clss.AddAttribute(new AttributeInfo("Table", new { Name = tname.ShortName }));

            builder.AddClass(clss);

            builder.AddUsing("System");
            //builder.AddUsing("System.Collections.Generic");
            //builder.AddUsing("System.Data");
            builder.AddUsing("System.Data.Linq");
            builder.AddUsing("System.Data.Linq.Mapping");

            TableSchema schema = new TableSchema(tname);

            Property prop;
            foreach (IColumn column in schema.Columns)
            {
                TypeInfo ty = new TypeInfo { userType = ColumnSchema.GetFieldType(column.DataType, column.Nullable) };

                prop = new Property(ty, column.ToFieldName()) { modifier = Modifier.Public };

                List<object> args = new List<object>();
                args.Add(new { Name = column.ColumnName });

                //args.Add(new { DbType = ColumnSchema.GetSQLType(column) + (column.Nullable ? " NULL" : " NOT NULL") });

                if (column.IsPrimary)
                    args.Add(new { IsPrimaryKey = true });

                if (column.IsIdentity)
                    args.Add(new { IsDbGenerated = true });

                if (!column.Nullable)
                    args.Add(new { CanBeNull = false });

                prop.AddAttribute(new AttributeInfo("Column", args.ToArray()));

                if (!column.IsComputed)
                    clss.Add(prop);

            }

            var fks = schema.ForeignKeys;
            List<Property> list = new List<Property>();
            foreach (var key in fks.Keys)
            {
                string cname = new TableName(tname.DatabaseName, key.PK_Schema, key.PK_Table).ToClassName(null);
                string pname = cname;
                if (cname == this.cname) //self-fk
                    pname += "1";

                var field = new Field(new TypeInfo { userType = $"EntityRef<{cname}>" }, $"_{pname}") { modifier = Modifier.Private };
                clss.Add(field);

                prop = new Property(new TypeInfo { userType = cname }, pname) { modifier = Modifier.Public };
                prop.gets.AppendFormat("return this._{0}.Entity;", pname);
                prop.sets.AppendFormat("this._{0}.Entity = value;", pname);
                prop.AddAttribute(new AttributeInfo("Association", new { Name = $"{cname}_{this.cname}", Storage = $"_{pname}", ThisKey = key.FK_Column, OtherKey = key.PK_Column, IsForeignKey = true }));
                list.Add(prop);
            }

            foreach (var p in list)
                clss.Add(p);

            return builder;
        }

        public string WriteFile(string path)
        {
            var builder = CreateClass();

            string code = $"{ builder}";
            string file = Path.ChangeExtension(Path.Combine(path, cname), "cs");
            code.WriteIntoFile(file);

            return file;
        }
    }
}
