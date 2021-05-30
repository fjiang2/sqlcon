using System.Data;

namespace Sys.Data.Resource
{
    public class ResourceEntry
    {
        public DataRowAction Action { get; set; }
        public string Name { get; set; }
        public string NewValue { get; set; }
        public string OldValue { get; set; }
        public int Index { get; set; }

        public ResourceEntry()
        {
        }

        public override int GetHashCode()
        {
            return Name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var entry = obj as ResourceEntry;
            if (entry == null)
                return false;

            return this.Name == entry.Name;
        }

        public override string ToString()
        {
            if (OldValue == null && NewValue == null)
                return $"{Action} | \"{Name}\"";

            if (OldValue == null)
                return $"{Action} | \"{Name}\" : \"{NewValue}\"";

            return $"{Action} | \"{Name}\" : \"{OldValue}\" -> \"{NewValue}\"";
        }
    }
}

