using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

namespace sqlcon
{
    class WorkingDirectory
    {
        public string CurrentDirectory { get; private set; } = Path.GetFullPath(".");

        public WorkingDirectory()
        {
        }

        public void SetCurrentDirectory(string currentDirectory)
        {
            this.CurrentDirectory = Path.GetFullPath(currentDirectory);
        }

        public void ChangeDirectory(string path)
        {
            if (!Path.IsPathRooted(path))
                path = Path.Combine(CurrentDirectory, path);

            if (!Directory.Exists(path))
                cerr.WriteLine($"path not exists {path}");
            else
                CurrentDirectory = Path.GetFullPath(path);
        }

        public string GetFullPath(string path, string ext)
        {
            if (path.StartsWith(@".\") || path.StartsWith(@"..\"))
            {
                return path;
            }

            if (!Path.IsPathRooted(path))
            {
                path = Path.Combine(CurrentDirectory, path);
            }

            string _ext = Path.GetExtension(path);

            //if no extension file defined
            if (_ext == string.Empty)
            {
                path = $"{path}{ext}";
            }

            return Path.GetFullPath(path);
        }

        public string[] ReadAllLines(string path)
        {
            path = GetFullPath(path, string.Empty);
            if (File.Exists(path))
                return File.ReadAllLines(path);
            else
                cout.WriteLine($"file not found {path}");

            return null;
        }

        public void ShowCurrentDirectory(string path)
        {
            const string DIR = "<DIR>";

            if (string.IsNullOrEmpty(path))
                path = CurrentDirectory;
            else if (!Path.IsPathRooted(path))
                path = Path.Combine(CurrentDirectory, path);


            if (Directory.Exists(path))
            {
                cout.WriteLine($"Directory of {path}\n");
                var directories = Directory.GetDirectories(path).OrderBy(x => x);
                foreach (string directory in directories)
                {
                    var directoryInfo = new DirectoryInfo(directory);
                    cout.WriteLine($"{directoryInfo.LastWriteTime,24}{DIR,20} {directoryInfo.Name,-30}");
                }

                var files = Directory.GetFiles(path).OrderBy(x => x);
                foreach (string file in files)
                {
                    var fileInfo = new FileInfo(file);
                    cout.WriteLine($"{fileInfo.LastWriteTime,24}{fileInfo.Length,20} {fileInfo.Name,-30}");
                }
            }
            else
            {
                cerr.WriteLine("directory not exists");
            }
        }
    }
}
