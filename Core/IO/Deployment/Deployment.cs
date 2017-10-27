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

        protected string DEST
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
        public bool AllDirectories { get; set; } = false;

        /// <summary>
        /// file search patterns
        /// </summary>
        public List<string> FileSearchPatterns { get; } = new List<string>();

        /// <summary>
        /// exclusive file patterns, such as *.vshost.exe
        /// </summary>
        public List<string> ExclusiveFilePatterns { get; } = new List<string>();

        /// <summary>
        /// directories not included to copy
        /// </summary>
        public List<string> ExclusiveDirectories { get; } = new List<string>();


        /// <summary>
        /// copy files from one directory to another
        /// </summary>
        /// <param name="src">source directory</param>
        /// <param name="dest">destination directory</param>
        public void CopyDirectory(string src, string dest)
        {

            var installation = new Installation()
            {
                FileSearchPatterns = FileSearchPatterns.ToArray(),
                ExclusiveFilePatterns = ExclusiveFilePatterns.ToArray(),
                ExclusiveDirectories = ExclusiveDirectories.ToArray()
            };

            Progress<string> report = new Progress<string>(s => Out.WriteLine(s));

            if (AllDirectories)
                installation.CopyAllDirectories(src, dest, report);
            else
                installation.CopyDirectory(src, dest, report);

        }
       
    }
}
