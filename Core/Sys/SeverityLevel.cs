using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sys
{
    [Flags]
    public enum SeverityLevel
    {
        Unknown = 0x00,
        Debug = 0x01,
        Information = 0x02,
        Warn = 0x04,
        Error = 0x08,
        Fatal = 0x10,
    }
}
