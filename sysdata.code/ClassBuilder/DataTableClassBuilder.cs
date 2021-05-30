using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;

using Sys;
using Sys.Data;
using Sys.CodeBuilder;
using Sys.Data.Manager;
using Sys.Data.Linq;
using Sys.Stdio;

namespace Sys.Data.Code
{
    public abstract class DataTableClassBuilder : TheClassBuilder
    {
        protected TableName tname;
        protected DataTable dt;
        protected IDictionary<DataColumn, TypeInfo> dict { get; }

        public DataTableClassBuilder(IApplicationCommand cmd, TableName tname, DataTable dt, bool allowDbNull)
            : base(cmd)
        {
            this.tname = tname;
            this.dt = dt;

            this.dict = CreateMapOfTypeInfo(dt, allowDbNull);

        }

        private static IDictionary<DataColumn, TypeInfo> CreateMapOfTypeInfo(DataTable dt, bool allowDbNull)
        {
            Dictionary<DataColumn, TypeInfo> dict = new Dictionary<DataColumn, TypeInfo>();

            foreach (DataColumn column in dt.Columns)
            {
                TypeInfo ty = new TypeInfo { Type = column.DataType };

                if (allowDbNull)
                {
                    if (column.AllowDBNull)
                    {
                        ty.Nullable = true;
                    }
                    else
                    {
                        foreach (DataRow row in dt.Rows)
                        {
                            if (row[column] == DBNull.Value)
                                ty.Nullable = true;
                            break;
                        }
                    }
                }

                dict.Add(column, ty);
            }

            return dict;
        }

        protected string GetField
        {
            get
            {
                string _GetField = "Field";

                if (base.MethodName != null)
                    _GetField = base.MethodName;

                return _GetField;
            }
        }
        public static string COLUMN(DataColumn column) => COLUMN(column.ColumnName);
        public static string COLUMN(string columnName)
        {
            string name = ident.Identifier(columnName).ToUpper();
            if (name.StartsWith("_"))
                return name;
            else
                return "_" + name;
        }

        protected void Method_CreateTable(Class clss)
        {
            Method method = new Method("CreateTable")
            {
                Modifier = Modifier.Public | Modifier.Static,
                Type = new TypeInfo { Type = typeof(DataTable) }
            };
            clss.Add(method);

            bool hasColumnProperty = cmd.Has("data-column-property");
            Statement sent = method.Body;
            sent.AppendLine("DataTable dt = new DataTable();");
            foreach (DataColumn column in dt.Columns)
            {
                TypeInfo ty = new TypeInfo(dict[column].Type);
                var name = COLUMN(column);

                List<Expression> expressions = new List<Expression>();
                if (hasColumnProperty)
                {
                    if (column.Unique)
                        expressions.Add(new Expression(nameof(column.Unique), true));
                    if (!column.AllowDBNull)
                        expressions.Add(new Expression(nameof(column.AllowDBNull), false));
                    if (column.MaxLength > 0)
                        expressions.Add(new Expression(nameof(column.MaxLength), column.MaxLength));
                    if (column.AutoIncrement)
                        expressions.Add(new Expression(nameof(column.AutoIncrement), true));
                }

                var _args = new Arguments(new Argument(name), new Argument(ty));
                var _column = new New(typeof(DataColumn), _args, expressions);

                sent.AppendLine($"dt.Columns.Add({_column});");
            }

            sent.AppendLine();
            sent.AppendLine("return dt;");
        }

       


