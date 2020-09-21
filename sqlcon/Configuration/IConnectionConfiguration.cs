using System.Collections.Generic;
using Sys.Data;

namespace sqlcon
{
    public interface IConnectionConfiguration
    {
        string DefaultServerPath { get; }
        string Home { get; }
        List<ConnectionProvider> Providers { get; }
        ConnectionProvider GetProvider(string path);
    }
}