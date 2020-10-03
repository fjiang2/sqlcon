using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data.Linq
{
    public class Association<TEntity> : IAssociation
    {
        public string Name { get; set; }
        public string ThisKey { get; set; }
        public string OtherKey { get; set; }
        public bool IsForeignKey { get; set; }
    }
}
