using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Data.OleDb;

namespace Sys.Data
{

    public enum CType
    {
        BigInt = 0,
        Binary = 1,
        Bit = 2,
        Char = 3,
        DateTime = 4,
        Decimal = 5,
        Float = 6,
        Image = 7,
        Int = 8,
        Money = 9,
        NChar = 10,
        NText = 11,
        NVarChar = 12,
        Real = 13,
        UniqueIdentifier = 14,
        SmallDateTime = 15,
        SmallInt = 16,
        SmallMoney = 17,
        Text = 18,
        Timestamp = 19,
        TinyInt = 20,
        VarBinary = 21,
        VarChar = 22,
        Variant = 23,
        Xml = 25,
        Udt = 29,
        Date = 31,
        Time = 32,
        DateTime2 = 33,
        DateTimeOffset = 34,

        HierarchyId = 35,
        Geometry = 36,
        Geography = 37,

        Auto = 100
    }

    public static class DbTypex
    {
        #region ToSqlDbType / ToType

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


                case CType.Variant:
                case CType.Xml:
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

        #endregion
    }
}
