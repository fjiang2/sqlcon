namespace Sys.Data.Resource
{
    class entry
    {
        public string name { get; set; }
        public string value { get; set; }

        public entry()
        {

        }

        public override int GetHashCode()
        {
            return name.GetHashCode();
        }

        public override bool Equals(object obj)
        {
            var entry = obj as entry;
            if (entry == null)
                return false;

            return this.name == entry.name && this.value == entry.value;
        }


        public override string ToString() => $"\"{name}\" : \"{value}\"";
    }
}