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

namespace sqlcon
{

    class DataContractClassBuilder : TheClassBuilder
    {
        private DataTable dt;

        public string mtd { get; set; }


        public DataContractClassBuilder(Command cmd, DataTable dt)
            : base(cmd)
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


            builder.AddUsing("System");
            builder.AddUsing("System.Collections.Generic");
            builder.AddUsing("System.Data");
            builder.AddUsing("System.Linq");
            builder.AddUsing("System.Runtime.Serialization");

            AddOptionalUsing();
        }

        private Dictionary<DataColumn, TypeInfo> dict = new Dictionary<DataColumn, TypeInfo>();

        protected override void CreateClass()
        {

            var clss = new Class(cname)
            {
                modifier = Modifier.Public | Modifier.Partial
            };
            builder.AddClass(clss);

            clss.AddAttribute(new AttributeInfo("DataContract"));


            foreach (DataColumn column in dt.Columns)
            {
                var property = new Property(dict[column], column.ColumnName.ToFieldName())
                {
                    modifier = Modifier.Public
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
