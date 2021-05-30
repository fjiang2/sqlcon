using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;

namespace Sys.Data
{
    public static class EntityExtension
    {
        public static List<T> ToList<T>(this DataTable dt) where T : IEntityRow, new()
        {
            return dt.AsEnumerable()
            .Select(row =>
            {
                var obj = new T(); 
                obj.FillObject(row); 
                return obj;
            })
            .ToList();
        }
    }
}
