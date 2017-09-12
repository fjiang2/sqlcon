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
        public static string CurrentDirectory { get; private set; } = ".";

        public static void lcd(string path)
        {
            if (!Path.IsPathRooted(path))
                path = Path.Combine(CurrentDirectory, path);

            if (!Directory.Exists(path))
                stdio.Error($"path not exists {path}");
            else
                CurrentDirectory = path;
        }

        public static string GetFullPath(string path, string ext)
        {
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
    }
}
