namespace Sys.Data.Resource
{
    class Entry
    {
        public string name { get; set; }
        public string value { get; set; }

        public Entry()
        {

        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var entry = obj as Entry;
            if (entry == null)
                return false;

            return this.name == entry.name && this.value == entry.value;
        }


        public override string ToString() => $"\"{name}\" : \"{value}\"";
    }
}