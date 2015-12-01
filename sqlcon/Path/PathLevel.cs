using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sqlcon
{
    enum PathLevel
    {
        Unknown,
        Tables,
        Views,
        Proc,
        Func
    }
}
