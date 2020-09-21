using System.Collections.Generic;
using Sys.Data;

namespace Sys
{
    public interface IConnectionConfiguration
    {
        string DefaultServerPath { get; }
        string Home { get; }
        List<ConnectionProvider> Providers { get; }
        ConnectionProvider GetProvider(string path);
    }
}