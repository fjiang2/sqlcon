using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;
using System.Xml.Linq;

namespace Sys.Data
{

    static class DbTypeExtension
    {
        #region ToSqlDbType / ToType

    

        public static SqlDbType ToSqlDbType(this CType type)
        {
            return (SqlDbType)((int)type);
        }


        public static OleDbType ToOleDbType(this Type type)
        {
            if (type == typeof(Boolean))
                return OleDbType.Boolean;

            else if (type == typeof(Byte))
                return OleDbType.TinyInt;

            else if (type == typeof(Int16))
                return OleDbType.SmallInt;

            else if (type == typeof(Int32))
                return OleDbType.Integer;

            else if (type == typeof(Int64))
                return OleDbType.BigInt;

            else if (type == typeof(Double))
                return OleDbType.Double;

            else if (type == typeof(Decimal))
                return OleDbType.Decimal;

            else if (type == typeof(String))
                return OleDbType.WChar;

            else if (type == typeof(DateTime))
                return OleDbType.Date;

            else if (type == typeof(Byte[]))
                return OleDbType.Binary;


            throw new MessageException("Type {0} cannot be converted into SqlDbType", type.FullName);
        }



        public static string GetCSharpType(this CType type, bool nullable)
        {
            string ty = "";
            switch (type)
            {
                case CType.VarChar:
                case CType.Char:
                case CType.Text:
                case CType.NVarChar:
                case CType.NChar:
                case CType.NText:
                    ty = "string";
                    break;

                case CType.Date:
                case CType.DateTime:
                case CType.SmallDateTime:
                    ty = "DateTime";
                    if (nullable) ty += "?";
                    break;

                case CType.DateTimeOffset:
                    ty = "DateTimeOffset";
                    if (nullable) ty += "?";
                    break;

                case CType.Time:
                case CType.Timestamp:
                    ty = "TimeSpan";
                    if (nullable) ty += "?";
                    break;

                case CType.Bit:
                    ty = "bool";
                    if (nullable) ty += "?";
                    break;

                case CType.Money:
                case CType.SmallMoney:
                case CType.Decimal:
                    ty = "decimal";
                    if (nullable) ty += "?";
                    break;

                case CType.Real:
                    ty = "Single";
                    if (nullable) ty += "?";
                    break;

                case CType.Float:
                    ty = "double";
                    if (nullable) ty += "?";
                    break;

                case CType.TinyInt:
                    ty = "byte";
                    if (nullable) ty += "?";
                    break;

                case CType.SmallInt:
                    ty = "short";
                    if (nullable) ty += "?";
                    break;

                case CType.Int:
                    ty = "int";
                    if (nullable) ty += "?";
                    break;

                case CType.BigInt:
                    ty = "long";
                    if (nullable) ty += "?";
                    break;


                case CType.VarBinary:
                case CType.Binary:
                case CType.Image:
                    ty = "byte[]";
                    break;

                case CType.UniqueIdentifier:
                    ty = "Guid";
                    break;

                case CType.Geography:
                    ty = "SqlGeography";
                    break;

                case CType.Geometry:
                    ty = "SqlGeometry";
                    break;

                case CType.HierarchyId:
                    ty = "SqlHierarchyId";
                    break;

                case CType.Object:
                    ty = "object";
                    break;

                default:
                    ty = "object";
                    break;
            }

            return ty;
        }





        #endregion
    }
}