        public static void CreateTableSchemaFields(TableName tname, DataTable dt, Class clss)
        {
            Field field;

            //schema name
            if (!dt.IsDbo())
            {
                field = new Field(new TypeInfo { Type = typeof(string) }, "SchemaName", new Value(dt.GetSchemaName()))
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


        }

        protected static Field CreateConstraintField(TableName tname)
        {
            const string CONSTRAINT = nameof(Constraint);
            CodeString ToColumn(string table, string column)
            {
                table = ident.Identifier(table);
                column = COLUMN(column);
                column = $"{table}{EXTENSION}.{column}";
                return new CodeString(column);
            }

            CodeString ToColumn2(string column)
            {
                column = COLUMN(column);
                return new CodeString(column);
            }

            var schema = TableSchemaCache.GetSchema(tname);
            var pkeys = schema.ByForeignKeys.Keys.OrderBy(k => k.FK_Table);

            List<Expression> L = new List<Expression>();
            foreach (IForeignKey pkey in pkeys)
            {
                string entity = ident.Identifier(pkey.FK_Table);
                TypeInfo type = new TypeInfo { UserType = $"{CONSTRAINT}<{entity}>" };
                var instance = new New(type) { Format = ValueOutputFormat.MultipleLine };
                instance.AddProperty(nameof(IConstraint.ThisKey), ToColumn2(pkey.PK_Column));
                instance.AddProperty(nameof(IConstraint.OtherKey), ToColumn(pkey.FK_Table, pkey.FK_Column));
                if (IsOneToMany(tname, pkey))
                    instance.AddProperty(nameof(IConstraint.OneToMany), true);
                L.Add(instance);
            }

            var fkeys = schema.ForeignKeys.Keys.OrderBy(k => k.FK_Table);
            foreach (IForeignKey fkey in fkeys)
            {
                string entity = ident.Identifier(fkey.PK_Table);
                TypeInfo type = new TypeInfo { UserType = $"{CONSTRAINT}<{entity}>" };
                var instance = new New(type) { Format = ValueOutputFormat.MultipleLine };
                instance.AddProperty(nameof(IConstraint.Name), new Value(fkey.Constraint_Name));
                instance.AddProperty(nameof(IConstraint.ThisKey), ToColumn2(fkey.FK_Column));
                instance.AddProperty(nameof(IConstraint.OtherKey), ToColumn(fkey.PK_Table, fkey.PK_Column));
                instance.AddProperty(nameof(IConstraint.IsForeignKey), true);
                L.Add(instance);
            }

            TypeInfo typeinfo = new TypeInfo { UserType = $"{nameof(IConstraint)}[]" };

            Field field = new Field(typeinfo, $"{CONSTRAINT}s", new New(typeinfo, L) { Format = ValueOutputFormat.MultipleLine })
            {
                Modifier = Modifier.Public | Modifier.Static | Modifier.Readonly
            };

            return field;
        }


        protected class AssociationPropertyInfo
        {
            public string PropertyType { get; set; }
            public string PropertyName { get; set; }
            public bool OneToMany { get; set; }

            public string PK_Column { get; set; }
            public string FK_Column { get; set; }
        }

        protected List<AssociationPropertyInfo> CreateAssoicationClass(TableName tname, Class clss)
        {
            List<AssociationPropertyInfo> properties = new List<AssociationPropertyInfo>();

            var schema = TableSchemaCache.GetSchema(tname);
            var pkeys = schema.ByForeignKeys.Keys.OrderBy(k => k.FK_Table);

            foreach (IForeignKey pkey in pkeys)
            {
                string entity = ident.Identifier(pkey.FK_Table);

                TypeInfo type;
                string propertyName;
                bool one2many = IsOneToMany(tname, pkey);
                if (one2many)
                {
                    type = new TypeInfo { UserType = $"EntitySet<{entity}>" };
                    propertyName = Plural.Pluralize(entity);
                }
                else
                {
                    type = new TypeInfo { UserType = $"EntityRef<{entity}>" };
                    propertyName = Plural.Singularize(entity);
                }

                var property = new Property(type, propertyName);
                clss.Add(property);
                properties.Add(new AssociationPropertyInfo
                {
                    PropertyType = entity,
                    PropertyName = propertyName,
                    OneToMany = one2many,
                    PK_Column = pkey.PK_Column,
                    FK_Column = pkey.FK_Column,
                });
            }

            var fkeys = schema.ForeignKeys.Keys.OrderBy(k => k.FK_Table);
            foreach (IForeignKey fkey in fkeys)
            {
                string entity = ident.Identifier(fkey.PK_Table);
                TypeInfo type = new TypeInfo { UserType = $"EntityRef<{entity}>" };
                string propertyName = Plural.Singularize(entity);
                var property = new Property(type, propertyName);
                clss.Add(property);

                properties.Add(new AssociationPropertyInfo
                {
                    PropertyType = entity,
                    PropertyName = propertyName,
                    OneToMany = false,
                    PK_Column = fkey.FK_Column,
                    FK_Column = fkey.PK_Column,
                });
            }

            return properties;
        }

        private static bool IsOneToMany(TableName tname, IForeignKey key)
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


    }
}
