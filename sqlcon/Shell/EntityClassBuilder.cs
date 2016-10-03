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
    public interface ICopiable<T>
    {
        void CopyFrom(T item);
    }


    class EntityClassBuilder
    {
        const string LP = "{";
        const string RP = "}";

        private TableName tname;

        public string ns { get; set; }
        public string cname { get; set; }
        public string _using { get; set; }
        public string _base { get; set; }


        public EntityClassBuilder(Command cmd, TableName tname)
        {
            this.tname = tname;
            this._using = cmd.GetValue("using");
            this._base = cmd.GetValue("base");

            cname = tname.Name;

        }


        private CSharpBuilder CreateDataContract()
        {
            CSharpBuilder builder = new CSharpBuilder { nameSpace = ns, };

            builder.AddUsing("System");
            builder.AddUsing("System.Collections.Generic");
            builder.AddUsing("System.Data");
            builder.AddUsing("System.Linq");

            if (_using != null)
            {
                string[] items = _using.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var item in items)
                {
                    builder.AddUsing(item);
                }
            }

            return builder;
        }

        private CSharpBuilder CreateDataContractExtension(CSharpBuilder builder)
        {
            TableSchema schema = new TableSchema(tname);
            Func<IColumn, string> COLUMN = column => "_" + column.ColumnName.ToUpper();

            List<TypeInfo> bases = new List<TypeInfo>();

            if (_base != null)
            {
                string[] items = _base.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string item in items)
                {
                    string type = item.Replace("~", cname);
                    bases.Add(new TypeInfo { userType = type });
                }
            }

            var clss = new Class(cname, bases.ToArray()) { modifier = Modifier.Public | Modifier.Partial };
            builder.AddClass(clss);

            //Const Field
            Field field;
            foreach (var column in schema.Columns)
            {
                field = new Field(new TypeInfo { type = typeof(string) }, COLUMN(column), column.ColumnName)
                {
                    modifier = Modifier.Public | Modifier.Const
                };
                clss.Add(field);
            }


            clss.AddCopyCloneEqualsFunc(cname, schema.Columns.Select(column => column.ColumnName));
            return builder;
        }

        public string WriteFile(string path)
        {
            var builder = CreateDataContract();
            CreateDataContractExtension(builder);

            string code = $"{ builder}";
            string file = Path.ChangeExtension(Path.Combine(path, cname), "cs");
            code.WriteIntoFile(file);

            return file;
        }
    }
}
