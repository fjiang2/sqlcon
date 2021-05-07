using Tie;
using Sys.Data;

namespace sqlcon
{
    public interface IApplicationConfiguration 
    {
        string OutputFile { get; }
        WorkingDirectory WorkingDirectory { get; }
        string XmlDbDirectory { get; }
        string Path { get; }
        int TopLimit { get; }
        int MaxRows { get; }
        VAL GetValue(VAR variable);
        T GetValue<T>(string variable, T defaultValue = default);
        bool TryGetValue<T>(string variable, out T result);
        IConnectionConfiguration Connection { get; }
    }
}