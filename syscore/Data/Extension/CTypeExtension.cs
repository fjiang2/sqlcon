using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace Sys.Data
{
    static class CTypeExtension
    {

        public static CType ToCType(this Type type)
        {
            if (type == typeof(Boolean))
                return CType.Bit;

            else if (type == typeof(Byte))
                return CType.TinyInt;
            else if (type == typeof(SByte))
                return CType.SmallInt;

            else if (type == typeof(Int16))
                return CType.SmallInt;
            else if (type == typeof(UInt16))
                return CType.Int;

            else if (type == typeof(Int32))
                return CType.Int;
            else if (type == typeof(UInt32))
                return CType.BigInt;

            else if (type == typeof(Int64))
                return CType.BigInt;
            else if (type == typeof(UInt64))
                return CType.Decimal;

            else if (type == typeof(Single))
                return CType.Real;
            else if (type == typeof(Double))
                return CType.Float;
            
            else if (type == typeof(Decimal))
                return CType.Decimal;

            else if (type == typeof(String))
                return CType.NVarChar;

            else if (type == typeof(DateTime))
                return CType.DateTime;
            else if (type == typeof(DateTimeOffset))
                return CType.DateTimeOffset;
            else if (type == typeof(TimeSpan))
                return CType.Time;

            else if (type == typeof(Byte[]))
                return CType.Binary;

            else if (type == typeof(Guid))
                return CType.UniqueIdentifier;

            throw new MessageException("Type {0} cannot be converted into SqlDbType", type.FullName);
        }


        public static Type ToType(this CType type)
        {
            switch (type)
            {
                case CType.Bit:
                    return typeof(bool);

                case CType.TinyInt:
                    return typeof(byte);

                case CType.SmallInt:
                    return typeof(short);

                case CType.Int:
                    return typeof(int);

                case CType.BigInt:
                    return typeof(long);

                case CType.Float:
                    return typeof(double);

                case CType.Real:
                    return typeof(float);

                case CType.Decimal:
                case CType.SmallMoney:
                case CType.Money:
                    return typeof(decimal);

                case CType.Char:
                case CType.NChar:
                case CType.VarChar:
                case CType.NVarChar:
                case CType.Text:
                case CType.NText:
                    return typeof(string);

                case CType.SmallDateTime:
                case CType.DateTime:
                case CType.Date:
                case CType.DateTime2:
                    return typeof(DateTime);

                case CType.DateTimeOffset:
                    return typeof(DateTimeOffset);

                case CType.Time:
                    return typeof(TimeSpan);

                case CType.Timestamp:
                case CType.VarBinary:
                case CType.Binary:
                case CType.Image:
                    return typeof(byte[]);

                case CType.UniqueIdentifier:
                    return typeof(Guid);

                case CType.Xml:
                    return typeof(XElement);

                case CType.Variant:
                case CType.Udt:
                    break;

                    //case CType.Geography:
                    //    return typeof(Microsoft.SqlServer.Types.SqlGeography);

                    //case CType.Geometry:
                    //    return typeof(Microsoft.SqlServer.Types.SqlGeometry);

                    //case CType.HierarchyId:
                    //    return typeof(Microsoft.SqlServer.Types.SqlHierarchyId);

            }

            throw new MessageException("SqlDbType {0} cannot be converted into Type", type);
        }

        public static CType GetCType(this string sqlType)
        {
            switch (sqlType.ToLower())
            {
                case "varchar":
                    return CType.VarChar;

                case "char":
                    return CType.Char;

                case "nvarchar":
                    return CType.NVarChar;

                case "nchar":
                    return CType.NChar;

                case "decimal":
                    return CType.Decimal;

                case "numeric":
                    return CType.Decimal;

                case "text":
                    return CType.Text;

                case "ntext":
                    return CType.NText;

                case "datetime":
                    return CType.DateTime;

                case "datetime2":
                    return CType.DateTime;

                case "smalldatetime":
                    return CType.SmallDateTime;

                case "datetimeoffset":
                    return CType.DateTimeOffset;

                case "timestamp":
                    return CType.Timestamp;

                case "bit":
                    return CType.Bit;

                case "money":
                    return CType.Money;

                case "smallmoney":
                    return CType.SmallMoney;

                case "real":
                    return CType.Real;

                case "float":
                    return CType.Float;

                case "tinyint":
                    return CType.TinyInt;

                case "smallint":
                    return CType.SmallInt;

                case "int":
                    return CType.Int;

                case "bigint":
                    return CType.BigInt;

                case "varbinary":
                    return CType.VarBinary;

                case "binary":
                    return CType.Binary;

                case "image":
                    return CType.Image;

                case "uniqueidentifier":
                    return CType.UniqueIdentifier;

                case "hierarchyid":
                    return CType.HierarchyId;

                case "geometry":
                    return CType.Geometry;

                case "geography":
                    return CType.Geography;

                case "date":
                    return CType.Date;

                case "time":
                    return CType.Time;

                case "xml":
                    return CType.Xml;

                case "sql_variant":
                    return CType.Object;
            }

            throw new MessageException("data type [{0}] is not supported", sqlType);
        }


        public static string GetSqlType(this CType ctype)
        {
            switch (ctype)
            {
                case CType.VarChar:
                    return "varchar";

                case CType.Char:
                    return "char";

                case CType.NVarChar:
                    return "nvarchar";

                case CType.NChar:
                    return "nchar";

                case CType.Decimal:
                    return "decimal";
                    //return "numeric";

                case CType.Text:
                    return "text";

                case CType.NText:
                    return "ntext";

                case CType.DateTime:
                    return "datetime";
                    //return "datetime2";

                case CType.SmallDateTime:
                    return "smalldatetime";

                case CType.DateTimeOffset:
                    return "datetimeoffset";

                case CType.Timestamp:
                    return "timestamp";

                case CType.Bit:
                    return "bit";

                case CType.Money:
                    return "money";

                case CType.SmallMoney:
                    return "smallmoney";

                case CType.Real:
                    return "real";

                case CType.Float:
                    return "float";

                case CType.TinyInt:
                    return "tinyint";

                case CType.SmallInt:
                    return "smallint";

                case CType.Int:
                    return "int";

                case CType.BigInt:
                    return "bigint";

                case CType.VarBinary:
                    return "varbinary";

                case CType.Binary:
                    return "binary";

                case CType.Image:
                    return "image";

                case CType.UniqueIdentifier:
                    return "uniqueidentifier";

                case CType.HierarchyId:
                    return "hierarchyid";

                case CType.Geometry:
                    return "geometry";

                case CType.Geography:
                    return "geography";

                case CType.Date:
                    return "date";

                case CType.Time:
                    return "time";

                case CType.Xml:
                    return "xml";

                case CType.Object:
                    return "sql_variant";
            }

            throw new MessageException($"ctype [{ctype}] is not supported");
        }


    }
}
