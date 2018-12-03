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

            UtilsThisMethod option = UtilsThisMethod.ToString;

            if (cmd.Has("Copy"))
                option |= UtilsThisMethod.Copy;

            if (cmd.Has("Clone"))
                option |= UtilsThisMethod.Clone;

            if (cmd.Has("Equals"))
                option |= UtilsThisMethod.Equals;

            if (cmd.Has("GetHashCode"))
                option |= UtilsThisMethod.GetHashCode;

            if (cmd.Has("Compare"))
                option |= UtilsThisMethod.GetHashCode;

            clss.AddUtilsMethod(cname, schema.Columns.Select(column => column.ColumnName), option);
        }


    }
}
