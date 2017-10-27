using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Sys.IO
{
    public class InstallSourceCode : Deployment
    {
        public InstallSourceCode(string BUILDSRC, string MAINDIR)
            : base(BUILDSRC, MAINDIR)
        {
            this.AllDirectories = true;
            this.FileSearchPatterns.AddRange(new string[] { "*.cs", "*.config", "*.png", "*.ico", "*.xaml", "*.cfg", "*.sln", "*.csproj", "*.dll" });
            this.ExclusiveFilePatterns.AddRange(new string[] { "packages.config" });
            this.ExclusiveDirectories.AddRange(new string[] { "bin", "obj" });
        }

        /// <summary>
        /// default installation for source code
        /// </summary>
        /// <param name="projectDirectory"></param>
        /// <param name="applicationName"></param>
        public void Install(string projectDirectory, string applicationName)
        {
            string src = $@"{SRC}\{projectDirectory}";
            string dest = DEST;
            if (!string.IsNullOrEmpty(applicationName))
                dest = Path.Combine(dest, applicationName);

            CopyDirectory(src, dest);
        }
    }
}
