using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;


namespace Sys.IO
{
    public class Installation
    {
        //the files being copied with search patterns
        public string[] FileSearchPatterns { get; set; } = new string[] { };

        //the files or file patterns are not copied
        public string[] ExclusiveFilePatterns { get; set; } = new string[] { };

        //the directories are not copied
        public string[] ExclusiveDirectories { get; set; } = new string[] { };


        public Installation()
        {
        }


        private static string[] GetFiles(string directory, string[] searchPatterns)
        {

            if (searchPatterns.Length == 0)
                return Directory.GetFiles(directory);

            List<string> files = new List<string>();
            foreach (var searchPattern in searchPatterns)
                files.AddRange(Directory.GetFiles(directory, searchPattern));

            return files.ToArray();
        }

        /// <summary>
        /// get files in the directory
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        private string[] GetFiles(string directory)
        {
            string[] files = GetFiles(directory, FileSearchPatterns);

            List<string> L = new List<string>();
            foreach (string file in files)
            {
                string name = Path.GetFileName(file);
                if (ExclusiveFilePatterns.Any(pattern => pattern.IsMatch(name)))
                    continue;

                L.Add(file);
            }

            return L.ToArray();
        }

        /// <summary>
        /// delete files in the directory
        /// </summary>
        /// <param name="directory"></param>
        private void DeleteFiles(string directory)
        {
            string[] files = Directory.GetFiles(directory);
            foreach (string file in files)
                File.Delete(file);
        }


        /// <summary>
        /// Copy files on directory and sub-directories
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        public void CopyAllDirectories(string src, string dest, IProgress<string> progress)
        {
            int count = 0;
            CopyDirectory(src, dest, progress);
            count++;

            string[] directories = Directory.GetDirectories(src);
            foreach (string directory in directories)
            {
                string folder = Path.GetFileName(directory);
                if (ExclusiveDirectories.Contains(folder))
                    continue;

                CopyAllDirectories($"{src}\\{folder}", $"{dest}\\{folder}", progress);
                count++;
            }

            progress.Report($"{count} directories copied");
        }


        /// <summary>
        /// copy directory files, files on sub-directory is not copied
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        public void CopyDirectory(string src, string dest, IProgress<string> progress)
        {
            if (!Directory.Exists(dest))
                Directory.CreateDirectory(dest);
            else
                DeleteFiles(dest);

            progress.Report($"{src} -> {dest}");

            int count = 0;
            string[] files = GetFiles(src);
            foreach (var file in files)
            {
                string name = Path.GetFileName(file);

                progress.Report($"copying {name}");
                File.Copy(file, $"{dest}\\{name}", true);
                count++;
            }

            progress.Report($"{count} file(s) copied");
        }

        //dest could be file name or directory name
        public static void CopyFile(string srcFile, string dest)
        {
            string path = Path.GetFullPath(dest);
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);

            string destFile = dest;
            if (path == dest)
            {
                string name = Path.GetFileName(srcFile);
                destFile = $"{path}\\{name}";
            }

            File.Copy(srcFile, destFile, true);
        }
    }
}
