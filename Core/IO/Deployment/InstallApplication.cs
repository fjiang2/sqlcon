using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Sys.IO
{
    public class InstallApplication : Deployment
    {
        public InstallApplication(string BUILDSRC, string MAINDIR)
            : base(BUILDSRC, MAINDIR)
        {
            this.FileSearchPatterns.AddRange(new string[] { "*.dll", "*.exe", "*.config", "*.cfg" });
            this.ExclusiveFilePatterns.AddRange(new string[] { "*.vshost.exe*" });
            this.ExclusiveDirectories.AddRange(new string[] { "obj" });
        }

        public void Install(string publishDirectory, string applicationName = null)
        {
            string src = $@"{SRC}\{publishDirectory}";
            string dest = DEST;
            if (!string.IsNullOrEmpty(applicationName))
                dest = Path.Combine(dest, applicationName);

            CopyDirectory(src, dest);
        }
    }

   
}
