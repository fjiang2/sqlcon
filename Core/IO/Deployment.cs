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
        protected string MAINDIR;


        /// <summary>
        /// product name or software suite name
        /// </summary>
        public string SuiteName { get; set; } = string.Empty;

        public string Version { get; set; } = string.Empty;

        public TextWriter Out { get; set; } = Console.Out;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="BUILDSRC">build directory</param>
        /// <param name="MAINDIR">install directory</param>
        public Deployment(string BUILDSRC, string MAINDIR)
        {
            this.SRC = BUILDSRC;
            this.MAINDIR = MAINDIR;
        }

        private string DEST
        {
            get
            {
                string path = MAINDIR;
                if (!string.IsNullOrEmpty(SuiteName))
                    path = Path.Combine(path, SuiteName);

                if (!string.IsNullOrEmpty(Version))
                    path = Path.Combine(path, Version);

                return path;
            }
        }
        /// <summary>
        /// if true, copy directory and all sub-directories, default is false
        /// </summary>
        public bool AllDirectory { get; set; } = false;

        /// <summary>
        /// file extension names
        /// </summary>
        public List<string> Extensions { get; set; } = new List<string>();

        /// <summary>
        /// exclusive file patterns, such as *.vshost.exe
        /// </summary>
        public List<string> ExclusiveFilePatterns { get; set; } = new List<string>();

        /// <summary>
        /// directories not included to copy
        /// </summary>
        public List<string> ExclusiveDirectories { get; set; } = new List<string>();


        /// <summary>
        /// copy files from one directory to another
        /// </summary>
        /// <param name="src">source directory</param>
        /// <param name="dest">destination directory</param>
        public void CopyDirectory(string src, string dest)
        {

            var installation = new Installation(Out)
            {
                Extensions = Extensions.ToArray(),
                ExclusiveFilePatterns = ExclusiveFilePatterns.ToArray(),
                ExclusiveDirectories = ExclusiveDirectories.ToArray()
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
            this.AllDirectory = true;
            this.Extensions = new List<string> { "*.cs", "*.config", "*.png", "*.ico", "*.xaml", "*.cfg", "*.sln", "*.csproj", "*.dll" };
            this.ExclusiveFilePatterns = new List<string> { "packages.config" };
            this.ExclusiveDirectories = new List<string> { "bin", "obj" };

            string src = $@"{SRC}\{projectDirectory}";
            string dest = DEST;
            if (!string.IsNullOrEmpty(applicationName))
                dest = Path.Combine(dest, applicationName);

            CopyDirectory(src, dest);
        }


        /// <summary>
        /// default installation for application
        /// </summary>
        /// <param name="publishDirectory"></param>
        /// <param name="applicationName"></param>
        /// <param name="allDirectory">install current directory and sub-directories</param>
        public void InstallApplication(string publishDirectory, string applicationName = null, bool allDirectory = false)
        {
            this.AllDirectory = allDirectory;
            this.Extensions = new List<string> { "*.dll", "*.exe", "*.config", "*.cfg", "*.sql" };
            this.ExclusiveFilePatterns = new List<string> { "*.vshost.exe" };
            this.ExclusiveDirectories = new List<string> { "bin", "obj" };

            string src = $@"{SRC}\{publishDirectory}";
            string dest = DEST;
            if (!string.IsNullOrEmpty(applicationName))
                dest = Path.Combine(dest, applicationName);

            CopyDirectory(src, dest);
        }
    }
}
