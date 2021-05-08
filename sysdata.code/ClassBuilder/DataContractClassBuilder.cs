using Sys;
using Sys.CodeBuilder;
using Sys.Data;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;
using System.Text;
using Sys.Data.Manager;
using Sys.Stdio;

namespace Sys.Data.Code
{

    public class DataContractClassBuilder : DataTableClassBuilder
    {

        public DataContractClassBuilder(IApplicationCommand cmd, TableName tname, DataTable dt, bool allowDbNull)
            : base(cmd, tname, dt, allowDbNull)
        {


            builder.AddUsing("System");
            builder.AddUsing("System.Collections.Generic");
            builder.AddUsing("System.Data");
            builder.AddUsing("System.Linq");
            builder.AddUsing("System.Runtime.Serialization");

            AddOptionalUsing();
        }

        protected override void CreateClass()
        {

            var clss = new Class(ClassName)
            {
                Modifier = Modifier.Public | Modifier.Partial
            };
            builder.AddClass(clss);

            clss.AddAttribute(new AttributeInfo("DataContract"));


            foreach (DataColumn column in dt.Columns)
            {
                var property = new Property(dict[column], column.ColumnName.ToFieldName())
                {
                    Modifier = Modifier.Public
                };

                property.AddAttribute(new AttributeInfo("DataMember", new
                {
                    Name = column.ColumnName,
                    EmitDefaultValue = false,
                }));

                clss.Add(property);
            }
        }

    }
}
