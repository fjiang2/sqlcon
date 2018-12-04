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

            var clss = new Class(cname, OptionalBaseType()) { modifier = Modifier.Public | Modifier.Partial };
            builder.AddClass(clss);

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

            UtilsThisMethod option = UtilsThisMethod.Undefined;

            string x = cmd.GetValue("method");
            if(x==null)
            {
                cerr.WriteLine("invalid option /method");
                return;
            }

            string[] methods = x.Split(',');

            if (methods.Contains("Copy"))
                option |= UtilsThisMethod.Copy;

            if (methods.Contains("Clone"))
                option |= UtilsThisMethod.Clone;

            if (methods.Contains("Equals"))
                option |= UtilsThisMethod.Equals;

            if (methods.Contains("GetHashCode"))
                option |= UtilsThisMethod.GetHashCode;

            if (methods.Contains("Compare"))
                option |= UtilsThisMethod.GetHashCode;

            if (methods.Contains("ToString"))
                option |= UtilsThisMethod.ToString;

            string[] columns;
            if (cmd.Has("identity"))
            {
                columns = schema.Columns
                .Select(column => column.ColumnName)
                .ToArray();
            }
            else
            {
                columns = schema.Columns
                    .Where(column => !column.IsIdentity)
                    .Select(column => column.ColumnName)
                    .ToArray();
            }

            clss.AddUtilsMethod(cname, columns, option);
        }


    }
}
