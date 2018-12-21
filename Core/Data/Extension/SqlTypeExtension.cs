using Sys.CodeBuilder;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data
{
    public static class SqlTypeExtension
    {
        public static string GetFieldType(this string sqlType, bool nullable)
        {
            CType ty = sqlType.GetCType();
            return ty.GetCSharpType(nullable);
        }


        public static string GetSQLField(this IColumn column)
        {
            string ty = GetSQLType(column);

            string line = $"[{column.ColumnName}] {ty}";

            if (column.IsIdentity)
            {
                line += " IDENTITY(1,1)";
                return line;
            }

            if (column.Nullable)
                line += " NULL";
            else
                line += " NOT NULL";

            if (column.IsComputed)
            {
                line = string.Format("[{0}] AS {1}", column.ColumnName, column.Definition);
                //throw new JException("not support computed column: {0}", column.ColumnName);
            }

            return line;
        }

        public static string GetSQLType(this IColumn column)
        {
            string ty = "";
            string DataType = column.DataType;
            int Length = column.Length;

            switch (column.CType)
            {
                case CType.VarChar:
                case CType.Char:
                case CType.VarBinary:
                case CType.Binary:
                    if (Length >= 0)
                        ty = string.Format("{0}({1})", DataType, Length);
                    else
                        ty = string.Format("{0}(max)", DataType);
                    break;

                case CType.NVarChar:
                case CType.NChar:
                    if (Length >= 0)
                        ty = string.Format("{0}({1})", DataType, Length / 2);
                    else
                        ty = string.Format("{0}(max)", DataType);
                    break;

                //case SqlDbType.Numeric:
                case CType.Decimal:
                    ty = string.Format("{0}({1},{2})", DataType, column.Precision, column.Scale);
                    break;


                default:
                    ty = DataType;
                    break;
            }
            return ty;
        }

        public static TypeInfo GetTypeInfo(this IColumn column)
        {
            TypeInfo ty = new TypeInfo
            {
                type = column.CType.ToType(),
                Nullable = column.Nullable,
            };

            return ty;
        }
    }
}
