using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Sys.Data
{

    public interface IIdentityKeys
    {
        string[] ColumnNames { get; }
        int Length { get; }
    }
}
