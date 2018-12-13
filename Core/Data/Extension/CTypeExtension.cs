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

            else if (type == typeof(Int16))
                return CType.SmallInt;

            else if (type == typeof(Byte))
                return CType.TinyInt;

            else if (type == typeof(Int32))
                return CType.Int;

            else if (type == typeof(Int64))
                return CType.BigInt;

            else if (type == typeof(Double))
                return CType.Float;

            else if (type == typeof(Single))
                return CType.Float;

            else if (type == typeof(Decimal))
                return CType.Decimal;

            else if (type == typeof(String))
                return CType.NVarChar;

            else if (type == typeof(DateTime))
                return CType.DateTime;

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
                    return typeof(System.Boolean);

                case CType.TinyInt:
                    return typeof(byte);

                case CType.SmallInt:
                    return typeof(Int16);

                case CType.Int:
                    return typeof(Int32);

                case CType.BigInt:
                    return typeof(Int64);

                case CType.Float:
                    return typeof(Double);

                case CType.Real:
                    return typeof(Single);

                case CType.Decimal:
                case CType.SmallMoney:
                case CType.Money:
                    return typeof(Decimal);

                case CType.Char:
                case CType.NChar:
                case CType.VarChar:
                case CType.NVarChar:
                case CType.Text:
                case CType.NText:
                    return typeof(String);

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
                    return typeof(Byte[]);

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


    }
}
