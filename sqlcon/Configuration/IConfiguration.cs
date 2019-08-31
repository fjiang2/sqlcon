using Tie;

namespace sqlcon
{
    interface IConfiguration : IConnectionConfiguration
    {
        string UserConfigurationFile { get; }
        string OutputFile { get; }
        WorkingDirectory WorkingDirectory { get; }
        string XmlDbDirectory { get; }
        int TopLimit { get; }
        int MaxRows { get; }
        VAL GetValue(VAR variable);
        T GetValue<T>(string variable, T defaultValue = default);
    }
}