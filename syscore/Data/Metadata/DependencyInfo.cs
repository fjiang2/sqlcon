using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys.Data
{

    public class DependencyInfo
    {
        public TableName FkTable { get; set; }
        public string FkColumn { get; set; }

        public TableName PkTable { get; set; }
        public string PkColumn { get; set; }

        public DependencyInfo()
        {
        }

        public override string ToString() => $"{FkTable.FormalName}.{FkColumn} => {PkTable.FormalName}.{PkColumn}";
    }
}
