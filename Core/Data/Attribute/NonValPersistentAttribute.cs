using System;
using System.Collections.Generic;
using System.Text;

namespace Afs.Data
{
    [System.AttributeUsage(System.AttributeTargets.Field)]
    public class NonValPersistentAttribute : Attribute 
    {
    }
}
