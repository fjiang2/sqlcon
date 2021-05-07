using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data
{
    public static class Extension
    {
    

        /// <summary>
        /// Adjuested Length
        /// </summary>
        public static int AdjuestedLength(this IColumn column)
        {
            if (column.Length == -1)
                return -1;

            switch (column.CType)
            {
                case CType.NChar:
                case CType.NVarChar:
                    return column.Length / 2;
            }

            return column.Length;
        }
    }
}
