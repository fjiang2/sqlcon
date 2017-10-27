using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace Sys.IO
{
    public class Deployment
    {
        protected string SRC;
        protected string DEST;

        public TextWriter Out { get; set; } = Console.Out;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="BUILDSRC">build directory</param>
        /// <param name="DEST">install directory</param>
        /// <param name="suite">product name or software suite name</param>
        /// <param name="version"></param>
        public Deployment(string BUILDSRC, string DEST, string suite, string version)
        {
            this.SRC = BUILDSRC;
            this.DEST = $@"{DEST}\{suite}\{version}";
        }

        /// <summary>
        /// if true, copy directory and all sub-directories
        /// </summary>
        public bool AllDirectory { get; set; } = true;

        /// <summary>
        /// file extension names
        /// </summary>
        public string[] Extensions { get; set; } = new string[] { };

        /// <summary>
        /// exclusive file patterns, such as *.vshost.exe
        /// </summary>
        public string[] ExclusiveFilePatterns { get; set; } = new string[] { };

        /// <summary>
        /// directories not included to copy
        /// </summary>
        public string[] ExclusiveDirectories { get; set; } = new string[] { };


        /// <summary>
        /// copy files from one directory to another
        /// </summary>
        /// <param name="src">source directory</param>
        /// <param name="dest">destination directory</param>
        public void CopyDirectory(string src, string dest)
        {

            var installation = new Installation(Out)
            {
                Extensions = Extensions,
                ExclusiveFilePatterns = ExclusiveFilePatterns,
                ExclusiveDirectories = ExclusiveDirectories
            };

            if (AllDirectory)
                installation.CopyAllDirectory(src, dest);
            else
                installation.CopyDirectory(src, dest);

        }

        /// <summary>
        /// default installation for source code
        /// </summary>
        /// <param name="projectDirectory"></param>
        /// <param name="applicationName"></param>
        public void InstallCode(string projectDirectory, string applicationName)
        {
            //C# source code files
            AllDirectory = true;
            Extensions = new string[] { "*.cs", "*.config", "*.png", "*.ico", "*.xaml", "*.cfg", "*.sln", "*.csproj", "*.dll" };
            ExclusiveFilePatterns = new string[] { "packages.config" };
            ExclusiveDirectories = new string[] { "bin", "obj" };

            string src = $@"{SRC}\{projectDirectory}";
            string dest = $@"{DEST}\{applicationName}";
            CopyDirectory(src, dest);
        }


        /// <summary>
        /// default installation for application
        /// </summary>
        /// <param name="projectDirectory"></param>
        /// <param name="applicationName"></param>
        /// <param name="configuration"></param>
        public void InstallApplication(string projectDirectory, string applicationName, string configuration = "Release")
        {
            AllDirectory = false;
            Extensions = new string[] { "*.dll", "*.exe", "*.config", "*.cfg", "*.sql" };
            ExclusiveFilePatterns = new string[] { "*.vshost.exe" };
            ExclusiveDirectories = new string[] { "bin", "obj" };

            string src = $@"{SRC}\{projectDirectory}\bin\{configuration}";
            string dest = $@"{DEST}\{applicationName}";
            CopyDirectory(src, dest);
        }
    }
}
