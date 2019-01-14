using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;

namespace Sys.Data
{
    static class DbSchemaColumnExtension
    {
        public static List<DbSchemaColumn> ToDbSchemaColumnCollection(this DataTable dt)
        {
            return dt.AsEnumerable()
            .Select(row => NewObject(row))
            .ToList();
        }

        public static DbSchemaColumn NewObject(DataRow row)
        {
            return new DbSchemaColumn
            {
                SchemaName = row.Field<string>(_SCHEMANAME),
                TableName = row.Field<string>(_TABLENAME),
                ColumnName = row.Field<string>(_COLUMNNAME),
                DataType = row.Field<string>(_DATATYPE),
                Length = row.Field<short>(_LENGTH),
                Nullable = row.Field<bool>(_NULLABLE),
                precision = row.Field<byte>(_PRECISION),
                scale = row.Field<byte>(_SCALE),
                IsPrimary = row.Field<bool>(_ISPRIMARY),
                IsIdentity = row.Field<bool>(_ISIDENTITY),
                IsComputed = row.Field<bool>(_ISCOMPUTED),
                definition = row.Field<string>(_DEFINITION),
                PKContraintName = row.Field<string>(_PKCONTRAINTNAME),
                PK_Schema = row.Field<string>(_PK_SCHEMA),
                PK_Table = row.Field<string>(_PK_TABLE),
                PK_Column = row.Field<string>(_PK_COLUMN),
                FKContraintName = row.Field<string>(_FKCONTRAINTNAME)
            };
        }

        public static void FillObject(this DbSchemaColumn item, DataRow row)
        {
            item.SchemaName = row.Field<string>(_SCHEMANAME);
            item.TableName = row.Field<string>(_TABLENAME);
            item.ColumnName = row.Field<string>(_COLUMNNAME);
            item.DataType = row.Field<string>(_DATATYPE);
            item.Length = row.Field<short>(_LENGTH);
            item.Nullable = row.Field<bool>(_NULLABLE);
            item.precision = row.Field<byte>(_PRECISION);
            item.scale = row.Field<byte>(_SCALE);
            item.IsPrimary = row.Field<bool>(_ISPRIMARY);
            item.IsIdentity = row.Field<bool>(_ISIDENTITY);
            item.IsComputed = row.Field<bool>(_ISCOMPUTED);
            item.definition = row.Field<string>(_DEFINITION);
            item.PKContraintName = row.Field<string>(_PKCONTRAINTNAME);
            item.PK_Schema = row.Field<string>(_PK_SCHEMA);
            item.PK_Table = row.Field<string>(_PK_TABLE);
            item.PK_Column = row.Field<string>(_PK_COLUMN);
            item.FKContraintName = row.Field<string>(_FKCONTRAINTNAME);
        }

        public static void UpdateRow(this DbSchemaColumn item, DataRow row)
        {
            row.SetField(_SCHEMANAME, item.SchemaName);
            row.SetField(_TABLENAME, item.TableName);
            row.SetField(_COLUMNNAME, item.ColumnName);
            row.SetField(_DATATYPE, item.DataType);
            row.SetField(_LENGTH, item.Length);
            row.SetField(_NULLABLE, item.Nullable);
            row.SetField(_PRECISION, item.precision);
            row.SetField(_SCALE, item.scale);
            row.SetField(_ISPRIMARY, item.IsPrimary);
            row.SetField(_ISIDENTITY, item.IsIdentity);
            row.SetField(_ISCOMPUTED, item.IsComputed);
            row.SetField(_DEFINITION, item.definition);
            row.SetField(_PKCONTRAINTNAME, item.PKContraintName);
            row.SetField(_PK_SCHEMA, item.PK_Schema);
            row.SetField(_PK_TABLE, item.PK_Table);
            row.SetField(_PK_COLUMN, item.PK_Column);
            row.SetField(_FKCONTRAINTNAME, item.FKContraintName);
        }

