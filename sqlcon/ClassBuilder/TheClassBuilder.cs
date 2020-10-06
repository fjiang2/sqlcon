using System;
using System.Collections.Generic;
using System.Linq;
using System.Data;
using System.IO;
using Sys;
using Sys.Data;
using Sys.CodeBuilder;
using Sys.Data.Manager;
using Sys.Data.Linq;
using System.Data.Common;

namespace sqlcon
{
    abstract class TheClassBuilder : ClassMaker
    {
        protected string EXTENSION = "Extension";

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
                    string type = item.Replace("~", ClassName);
                    bases.Add(new TypeInfo { UserType = type });
                }
            }

            return bases.ToArray();
        }

        protected abstract void CreateClass();

        private void createClass()
        {
            builder.Namespace = NamespaceName;
            CreateClass();
        }

        public string WriteFile(string path)
        {
            createClass();

            base.PrintOutput(builder, ClassName);
            string code = $"{builder}";
            string file = Path.ChangeExtension(Path.Combine(path, ClassName), "cs");
            code.WriteIntoFile(file);

            return file;
        }

        public void Done()
        {
            createClass();
            PrintOutput(builder, ClassName);
        }

        public static string COLUMN(DataColumn column) => COLUMN(column.ColumnName);
        public static string COLUMN(string columnName) => $"_{columnName.ToUpper()}";

        public void CreateTableSchemaFields(TableName tname, DataTable dt, Class clss)
        {
            Field field;

            //schema name
            if (!string.IsNullOrEmpty(dt.Prefix))
            {
                field = new Field(new TypeInfo { Type = typeof(string) }, "SchemaName", new Value(dt.Prefix))
                {
                    Modifier = Modifier.Public | Modifier.Const
                };
                clss.Add(field);
            }

            //table name
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
            if (pk.Length > 0)
            {
                string pks = string.Join(", ", pk.Select(key => COLUMN(key)));
                field = new Field(new TypeInfo { Type = typeof(string[]) }, "Keys")
                {
                    Modifier = Modifier.Public | Modifier.Static | Modifier.Readonly,
                    UserValue = $"new string[] {LP} {pks} {RP}"
                };

                clss.Add(field);
            }

            //identity keys
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

            if (tname != null)
            {
                //builder.AddUsing(typeof(IAssociation).Namespace);
                field = CreateAssoication(tname);
                if (field != null)
                    clss.Add(field);
            }
        }

        private Field CreateAssoication(TableName tname)
        {

            Value ToColumn(string table, string column)
            {
                table = ident.Identifier(table);
                column = COLUMN(column);
                column = $"{table}{EXTENSION}.{column}";
                return new Value(new CodeString(column));
            }

            Value ToColumn2(string column)
            {
                column = COLUMN(column);
                return new Value(new CodeString(column));
            }

            var schema = new TableSchema(tname);
            var pkeys = schema.ByForeignKeys.Keys.OrderBy(k => k.FK_Table);

            List<Value> L = new List<Value>();
            foreach (IForeignKey pkey in pkeys)
            {
                TypeInfo type = new TypeInfo { UserType = $"Association<{ident.Identifier(pkey.FK_Table)}>" };
                var V = Value.NewPropertyObject(type);
                V.AddProperty(nameof(IAssociation.ThisKey), ToColumn2(pkey.PK_Column));
                V.AddProperty(nameof(IAssociation.OtherKey), ToColumn(pkey.FK_Table, pkey.FK_Column));
                if(IsOneToMany(tname, pkey))
                    V.AddProperty(nameof(IAssociation.OneToMany), new Value(true));
                L.Add(V);
            }

            var fkeys = schema.ForeignKeys.Keys.OrderBy(k => k.FK_Table);
            foreach (IForeignKey fkey in fkeys)
            {
                TypeInfo type = new TypeInfo { UserType = $"Association<{ident.Identifier(fkey.PK_Table)}>" };
                var V = Value.NewPropertyObject(type);
                V.AddProperty(nameof(IAssociation.Name), new Value(fkey.Constraint_Name));
                V.AddProperty(nameof(IAssociation.ThisKey), ToColumn2(fkey.FK_Column));
                V.AddProperty(nameof(IAssociation.OtherKey), ToColumn(fkey.PK_Table, fkey.PK_Column));
                V.AddProperty(nameof(IAssociation.IsForeignKey), new Value(true));
                L.Add(V);
            }

            TypeInfo typeinfo = new TypeInfo { UserType = $"{nameof(IAssociation)}[]" };

            Field field = new Field(typeinfo, "Associations", new Value(L.ToArray()) { Type = typeinfo })
            {
                Modifier = Modifier.Public | Modifier.Static | Modifier.Readonly
            };

            return field;
        }

        private bool IsOneToMany(TableName tname, IForeignKey key)
        {
            TableName fk_tname = new TableName(tname.DatabaseName, key.FK_Schema, key.FK_Table);
            var fk_schema = new TableSchema(fk_tname);
            var _keys = fk_schema.PrimaryKeys.Keys;

            if (_keys.Length == 1 && _keys.Contains(key.FK_Column))
            {
                // 1:1 mapping
                return false;
            }
            else
            {
                //1:n mapping
                return true;
            }

        }


        private string[] optionMethods = null;
        public bool ContainsMethod(string methodName)
        {
            if (optionMethods == null)
            {
                string optionMethod = cmd.GetValue("methods");
                if (optionMethod != null)
                    optionMethods = optionMethod.Split(',');
                else
                    optionMethods = new string[] { };
            }

            if (optionMethods.Length == 0)
                return true;

            return optionMethods.Contains(methodName);
        }

        public string PropertyName(DataColumn column)
        {
            return column.ColumnName.ToFieldName("C");
        }
    }
}
