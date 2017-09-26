using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data;
using Sys;
using Sys.Data;
using Sys.CodeBuilder;

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


            clss.AddUtilsMethod(cname, schema.Columns.Select(column => column.ColumnName), UtilsMethodType.CopyFrom | UtilsMethodType.Clone | UtilsMethodType.Compare | UtilsMethodType.ToSimpleString);
        }


    }
}
