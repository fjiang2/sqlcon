using Tie;

namespace sqlcon
{
    public interface IApplicationConfiguration 
    {
        string UserConfigFile { get; }
        string OutputFile { get; }
        WorkingDirectory WorkingDirectory { get; }
        string XmlDbDirectory { get; }
        string[] PATH { get; }
        int TopLimit { get; }
        int MaxRows { get; }
        VAL GetValue(VAR variable);
        T GetValue<T>(string variable, T defaultValue = default);
        IConnectionConfiguration Connection { get; }
    }
}