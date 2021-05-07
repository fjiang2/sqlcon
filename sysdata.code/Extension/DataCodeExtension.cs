using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sys.CodeBuilder;

namespace Sys.Data
{
    public static class DataCodeExtension
    {
        public static TypeInfo GetTypeInfo(this IColumn column)
        {
            TypeInfo ty = new TypeInfo
            {
                Type = column.CType.ToType(),
                Nullable = column.Nullable,
            };

            return ty;
        }

        public static Level Level(this Type dpoType)
        {
            TableAttribute[] A = dpoType.GetAttributes<TableAttribute>();
            if (A.Length > 0)
                return A[0].Level;

            throw new MessageException("Table Level is not defined");
        }

    }
}
