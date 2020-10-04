using System;

namespace Sys.Data.Linq
{
    public interface IAssociation
    {
        Type OtherType { get; }
        string Name { get; set; }
        string ThisKey { get; set; }
        string OtherKey { get; set; }
        bool IsForeignKey { get; set; }
    }
}