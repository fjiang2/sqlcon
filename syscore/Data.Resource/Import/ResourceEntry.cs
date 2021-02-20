using System.Data;

namespace Sys.Data.Resource
{
    public class ResourceEntry
    {
        public DataRowAction Action { get; set; }
        public string Name { get; set; }
        public string NewValue { get; set; }
        public string OldValue { get; set; }

        public override string ToString()
        {
            if (OldValue == null)
                return $"\"{Name}\" : \"{NewValue}\"";
            else
                return $"\"{Name}\" : \"{OldValue}\" -> \"{NewValue}\"";
        }
    }
}

