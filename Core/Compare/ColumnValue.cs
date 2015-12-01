using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data.Comparison
{
    class ColumnValue
    {
        public object Value { get; set; }

        private const string DELIMETER = "'";

        public ColumnValue(object value)
        {
            this.Value = value;
        }

        public string ToScript()
        {
            if (Value == null || Value == DBNull.Value)
                return "NULL";
            else if (Value is DateTime)
            {
                DateTime time = (DateTime)Value;
                var d = DELIMETER + string.Format("{0} {1}", time.ToString("d"), time.ToString("HH:mm:ss.fff")) + DELIMETER;
                return d;
            }
            else if (Value is string)
                return "N" + DELIMETER + (Value as string).Replace("'", "''") + DELIMETER;
            else if (Value is Guid)
                return "N" + DELIMETER + Value.ToString() + DELIMETER;
            else if (Value is bool)
                return (bool)Value ? "1" : "0";
            else if (Value is byte[])
            {
                return "0x" + ByteArrayToHexString((byte[])Value);
            }
            else
                return Value.ToString();
        }

        public static string ToScript(IColumn column)
        {
            string name = "@" + column.ColumnName;

            switch (column.CType)
            {
                case CType.VarChar:
                case CType.Char:
                case CType.NVarChar:
                case CType.NChar:
                case CType.DateTime:
                case CType.DateTime2:
                case CType.DateTimeOffset:
                    return DELIMETER + name + DELIMETER;
            }
         
            return name;
        }

        public static string ByteArrayToHexString(byte[] bytes)
        {
            char[] c = new char[bytes.Length * 2];
            byte b;
            for (int i = 0; i < bytes.Length; ++i)
            {
                b = ((byte)(bytes[i] >> 4));
                c[i * 2] = (char)(b > 9 ? b + 0x37 : b + 0x30);

                b = ((byte)(bytes[i] & 0xF));
                c[i * 2 + 1] = (char)(b > 9 ? b + 0x37 : b + 0x30);
            }

            return new string(c);
        }


        public override string ToString()
        {
            return ToScript();
        }

    }
}
