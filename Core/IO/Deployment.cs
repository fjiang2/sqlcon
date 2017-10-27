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
        private string version;
        protected string SRC;
        protected string GA;

        public TextWriter Out { get; set; } = Console.Out;


        public Deployment(string product, string version, string SRC, string GA)
        {
            this.version = version;
            this.SRC = SRC;
            this.GA = $@"{GA}\{product}\{version}";
        }

        public bool AllDirectory { get; set; } = true;
        public string[] Extensions { get; set; } = new string[] { };
        public string[] ExclusiveFilePatterns { get; set; } = new string[] { };
        public string[] ExclusiveDirectories { get; set; } = new string[] { };

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
        /// 
        /// </summary>
        /// <param name="projectDirectory"></param>
        /// <param name="applicationName"></param>
        public void InstallCode(string projectDirectory, string applicationName)
        {
            //C# source code files
            AllDirectory = true;
            Extensions = new string[] { "*.cs", "*.config", "*.png", "*.ico", "*.xaml", "*.cfg", "*.sln", "*.csproj" };
            ExclusiveFilePatterns = new string[] { "packages.config" };
            ExclusiveDirectories = new string[] { "bin", "obj" };

            string src = $@"{SRC}\{projectDirectory}";
            string dest = $@"{GA}\{applicationName}";
            CopyDirectory(src, dest);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="projectDirectory"></param>
        /// <param name="applicationName"></param>
        /// <param name="configuration"></param>
        public void InstallApplication(string projectDirectory, string applicationName, string configuration = "Debug")
        {
            AllDirectory = false;
            Extensions = new string[] { "*.dll", "*.exe", "*.config", "*.cfg", "*.sql" };
            ExclusiveFilePatterns = new string[] { "*.vshost.exe" };
            ExclusiveDirectories = new string[] { "bin", "obj" };

            string src = $@"{SRC}\{projectDirectory}\bin\{configuration}";
            string dest = $@"{GA}\{applicationName}";
            CopyDirectory(src, dest);
        }
    }
}
