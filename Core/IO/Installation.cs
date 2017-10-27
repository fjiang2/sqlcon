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
        //the files being copied with extension names 
        public string[] Extensions { get; set; } = new string[] { };

        //the files or file patterns are not copied
        public string[] ExclusiveFilePatterns { get; set; } = new string[] { };

        //the directories are not copied
        public string[] ExclusiveDirectories { get; set; } = new string[] { };

        public TextWriter Out { get; }

        public Installation(TextWriter Out)
        {
            this.Out = Out;
        }

        /// <summary>
        /// get files in the directory
        /// </summary>
        /// <param name="directory"></param>
        /// <returns></returns>
        private string[] GetFiles(string directory)
        {
            if (Extensions.Length == 0)
                return Directory.GetFiles(directory);

            List<string> files = new List<string>();
            foreach (var ext in Extensions)
            {
                files.AddRange(Directory.GetFiles(directory, ext));
            }

            return files.ToArray();
        }

        /// <summary>
        /// delete files in the directory
        /// </summary>
        /// <param name="directory"></param>
        private void DeleteFiles(string directory)
        {
            string[] files = GetFiles(directory);
            foreach (string file in files)
                File.Delete(file);
        }


        /// <summary>
        /// Copy files on directory and sub-directories
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        public void CopyAllDirectory(string src, string dest)
        {
            int count = 0;
            CopyDirectory(src, dest);
            count++;

            string[] directories = Directory.GetDirectories(src);
            foreach (string directory in directories)
            {
                string folder = Path.GetFileName(directory);
                if (ExclusiveDirectories.Contains(folder))
                    continue;

                CopyAllDirectory($"{src}\\{folder}", $"{dest}\\{folder}");
                count++;
            }

            Out.WriteLine($"{count} directories copied");
            Out.WriteLine();
        }


        /// <summary>
        /// copy directory files, files on sub-directory is not copied
        /// </summary>
        /// <param name="src"></param>
        /// <param name="dest"></param>
        public void CopyDirectory(string src, string dest)
        {
            if (!Directory.Exists(dest))
                Directory.CreateDirectory(dest);
            else
                DeleteFiles(dest);

            Out.WriteLine($"{src} -> {dest}");

            int count = 0;
            string[] files = GetFiles(src);
            foreach (var file in files)
            {
                string name = Path.GetFileName(file);

                if (ExclusiveFilePatterns.Any(pattern => pattern.IsMatch(name)))
                    continue;

                Out.WriteLine($"copying {name}");
                File.Copy(file, $"{dest}\\{name}", true);
                count++;
            }

            Out.WriteLine($"{count} file(s) copied");
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
