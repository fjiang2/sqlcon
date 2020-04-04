using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;
using System.Reflection;

namespace Sys.IO
{

    public class FileName
    {
        private readonly string fullPath;

        public FileName(string fullPath)
        {
            this.fullPath = fullPath;
        }

        public FileName(string directory, string relativePath)
        {
            if (Path.IsPathRooted(relativePath))
                throw new Exception("invalid relative path");

            this.fullPath = Path.Combine(directory, relativePath);
        }

        public bool Exists => File.Exists(fullPath);

        public string Name => Path.GetFileNameWithoutExtension(fullPath);

        public string ShortName => Path.GetFileName(fullPath);

        public string Extension => Path.GetExtension(fullPath);

        public string DirectoryName => Path.GetDirectoryName(fullPath);

        public string FullPath => Path.GetFullPath(fullPath);


        public void CreateDiretoryIfNotExists()
        {
            string directory = Path.GetDirectoryName(fullPath);
            if (!Directory.Exists(directory))
                Directory.CreateDirectory(directory);
        }

        public void CopyFrom(FileName fileName, bool overwrite)
        {
            this.CreateDiretoryIfNotExists();

            string from = fileName.FullPath;
            string to = this.FullPath;

            if (overwrite)
            {
                if (File.Exists(to))
                    File.Delete(to);
            }

            File.Copy(from, to);
        }


        public void CopyTo(FileName fileName, bool overwrite)
        {
            fileName.CopyFrom(this, overwrite);
        }

        public void CopyFromExecutingPath(string fileName)
        {
            FileName from = new FileName(Path.Combine(ExecutingDirectory, fileName));
            CopyFrom(from, false);
        }

        public static string ExecutingDirectory
        {
            get
            {
                Assembly assembly = Assembly.GetExecutingAssembly();
                Uri uri = new Uri(Path.GetDirectoryName(assembly.GetName().CodeBase));
                return uri.LocalPath;
            }
        }

        public static string CurrentDirectory => Path.GetFullPath(".");

        public override string ToString()
        {
            return this.fullPath;
        }

    }
}
