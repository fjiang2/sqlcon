using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using Sys;
using Sys.CodeBuilder;

namespace sqlcon
{
    abstract class TheClassBuilder : ClassMaker
    {
        protected CSharpBuilder builder;

        public TheClassBuilder(ApplicationCommand cmd)
            : base(cmd)
        {
            builder = new CSharpBuilder();
        }

        public void AddOptionalUsing()
        {
            builder.AddUsingRange(base.Usings);
        }

        public TypeInfo[] OptionalBaseType(params TypeInfo[] inherits)
        {
            List<TypeInfo> bases = new List<TypeInfo>(inherits);

            string _base = cmd.GetValue("base");
            if (_base != null)
            {
                string[] items = _base.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string item in items)
                {
                    string type = item.Replace("~", cname);
                    bases.Add(new TypeInfo { UserType = type });
                }
            }

            return bases.ToArray();
        }

        protected abstract void CreateClass();

        private void createClass()
        {
            builder.Namespace = NameSpace;
            CreateClass();
        }

        public string WriteFile(string path)
        {
            createClass();

            base.PrintOutput(builder, cname);
            string code = $"{builder}";
            string file = Path.ChangeExtension(Path.Combine(path, cname), "cs");
            code.WriteIntoFile(file);

            return file;
        }

        public void Done()
        {
            createClass();
            PrintOutput(builder, ClassName);
        }

        public static string COLUMN(DataColumn column) => $"_{column.ColumnName.ToUpper()}";


        public static void CreateTableNameAndPrimaryKey(DataTable dt, Class clss)
        {
            Field field;


            if (dt.TableName != null)
            {
                field = new Field(new TypeInfo { Type = typeof(string) }, "TableName", new Value(dt.TableName))
                {
                    Modifier = Modifier.Public | Modifier.Const
                };
                clss.Add(field);
            }

            //primary keys
            DataColumn[] pk = dt.PrimaryKey;

            string pks = string.Join(", ", pk.Select(key => COLUMN(key)));
            field = new Field(new TypeInfo { Type = typeof(string[]) }, "Keys")
            {
                Modifier = Modifier.Public | Modifier.Static | Modifier.Readonly,
                UserValue = $"new string[] {LP} {pks} {RP}"
            };

            clss.Add(field);

            DataColumn[] ik = dt.Columns.OfType<DataColumn>().Where(c => c.AutoIncrement).ToArray();
            if (ik.Length > 0)
            {
                string iks = string.Join(", ", ik.Select(key => COLUMN(key)));
                field = new Field(new TypeInfo { Type = typeof(string[]) }, "Identity")
                {
                    Modifier = Modifier.Public | Modifier.Static | Modifier.Readonly,
                    UserValue = $"new string[] {LP} {iks} {RP}"
                };
                clss.Add(field);
            }

        }

    }
}
