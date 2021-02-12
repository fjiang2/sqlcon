using System.Globalization;

namespace Sys.Data.Resource
{
    public interface IResourceFile
    {
        CultureInfo CultureInfo { get; }
        string FullName { get; }
    }
}