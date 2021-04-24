using System.Collections.Generic;
using Sys.Data;

namespace Sys.Data
{
    public interface IConnectionConfiguration
    {
        string DefaultServerPath { get; }
        string Home { get; }
        List<ConnectionProvider> Providers { get; }
        ConnectionProvider GetProvider(string path);
    }
}