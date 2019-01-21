using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data
{
    public class SelectClause : SqlClause
    {
        public int Top { get; set; }
        public List<ColumnDescriptor> Descriptors { get; } = new List<ColumnDescriptor>();
        public Locator Locator { get; set; }

        public SelectClause()
        {
        }

        public string[] Columns => Descriptors.Select(x => x.ColumnName).ToArray();
    }

    public class ColumnDescriptor
    {
        public string ColumnName { get; set; }
        public string ColumnCaption { get; set; }
    }

}
