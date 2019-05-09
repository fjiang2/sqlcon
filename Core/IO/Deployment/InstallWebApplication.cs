using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Sys.IO
{
    public class InstallWebApplication : Deployment
    {
        public InstallWebApplication(string BUILDSRC, string MAINDIR)
            : base(BUILDSRC, MAINDIR)
        {
            base.AllDirectories = true;
            this.FileSearchPatterns.AddRange(new string[] { "*.*" });
            this.ExclusiveFilePatterns.AddRange(new string[] { });
            this.ExclusiveDirectories.AddRange(new string[] { });
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