        public static DataTable CreateTable()
        {
            DataTable dt = new DataTable();
            dt.Columns.Add(new DataColumn(_SCHEMANAME, typeof(System.String)));
            dt.Columns.Add(new DataColumn(_TABLENAME, typeof(System.String)));
            dt.Columns.Add(new DataColumn(_COLUMNNAME, typeof(System.String)));
            dt.Columns.Add(new DataColumn(_DATATYPE, typeof(System.String)));
            dt.Columns.Add(new DataColumn(_LENGTH, typeof(System.Int16)));
            dt.Columns.Add(new DataColumn(_NULLABLE, typeof(System.Boolean)));
            dt.Columns.Add(new DataColumn(_PRECISION, typeof(System.Byte)));
            dt.Columns.Add(new DataColumn(_SCALE, typeof(System.Byte)));
            dt.Columns.Add(new DataColumn(_ISPRIMARY, typeof(System.Boolean)));
            dt.Columns.Add(new DataColumn(_ISIDENTITY, typeof(System.Boolean)));
            dt.Columns.Add(new DataColumn(_ISCOMPUTED, typeof(System.Boolean)));
            dt.Columns.Add(new DataColumn(_DEFINITION, typeof(System.String)));
            dt.Columns.Add(new DataColumn(_PKCONTRAINTNAME, typeof(System.String)));
            dt.Columns.Add(new DataColumn(_PK_SCHEMA, typeof(System.String)));
            dt.Columns.Add(new DataColumn(_PK_TABLE, typeof(System.String)));
            dt.Columns.Add(new DataColumn(_PK_COLUMN, typeof(System.String)));
            dt.Columns.Add(new DataColumn(_FKCONTRAINTNAME, typeof(System.String)));

            return dt;
        }

        public static void ToDataTable(this IEnumerable<DbSchemaColumn> items, DataTable dt)
        {
            foreach (var item in items)
            {
                var row = dt.NewRow();
                UpdateRow(item, row);
                dt.Rows.Add(row);
            }
            dt.AcceptChanges();
        }

        public static DataTable ToDataTable(this IEnumerable<DbSchemaColumn> items)
        {
            var dt = CreateTable();
            ToDataTable(items, dt);
            return dt;
        }

        public static IDictionary<String, Object> ToDictionary(this DbSchemaColumn item)
        {
            return new Dictionary<string, object>()
            {
                [_SCHEMANAME] = item.SchemaName,
                [_TABLENAME] = item.TableName,
                [_COLUMNNAME] = item.ColumnName,
                [_DATATYPE] = item.DataType,
                [_LENGTH] = item.Length,
                [_NULLABLE] = item.Nullable,
                [_PRECISION] = item.precision,
                [_SCALE] = item.scale,
                [_ISPRIMARY] = item.IsPrimary,
                [_ISIDENTITY] = item.IsIdentity,
                [_ISCOMPUTED] = item.IsComputed,
                [_DEFINITION] = item.definition,
                [_PKCONTRAINTNAME] = item.PKContraintName,
                [_PK_SCHEMA] = item.PK_Schema,
                [_PK_TABLE] = item.PK_Table,
                [_PK_COLUMN] = item.PK_Column,
                [_FKCONTRAINTNAME] = item.FKContraintName
            };
        }

        public static DbSchemaColumn FromDictionary(this IDictionary<String, Object> dict)
        {
            return new DbSchemaColumn
            {
                SchemaName = (string)dict[_SCHEMANAME],
                TableName = (string)dict[_TABLENAME],
                ColumnName = (string)dict[_COLUMNNAME],
                DataType = (string)dict[_DATATYPE],
                Length = (short)dict[_LENGTH],
                Nullable = (bool)dict[_NULLABLE],
                precision = (byte)dict[_PRECISION],
                scale = (byte)dict[_SCALE],
                IsPrimary = (bool)dict[_ISPRIMARY],
                IsIdentity = (bool)dict[_ISIDENTITY],
                IsComputed = (bool)dict[_ISCOMPUTED],
                definition = (string)dict[_DEFINITION],
                PKContraintName = (string)dict[_PKCONTRAINTNAME],
                PK_Schema = (string)dict[_PK_SCHEMA],
                PK_Table = (string)dict[_PK_TABLE],
                PK_Column = (string)dict[_PK_COLUMN],
                FKContraintName = (string)dict[_FKCONTRAINTNAME]
            };
        }

