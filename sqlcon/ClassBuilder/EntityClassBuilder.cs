using Sys;
using Sys.CodeBuilder;
using Sys.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;

namespace sqlcon
{

    class EntityClassBuilder : TheClassBuilder
    {

        private TableName tname;


        public EntityClassBuilder(Command cmd, TableName tname)
            : base(cmd)
        {
            this.tname = tname;
            this.cname = tname.Name;

            builder.AddUsing("System");
            builder.AddUsing("System.Collections.Generic");
            builder.AddUsing("System.Data");
            builder.AddUsing("System.Linq");

            AddOptionalUsing();
        }

        protected override void CreateClass()
        {

            TableSchema schema = new TableSchema(tname);
            Func<IColumn, string> COLUMN = column => "_" + column.ColumnName.ToUpper();

            TypeInfo[] baseClass = OptionalBaseType();

            var clss = new Class(cname, OptionalBaseType())
            {
                modifier = Modifier.Public | Modifier.Partial
            };

            builder.AddClass(clss);

            string optionField = cmd.GetValue("field");
            if (optionField != null)
            {
                string[] fields = optionField.Split(',');

                if (fields.Contains("const"))
                {
                    //Const Field
                    Field field;
                    foreach (var column in schema.Columns)
                    {
                        field = new Field(new TypeInfo { type = typeof(string) }, COLUMN(column), new Value(column.ColumnName))
                        {
                            modifier = Modifier.Public | Modifier.Const
                        };
                        clss.Add(field);
                    }
                }
            }


            UtilsThisMethod option = UtilsThisMethod.Undefined;

            string optionMethod = cmd.GetValue("method");
            if (optionMethod == null)
            {
                cerr.WriteLine("invalid option /method");
                return;
            }

            string[] methods = optionMethod.Split(',');

            if (methods.Contains("Map"))
            {
                string identityColumn = schema.Columns
                    .Where(column => column.IsIdentity)
                    .Select(column => column.ColumnName)
                    .FirstOrDefault();

                if (identityColumn == null && schema.Columns[0].CType == CType.Int)
                    identityColumn = schema.Columns[0].ColumnName;

                if (identityColumn != null)
                {
                    const string IdentityName = "Identity";
                    Property identity = new Property(new TypeInfo(typeof(int)), IdentityName)
                    {
                        IsLambda = true,
                    };

                    var attr = base.Attributes;
                    if (attr.ContainsKey(IdentityName))
                    {
                        foreach (string x in attr[IdentityName])
                            identity.AddAttribute(new AttributeInfo(x));
                    }

                    identity.gets.Append($"this.{identityColumn};");
                    clss.Add(identity);
                }

                //identity column excluded
                PropertyInfo[] columns = schema.Columns
                    .Where(column => !column.IsIdentity)
                    .Select(column => new PropertyInfo { PropertyName = column.ColumnName })
                    .ToArray();

                clss.AddUtilsMethod(cname, columns, UtilsThisMethod.Map);
            }

            if (methods.Contains("Copy"))
                option |= UtilsThisMethod.Copy;

            if (methods.Contains("Clone"))
                option |= UtilsThisMethod.Clone;

            if (methods.Contains("Equals"))
                option |= UtilsThisMethod.Equals;

            if (methods.Contains("GetHashCode"))
                option |= UtilsThisMethod.GetHashCode;

            if (methods.Contains("Compare"))
                option |= UtilsThisMethod.Compare;

            if (methods.Contains("ToDictionary"))
                option |= UtilsThisMethod.ToDictionary;

            if (methods.Contains("ToString"))
                option |= UtilsThisMethod.ToString;

            {
                PropertyInfo[] columns = schema.Columns
                    .Select(column => new PropertyInfo
                    {
                        PropertyType = column.GetTypeInfo(),
                        PropertyName = column.ColumnName
                    })
                    .ToArray();

                clss.AddUtilsMethod(cname, columns, option);
            }
        }
    }
}
