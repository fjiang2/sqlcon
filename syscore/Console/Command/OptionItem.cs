namespace Sys.Stdio
{
    public class OptionItem
    {
        public char Prefix { get; set; }

        public string Name { get; set; }

        public string Value { get; set; }

        public override string ToString()
        {
            return $"{Prefix}{Name}:{Value}";
        }
    }
}