        public static bool CompareTo(this DbSchemaColumn a, DbSchemaColumn b)
        {
            return a.SchemaName == b.SchemaName
            && a.TableName == b.TableName
            && a.ColumnName == b.ColumnName
            && a.DataType == b.DataType
            && a.Length == b.Length
            && a.Nullable == b.Nullable
            && a.precision == b.precision
            && a.scale == b.scale
            && a.IsPrimary == b.IsPrimary
            && a.IsIdentity == b.IsIdentity
            && a.IsComputed == b.IsComputed
            && a.definition == b.definition
            && a.PKContraintName == b.PKContraintName
            && a.PK_Schema == b.PK_Schema
            && a.PK_Table == b.PK_Table
            && a.PK_Column == b.PK_Column
            && a.FKContraintName == b.FKContraintName;
        }

        public static void CopyTo(this DbSchemaColumn from, DbSchemaColumn to)
        {
            to.SchemaName = from.SchemaName;
            to.TableName = from.TableName;
            to.ColumnName = from.ColumnName;
            to.DataType = from.DataType;
            to.Length = from.Length;
            to.Nullable = from.Nullable;
            to.precision = from.precision;
            to.scale = from.scale;
            to.IsPrimary = from.IsPrimary;
            to.IsIdentity = from.IsIdentity;
            to.IsComputed = from.IsComputed;
            to.definition = from.definition;
            to.PKContraintName = from.PKContraintName;
            to.PK_Schema = from.PK_Schema;
            to.PK_Table = from.PK_Table;
            to.PK_Column = from.PK_Column;
            to.FKContraintName = from.FKContraintName;
        }

        public static string ToSimpleString(this DbSchemaColumn obj)
        {
            return string.Format("{{SchemaName:{0}, TableName:{1}, ColumnName:{2}, DataType:{3}, Length:{4}, Nullable:{5}, precision:{6}, scale:{7}, IsPrimary:{8}, IsIdentity:{9}, IsComputed:{10}, definition:{11}, PKContraintName:{12}, PK_Schema:{13}, PK_Table:{14}, PK_Column:{15}, FKContraintName:{16}}}",
            obj.SchemaName,
            obj.TableName,
            obj.ColumnName,
            obj.DataType,
            obj.Length,
            obj.Nullable,
            obj.precision,
            obj.scale,
            obj.IsPrimary,
            obj.IsIdentity,
            obj.IsComputed,
            obj.definition,
            obj.PKContraintName,
            obj.PK_Schema,
            obj.PK_Table,
            obj.PK_Column,
            obj.FKContraintName);
        }

        public const string _SCHEMANAME = "SchemaName";
        public const string _TABLENAME = "TableName";
        public const string _COLUMNNAME = "ColumnName";
        public const string _DATATYPE = "DataType";
        public const string _LENGTH = "Length";
        public const string _NULLABLE = "Nullable";
        public const string _PRECISION = "precision";
        public const string _SCALE = "scale";
        public const string _ISPRIMARY = "IsPrimary";
        public const string _ISIDENTITY = "IsIdentity";
        public const string _ISCOMPUTED = "IsComputed";
        public const string _DEFINITION = "definition";
        public const string _PKCONTRAINTNAME = "PKContraintName";
        public const string _PK_SCHEMA = "PK_Schema";
        public const string _PK_TABLE = "PK_Table";
        public const string _PK_COLUMN = "PK_Column";
        public const string _FKCONTRAINTNAME = "FKContraintName";
    }
}