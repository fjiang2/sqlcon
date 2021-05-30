namespace Sys.Stdio
{
    public interface ICommand
    {
        string Action { get; }
        bool? GetBoolean(string name);
        bool GetBoolean(string name, bool defaultValue);
        double? GetDouble(string name);
        double GetDouble(string name, double defaultValue);
        T GetEnum<T>(string name, T defaultValue) where T : struct;
        int? GetInt32(string name);
        int GetInt32(string name, int defaultValue);
        string GetValue(string name);
        bool Has(string name);
    }
}